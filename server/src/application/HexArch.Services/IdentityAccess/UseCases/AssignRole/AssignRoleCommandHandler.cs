using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.AssignRole
{
    public class AssignRoleCommandHandler : IAssignRoleCommandHandler
    {
        private readonly IValidator<AssignRoleCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public AssignRoleCommandHandler(
            IValidator<AssignRoleCommand> validator,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.roleRepository = roleRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<AssignRoleResult> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return AssignRoleResult.Failed(errors);
            }

            var user = await userRepository.GetUser(command.UserId);
            if (user is null)
            {
                return AssignRoleResult.Failed("User not found.");
            }

            if (user.ClosedOn is not null)
            {
                return AssignRoleResult.Failed("This account has been closed.");
            }

            var role = await roleRepository.GetRoleByName(command.RoleName);
            if (role is null)
            {
                return AssignRoleResult.Failed($"Role '{command.RoleName}' does not exist.");
            }

            if (user.Roles.Any(existing => existing.Id == role.Id))
            {
                return AssignRoleResult.Succeeded(user.Id);
            }

            // No security stamp rotation: a token issued before the grant merely lacks the new role,
            // which is harmless. The user picks it up on their next sign-in.
            user.Roles.Add(role);

            await userRepository.UpdateUser(user);

            await events.Dispatch(
                new UserRolesChanged(user.Id, user.Roles.Select(r => r.Name).ToArray(), clock.UtcNow), cancellationToken);

            return AssignRoleResult.Succeeded(user.Id);
        }
    }
}
