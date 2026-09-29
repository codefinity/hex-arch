using HexArch.Events;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Email;
using Microsoft.Extensions.Logging;

namespace HexArch.EMail.Handlers
{
    /// <summary>Tells the account holder their password changed, so an unexpected change gets noticed.</summary>
    internal sealed class PasswordChangedEmailHandler : IEventHandler<UserPasswordChanged>
    {
        private readonly IEmailSender emailSender;
        private readonly ILogger<PasswordChangedEmailHandler> logger;

        public PasswordChangedEmailHandler(IEmailSender emailSender, ILogger<PasswordChangedEmailHandler> logger)
        {
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public async Task Handle(UserPasswordChanged domainEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new EmailMessage(
                    domainEvent.Email,
                    "Your HexArch password was changed",
                    $"Hi {domainEvent.Name},\n\nThe password for your HexArch account was changed on {domainEvent.OccurredOnUtc:u}, " +
                    "and every device was signed out.\n\nIf this wasn't you, reset your password straight away.\n\nThanks,\nThe HexArch Team");

                await emailSender.Send(message, cancellationToken);
            }
            catch (Exception exception)
            {
                // The password is already committed; failing here would return an error for a request
                // that actually succeeded. Same stance as WelcomeEmailHandler.
                logger.LogError(exception, "Failed to send password changed email for {UserId}.", domainEvent.UserId);
            }
        }
    }
}
