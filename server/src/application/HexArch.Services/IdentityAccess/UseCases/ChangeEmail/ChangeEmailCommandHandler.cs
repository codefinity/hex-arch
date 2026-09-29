using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Email;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using HexArch.Services.IdentityAccess.UseCases.RequestEmailVerification;

namespace HexArch.Services.IdentityAccess.UseCases.ChangeEmail
{
    /// <summary>
    /// Starts an email change. The address on the account only changes once the new one is
    /// verified (VerifyEmail), so a typo cannot lock the user out of their own account.
    /// </summary>
    public class ChangeEmailCommandHandler : IChangeEmailCommandHandler
    {
        private readonly IValidator<ChangeEmailCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher passwordHasher;
        private readonly ISecureTokenGenerator tokenGenerator;
        private readonly IEmailSender emailSender;
        private readonly ISystemClock clock;

        public ChangeEmailCommandHandler(
            IValidator<ChangeEmailCommand> validator,
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ISecureTokenGenerator tokenGenerator,
            IEmailSender emailSender,
            ISystemClock clock)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.tokenGenerator = tokenGenerator;
            this.emailSender = emailSender;
            this.clock = clock;
        }

        public async Task<ChangeEmailResult> Handle(ChangeEmailCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return ChangeEmailResult.Failed(errors);
            }

            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return ChangeEmailResult.Failed("User not found.");
            }

            if (!passwordHasher.Verify(command.CurrentPassword, user.Password, user.Salt))
            {
                return ChangeEmailResult.Failed("The current password is incorrect.");
            }

            if (string.Equals(command.NewEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                return ChangeEmailResult.Failed("The new email address is the same as the current one.");
            }

            if (await userRepository.GetUser(command.NewEmail) is not null)
            {
                return ChangeEmailResult.Failed("A user with this email is already registered.");
            }

            var token = tokenGenerator.Generate();
            var expiresOn = clock.UtcNow + VerificationEmail.TokenLifetime;

            user.PendingEmail = command.NewEmail;
            user.EmailVerificationTokenHash = token.Hash;
            user.EmailVerificationTokenExpiresOn = expiresOn;

            await userRepository.UpdateUser(user);

            await emailSender.Send(
                VerificationEmail.Compose(command.NewEmail, user.Name, token.Value, expiresOn),
                cancellationToken);

            return ChangeEmailResult.Succeeded(user.Id);
        }
    }
}
