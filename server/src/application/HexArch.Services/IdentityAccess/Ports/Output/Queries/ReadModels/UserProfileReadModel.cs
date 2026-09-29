namespace HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels
{
    public sealed record UserProfileReadModel(
        Guid UserId,
        string Name,
        string Email,
        string MobileNo,
        bool Active,
        DateTime RegisteredOn,
        IReadOnlyList<RoleReadModel> Roles,
        string? Bio,
        string? Address,
        DateTime? DateOfBirth,
        string? AvatarUrl,
        DateTime? ProfileUpdatedOn,
        DateTime ProjectedOn);
}
