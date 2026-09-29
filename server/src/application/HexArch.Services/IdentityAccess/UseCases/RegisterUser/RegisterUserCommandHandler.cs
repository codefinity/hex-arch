using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.RegisterUser
{
    public class RegisterUserCommandHandler : IRegisterUserCommandHandler
    {
        private const string DefaultRoleName = "Customer";

        private readonly IValidator<RegisterUserCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IPasswordHasher passwordHasher;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public RegisterUserCommandHandler(
            IValidator<RegisterUserCommand> validator,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.roleRepository = roleRepository;
            this.passwordHasher = passwordHasher;
            this.clock = clock;
            this.events = events;
        }

        public async Task<RegisterUserResult> Handle(RegisterUserCommand command, CancellationToken cancellationToken = default)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return RegisterUserResult.Failed(errors);
            }

            var existingUser = await userRepository.GetUser(command.Email);
            if (existingUser is not null)
            {
                return RegisterUserResult.Failed("A user with this email is already registered.");
            }

            var defaultRole = await roleRepository.GetRoleByName(DefaultRoleName);
            if (defaultRole is null)
            {
                return RegisterUserResult.Failed($"Default role '{DefaultRoleName}' is not configured.");
            }

            var passwordHash = passwordHasher.Hash(command.Password);

            var user = new User
            {
                Name = command.Name,
                Email = command.Email,
                Password = passwordHash.Hash,
                Salt = passwordHash.Salt,
                MobileNo = command.MobileNo,
                Active = true,
                RegisteredOn = clock.UtcNow,
                Roles = new List<Role> { defaultRole }
            };

            await userRepository.AddUser(user);

            // Raised only after the user is committed, so any listener projecting a read model
            // always sees durable data.
            await events.Dispatch(new UserRegistered(user.Id, user.Name, user.Email, clock.UtcNow), cancellationToken);

            return RegisterUserResult.Succeeded(user.Id);
        }
    }
}
