namespace HexArch.Events.IdentityAccess
{
    public sealed record UserRolesChanged(Guid UserId, IReadOnlyList<string> Roles, DateTime OccurredOnUtc) : IDomainEvent;
}
