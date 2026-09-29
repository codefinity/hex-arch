namespace HexArch.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredOnUtc { get; }
    }
}
