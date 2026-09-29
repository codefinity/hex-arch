namespace HexArch.Events.IdentityAccess
{
    public sealed record UserRegistered(Guid UserId, string Name, string Email, DateTime OccurredOnUtc) : IDomainEvent;
}
