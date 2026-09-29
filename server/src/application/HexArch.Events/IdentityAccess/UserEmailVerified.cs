namespace HexArch.Events.IdentityAccess
{
    public sealed record UserEmailVerified(Guid UserId, string Email, DateTime OccurredOnUtc) : IDomainEvent;
}
