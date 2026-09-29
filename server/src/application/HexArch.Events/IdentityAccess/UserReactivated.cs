namespace HexArch.Events.IdentityAccess
{
    public sealed record UserReactivated(Guid UserId, string Reason, DateTime OccurredOnUtc) : IDomainEvent;
}
