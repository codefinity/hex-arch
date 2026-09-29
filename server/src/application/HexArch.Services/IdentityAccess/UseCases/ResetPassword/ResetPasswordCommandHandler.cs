using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.ResetPassword
{
    public class ResetPasswordCommandHandler : IResetPasswordCommandHandler
    {
        private const string InvalidToken = "The reset token is invalid or has expired.";

        private readonly IValidator<ResetPasswordCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly ISecureTokenGenerator tokenGenerator;
        private readonly IPasswordHasher passwordHasher;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public ResetPasswordCommandHandler(
            IValidator<ResetPasswordCommand> validator,
            IUserRepository userRepository,
            ISecureTokenGenerator tokenGenerator,
            IPasswordHasher passwordHasher,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.tokenGenerator = tokenGenerator;
            this.passwordHasher = passwordHasher;
            this.clock = clock;
            this.events = events;
        }

        public async Task<ResetPasswordResult> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return ResetPasswordResult.Failed(errors);
            }

            // One message for every failure, so a caller learns nothing about which part was wrong.
            var user = await userRepository.GetUser(command.Email);
            if (user is null
                || !user.Active
                || user.PasswordResetTokenHash is null
                || user.PasswordResetTokenExpiresOn is null
                || user.PasswordResetTokenExpiresOn <= clock.UtcNow
                || tokenGenerator.Hash(command.Token) != user.PasswordResetTokenHash)
            {
                return ResetPasswordResult.Failed(InvalidToken);
            }

            var passwordHash = passwordHasher.Hash(command.NewPassword);

            user.Password = passwordHash.Hash;
            user.Salt = passwordHash.Salt;
            // Single use.
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresOn = null;
            // Proving control of the mailbox is as good as knowing the password, so clear any lockout.
            user.FailedSignInCount = 0;
            user.LockedOutUntil = null;
            user.SecurityStamp = Guid.NewGuid();

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserPasswordChanged(user.Id, user.Name, user.Email, clock.UtcNow), cancellationToken);

            return ResetPasswordResult.Succeeded(user.Id);
        }
    }
}
