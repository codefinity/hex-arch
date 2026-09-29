namespace HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels
{
    public sealed record UserSummaryReadModel(
        Guid UserId,
        string Name,
        string Email,
        bool Active,
        bool EmailVerified,
        DateTime RegisteredOn,
        IReadOnlyList<RoleReadModel> Roles,
        DateTime? ClosedOn);
}
