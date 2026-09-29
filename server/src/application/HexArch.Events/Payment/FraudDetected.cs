namespace HexArch.Events.Payment
{
    public sealed record FraudDetected(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
}