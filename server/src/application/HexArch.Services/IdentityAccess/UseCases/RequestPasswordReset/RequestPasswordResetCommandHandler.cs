using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Email;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.RequestPasswordReset
{
    public class RequestPasswordResetCommandHandler : IRequestPasswordResetCommandHandler
    {
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

        private readonly IValidator<RequestPasswordResetCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly ISecureTokenGenerator tokenGenerator;
        private readonly IEmailSender emailSender;
        private readonly ISystemClock clock;

        public RequestPasswordResetCommandHandler(
            IValidator<RequestPasswordResetCommand> validator,
            IUserRepository userRepository,
            ISecureTokenGenerator tokenGenerator,
            IEmailSender emailSender,
            ISystemClock clock)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.tokenGenerator = tokenGenerator;
            this.emailSender = emailSender;
            this.clock = clock;
        }

        public async Task<RequestPasswordResetResult> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return RequestPasswordResetResult.Failed(errors);
            }

            // Every path below returns the same result, so this endpoint cannot be used to find out
            // which emails are registered - the same stance as SignIn.
            var user = await userRepository.GetUser(command.Email);
            if (user is null || !user.Active)
            {
                return RequestPasswordResetResult.Succeeded();
            }

            var token = tokenGenerator.Generate();
            var expiresOn = clock.UtcNow + TokenLifetime;

            user.PasswordResetTokenHash = token.Hash;
            user.PasswordResetTokenExpiresOn = expiresOn;

            await userRepository.UpdateUser(user);

            // Sent through the port directly rather than from an event handler: every dispatched event
            // is also published to RabbitMQ, and the raw token must never leave this process that way.
            var message = new EmailMessage(
                user.Email,
                "Reset your HexArch password",
                $"Hi {user.Name},\n\nUse this code to reset your password:\n\n{token.Value}\n\n" +
                $"It expires at {expiresOn:u}. If you didn't ask for this, you can ignore this email.\n\nThanks,\nThe HexArch Team");

            try
            {
                await emailSender.Send(message, cancellationToken);
            }
            catch (Exception)
            {
                // Swallowed on purpose: a failure here must look identical to "no such account". The
                // email adapter logs the failure, and the user can simply ask again.
            }

            return RequestPasswordResetResult.Succeeded();
        }
    }
}
