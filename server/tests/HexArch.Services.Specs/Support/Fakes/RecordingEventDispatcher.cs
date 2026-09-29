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
            mock.Setup(dispatcher => dispatcher.Dispatch(It.IsAny<UserRegistered>(), It.IsAny<CancellationToken>()))
                .Callback<UserRegistered, CancellationToken>((domainEvent, _) => published.Add(domainEvent))
                .Returns(Task.CompletedTask);

            mock.Setup(dispatcher => dispatcher.Dispatch(It.IsAny<UserProfileUpdated>(), It.IsAny<CancellationToken>()))
                .Callback<UserProfileUpdated, CancellationToken>((domainEvent, _) => published.Add(domainEvent))
                .Returns(Task.CompletedTask);
        }

        public IEventDispatcher Object => mock.Object;
        public IReadOnlyList<IDomainEvent> Published => published;

        public TEvent Single<TEvent>() where TEvent : IDomainEvent => published.OfType<TEvent>().Single();
    }
}
