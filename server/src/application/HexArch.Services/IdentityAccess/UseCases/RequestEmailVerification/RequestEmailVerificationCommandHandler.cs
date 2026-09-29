using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestEmailVerification;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Email;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.RequestEmailVerification
{
    /// <summary>
    /// Emails the current user a verification code for their address, or for their pending new
    /// address if they have started an email change. Calling it again replaces the previous code.
    /// </summary>
    public class RequestEmailVerificationCommandHandler : IRequestEmailVerificationCommandHandler
    {
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly ISecureTokenGenerator tokenGenerator;
        private readonly IEmailSender emailSender;
        private readonly ISystemClock clock;

        public RequestEmailVerificationCommandHandler(
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            ISecureTokenGenerator tokenGenerator,
            IEmailSender emailSender,
            ISystemClock clock)
        {
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.tokenGenerator = tokenGenerator;
            this.emailSender = emailSender;
            this.clock = clock;
        }

        public async Task<RequestEmailVerificationResult> Handle(RequestEmailVerificationCommand command, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return RequestEmailVerificationResult.Failed("User not found.");
            }

            if (user.EmailVerified && user.PendingEmail is null)
            {
                return RequestEmailVerificationResult.Failed("The email address is already verified.");
            }

            var token = tokenGenerator.Generate();
            var expiresOn = clock.UtcNow + VerificationEmail.TokenLifetime;

            user.EmailVerificationTokenHash = token.Hash;
            user.EmailVerificationTokenExpiresOn = expiresOn;

            await userRepository.UpdateUser(user);

            // Through the port directly, not an event: events are mirrored to RabbitMQ and the raw
            // token must stay in this process. A send failure propagates, since the caller is already
            // authenticated and can simply retry.
            await emailSender.Send(
                VerificationEmail.Compose(user.PendingEmail ?? user.Email, user.Name, token.Value, expiresOn),
                cancellationToken);

            return RequestEmailVerificationResult.Succeeded(user.Id);
        }
    }
}
