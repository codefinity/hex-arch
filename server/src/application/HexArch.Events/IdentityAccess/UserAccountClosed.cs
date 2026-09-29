namespace HexArch.Events.IdentityAccess
{
    public sealed record UserAccountClosed(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
}
