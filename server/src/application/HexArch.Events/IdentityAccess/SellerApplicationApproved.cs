namespace HexArch.Events.IdentityAccess
{
    public sealed record SellerApplicationApproved(Guid ApplicationId, Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
}
