namespace HexArch.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
