namespace HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels
{
    // Positional: Dapper maps viewmodels.get_user_viewmodel_by_id's columns onto this constructor,
    // so the parameter order and names must match that function's RETURNS TABLE.
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
        DateTime ProjectedOn,
        bool EmailVerified,
        string? DeactivationReason,
        DateTime? DeactivatedOn,
        DateTime? ClosedOn,
        Guid? SellerApplicationId,
        string? SellerApplicationStatus,
        string? BusinessName,
        DateTime? SellerApplicationSubmittedOn);
}
