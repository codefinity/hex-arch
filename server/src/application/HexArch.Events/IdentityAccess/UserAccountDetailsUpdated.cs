namespace HexArch.Events.IdentityAccess
{
    public sealed record UserAccountDetailsUpdated(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
}
