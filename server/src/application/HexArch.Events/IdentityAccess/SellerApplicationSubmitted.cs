namespace HexArch.Events.IdentityAccess
{
    public sealed record SellerApplicationSubmitted(Guid ApplicationId, Guid UserId, string BusinessName, DateTime OccurredOnUtc) : IDomainEvent;
}
