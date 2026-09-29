using HexArch.Events;
using HexArch.Events.IdentityAccess;
using Microsoft.Extensions.Logging;

namespace HexArch.Projections.Postgres.Handlers
{
    internal sealed class UserRegisteredProjectionHandler : IEventHandler<UserRegistered>
    {
        private readonly UserViewModelProjector projector;
        private readonly ILogger<UserRegisteredProjectionHandler> logger;

        public UserRegisteredProjectionHandler(UserViewModelProjector projector, ILogger<UserRegisteredProjectionHandler> logger)
        {
            this.projector = projector;
            this.logger = logger;
        }

        public async Task Handle(UserRegistered domainEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                await projector.Refresh(domainEvent.UserId, cancellationToken);
            }
            catch (Exception exception)
            {
                // The user is already committed; failing here would return an error for a request
                // that actually succeeded. Reconciliation repairs the row later.
                logger.LogError(exception,
                    "Failed to project user view model for {UserId}; leaving it to reconciliation.", domainEvent.UserId);
            }
        }
    }
}
