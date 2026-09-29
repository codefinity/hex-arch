using HexArch.Events;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Email;
using Microsoft.Extensions.Logging;

namespace HexArch.EMail.Handlers
{
    internal sealed class WelcomeEmailHandler : IEventHandler<UserRegistered>
    {
        private readonly IEmailSender emailSender;
        private readonly ILogger<WelcomeEmailHandler> logger;

        public WelcomeEmailHandler(IEmailSender emailSender, ILogger<WelcomeEmailHandler> logger)
        {
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public async Task Handle(UserRegistered domainEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new EmailMessage(
                    domainEvent.Email,
                    "Welcome to HexArch",
                    $"Hi {domainEvent.Name},\n\nWelcome to HexArch! Your account is ready to go.\n\nThanks,\nThe HexArch Team");

                await emailSender.Send(message, cancellationToken);
            }
            catch (Exception exception)
            {
                // The user is already committed; failing here would return an error for a request
                // that actually succeeded. Same stance as UserRegisteredProjectionHandler.
                logger.LogError(exception, "Failed to send welcome email for {UserId}.", domainEvent.UserId);
            }
        }
    }
}
