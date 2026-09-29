namespace HexArch.Events.IdentityAccess
{
    public sealed record UserDeactivated(Guid UserId, string Reason, DateTime OccurredOnUtc) : IDomainEvent;
}
