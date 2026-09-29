namespace HexArch.Events.IdentityAccess
{
    public sealed record SellerApplicationRejected(Guid ApplicationId, Guid UserId, string Reason, DateTime OccurredOnUtc) : IDomainEvent;
}
