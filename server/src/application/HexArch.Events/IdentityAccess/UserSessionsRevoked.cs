namespace HexArch.Events.IdentityAccess
{
    public sealed record UserSessionsRevoked(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
}
