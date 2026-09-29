namespace HexArch.Events.Payment
{
    public sealed record FraudCleared(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
}
