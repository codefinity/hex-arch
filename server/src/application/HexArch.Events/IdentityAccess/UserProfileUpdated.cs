namespace HexArch.Events.IdentityAccess
{
    public sealed record UserProfileUpdated(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
}
