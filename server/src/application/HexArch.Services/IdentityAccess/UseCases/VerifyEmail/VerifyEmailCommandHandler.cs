using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.VerifyEmail
{
    public class VerifyEmailCommandHandler : IVerifyEmailCommandHandler
    {
        private readonly IValidator<VerifyEmailCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly ISecureTokenGenerator tokenGenerator;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public VerifyEmailCommandHandler(
            IValidator<VerifyEmailCommand> validator,
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            ISecureTokenGenerator tokenGenerator,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.tokenGenerator = tokenGenerator;
            this.clock = clock;
            this.events = events;
        }

        public async Task<VerifyEmailResult> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return VerifyEmailResult.Failed(errors);
            }

            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return VerifyEmailResult.Failed("User not found.");
            }

            if (user.EmailVerificationTokenHash is null
                || user.EmailVerificationTokenExpiresOn is null
                || user.EmailVerificationTokenExpiresOn <= clock.UtcNow
                || tokenGenerator.Hash(command.Token) != user.EmailVerificationTokenHash)
            {
                return VerifyEmailResult.Failed("The verification token is invalid or has expired.");
            }

            if (user.PendingEmail is not null)
            {
                // Checked again here, not just when the change was requested: someone may have
                // registered the address in the meantime.
                var owner = await userRepository.GetUser(user.PendingEmail);
                if (owner is not null && owner.Id != user.Id)
                {
                    return VerifyEmailResult.Failed("A user with this email is already registered.");
                }

                user.Email = user.PendingEmail;
                user.PendingEmail = null;
            }

            user.EmailVerified = true;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiresOn = null;

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserEmailVerified(user.Id, user.Email, clock.UtcNow), cancellationToken);

            return VerifyEmailResult.Succeeded(user.Id);
        }
    }
}
