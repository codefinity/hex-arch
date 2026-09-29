using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.CloseAccount;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.CloseAccount
{
    /// <summary>
    /// Self-service account closure. Unlike deactivation it is permanent: personal data is erased
    /// and the row is kept only as an anonymous tombstone, so other contexts' references stay valid.
    /// </summary>
    public class CloseAccountCommandHandler : ICloseAccountCommandHandler
    {
        public const string ClosedAccountName = "Closed account";
        public const string DeactivationReason = "Account closed.";

        private readonly IValidator<CloseAccountCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly IProfileRepository profileRepository;
        private readonly IPasswordHasher passwordHasher;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public CloseAccountCommandHandler(
            IValidator<CloseAccountCommand> validator,
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            IProfileRepository profileRepository,
            IPasswordHasher passwordHasher,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.passwordHasher = passwordHasher;
            this.clock = clock;
            this.events = events;
        }

        public async Task<CloseAccountResult> Handle(CloseAccountCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return CloseAccountResult.Failed(errors);
            }

            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return CloseAccountResult.Failed("User not found.");
            }

            if (!passwordHasher.Verify(command.CurrentPassword, user.Password, user.Salt))
            {
                return CloseAccountResult.Failed("The current password is incorrect.");
            }

            if (user.Active && user.Roles.Any(role => role.Name == RoleNames.Admin)
                && await userRepository.CountActiveUsersInRole(RoleNames.Admin) <= 1)
            {
                return CloseAccountResult.Failed("The last active administrator account cannot be closed.");
            }

            // Profile first: the delete is idempotent, so if anonymising the user fails afterwards the
            // whole request can simply be retried.
            await profileRepository.DeleteProfile(user.Id);

            var now = clock.UtcNow;
            // Nobody knows this password, so the tombstone can never be signed into.
            var scrambled = passwordHasher.Hash(Guid.NewGuid().ToString());

            user.Name = ClosedAccountName;
            // Frees the real address for a future registration. "N" format keeps it within VARCHAR(50).
            user.Email = $"{user.Id:N}@closed.invalid";
            user.MobileNo = string.Empty;
            user.Password = scrambled.Hash;
            user.Salt = scrambled.Salt;
            user.Active = false;
            user.DeactivationReason = DeactivationReason;
            user.DeactivatedOn = now;
            user.ClosedOn = now;
            user.EmailVerified = false;
            user.PendingEmail = null;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiresOn = null;
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresOn = null;
            user.FailedSignInCount = 0;
            user.LockedOutUntil = null;
            user.SecurityStamp = Guid.NewGuid();

            await userRepository.UpdateUser(user);

            // Carries no personal data: it is mirrored onto the analytics firehose.
            await events.Dispatch(new UserAccountClosed(user.Id, now), cancellationToken);

            return CloseAccountResult.Succeeded(user.Id);
        }
    }
}
