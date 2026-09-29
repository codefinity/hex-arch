namespace HexArch.Events.IdentityAccess
{
    public sealed record UserPasswordChanged(Guid UserId, string Name, string Email, DateTime OccurredOnUtc) : IDomainEvent;
}
