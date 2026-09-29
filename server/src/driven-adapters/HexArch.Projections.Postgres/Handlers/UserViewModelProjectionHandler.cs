using HexArch.Events;
using HexArch.Events.IdentityAccess;
using Microsoft.Extensions.Logging;

namespace HexArch.Projections.Postgres.Handlers
{
    /// <summary>
    /// Keeps the user view model current for every event after registration that changes something
    /// it shows. Each one needs the same thing - refresh one user's row - so they share a class
    /// instead of repeating UserProfileUpdatedProjectionHandler once per event.
    /// </summary>
    internal sealed class UserViewModelProjectionHandler :
        IEventHandler<UserDeactivated>,
        IEventHandler<UserReactivated>,
        IEventHandler<UserRolesChanged>,
        IEventHandler<UserAccountDetailsUpdated>,
        IEventHandler<UserEmailVerified>,
        IEventHandler<UserAccountClosed>
    {
        private readonly UserViewModelProjector projector;
        private readonly ILogger<UserViewModelProjectionHandler> logger;

        public UserViewModelProjectionHandler(UserViewModelProjector projector, ILogger<UserViewModelProjectionHandler> logger)
        {
            this.projector = projector;
            this.logger = logger;
        }

        public Task Handle(UserDeactivated domainEvent, CancellationToken cancellationToken = default) =>
            Refresh(domainEvent.UserId, cancellationToken);

        public Task Handle(UserReactivated domainEvent, CancellationToken cancellationToken = default) =>
            Refresh(domainEvent.UserId, cancellationToken);

        public Task Handle(UserRolesChanged domainEvent, CancellationToken cancellationToken = default) =>
            Refresh(domainEvent.UserId, cancellationToken);

        public Task Handle(UserAccountDetailsUpdated domainEvent, CancellationToken cancellationToken = default) =>
            Refresh(domainEvent.UserId, cancellationToken);

        public Task Handle(UserEmailVerified domainEvent, CancellationToken cancellationToken = default) =>
            Refresh(domainEvent.UserId, cancellationToken);

        public Task Handle(UserAccountClosed domainEvent, CancellationToken cancellationToken = default) =>
            Refresh(domainEvent.UserId, cancellationToken);

        private async Task Refresh(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                await projector.Refresh(userId, cancellationToken);
            }
            catch (Exception exception)
            {
                // The change is already committed; failing here would return an error for a request
                // that actually succeeded. Reconciliation repairs the row later.
                logger.LogError(exception,
                    "Failed to project user view model for {UserId}; leaving it to reconciliation.", userId);
            }
        }
    }
}
