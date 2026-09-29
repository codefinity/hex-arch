using HexArch.Events;

namespace HexArch.Services.IdentityAccess.Ports.Output.Events
{
    public interface IEventDispatcher
    {
        Task Dispatch<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent;
    }
}
