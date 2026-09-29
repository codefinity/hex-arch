using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignOut;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.SignOut
{
    /// <summary>
    /// Signs the current user out of every device. Tokens are stateless JWTs, so revoking just the
    /// caller's token would need a server-side deny list; rotating the security stamp invalidates
    /// all of them at once instead.
    /// </summary>
    public class SignOutCommandHandler : ISignOutCommandHandler
    {
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public SignOutCommandHandler(
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<SignOutResult> Handle(SignOutCommand command, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return SignOutResult.Failed("User not found.");
            }

            user.SecurityStamp = Guid.NewGuid();

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserSessionsRevoked(user.Id, clock.UtcNow), cancellationToken);

            return SignOutResult.Succeeded(user.Id);
        }
    }
}
