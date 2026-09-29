using HexArch.Events;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    public class RecordingEventDispatcher
    {
        private readonly List<IDomainEvent> published = new();
        private readonly Mock<IEventDispatcher> mock = new();

        public RecordingEventDispatcher()
        {
            // IEventDispatcher.Dispatch<TEvent> is generic, so each event type the use cases
            // dispatch needs its own closed Setup.
            Record<UserRegistered>();
            Record<UserProfileUpdated>();
            Record<UserDeactivated>();
            Record<UserReactivated>();
            Record<UserRolesChanged>();
            Record<UserSessionsRevoked>();
            Record<UserAccountDetailsUpdated>();
            Record<UserPasswordChanged>();
            Record<UserEmailVerified>();
            Record<UserLockedOut>();
            Record<UserAccountClosed>();
            Record<SellerApplicationSubmitted>();
            Record<SellerApplicationApproved>();
            Record<SellerApplicationRejected>();
        }

        public IEventDispatcher Object => mock.Object;
        public IReadOnlyList<IDomainEvent> Published => published;

        public TEvent Single<TEvent>() where TEvent : IDomainEvent => published.OfType<TEvent>().Single();

        private void Record<TEvent>() where TEvent : IDomainEvent =>
            mock.Setup(dispatcher => dispatcher.Dispatch(It.IsAny<TEvent>(), It.IsAny<CancellationToken>()))
                .Callback<TEvent, CancellationToken>((domainEvent, _) => published.Add(domainEvent))
                .Returns(Task.CompletedTask);
    }
}
