using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.ChangePassword
{
    public class ChangePasswordCommandHandler : IChangePasswordCommandHandler
    {
        private readonly IValidator<ChangePasswordCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher passwordHasher;
        private readonly IJwtTokenGenerator tokenGenerator;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public ChangePasswordCommandHandler(
            IValidator<ChangePasswordCommand> validator,
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.tokenGenerator = tokenGenerator;
            this.clock = clock;
            this.events = events;
        }

        public async Task<ChangePasswordResult> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return ChangePasswordResult.Failed(errors);
            }

            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return ChangePasswordResult.Failed("User not found.");
            }

            if (!passwordHasher.Verify(command.CurrentPassword, user.Password, user.Salt))
            {
                return ChangePasswordResult.Failed("The current password is incorrect.");
            }

            var passwordHash = passwordHasher.Hash(command.NewPassword);

            user.Password = passwordHash.Hash;
            user.Salt = passwordHash.Salt;
            // A pending reset link would otherwise still be able to overwrite the new password.
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresOn = null;
            // Signs out every other device, which is the point if the old password leaked.
            user.SecurityStamp = Guid.NewGuid();

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserPasswordChanged(user.Id, user.Name, user.Email, clock.UtcNow), cancellationToken);

            var token = tokenGenerator.GenerateToken(user);

            return ChangePasswordResult.Succeeded(user.Id, token.Value, token.ExpiresOnUtc);
        }
    }
}
