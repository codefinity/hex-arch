namespace HexArch.Events.IdentityAccess
{
    public sealed record UserLockedOut(Guid UserId, DateTime LockedOutUntilUtc, DateTime OccurredOnUtc) : IDomainEvent;
}
