namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers
{
    /// <summary>Search matches name or email. Page is 1-based.</summary>
    public record SearchUsersQuery(
        string? Search = null,
        string? Role = null,
        bool? Active = null,
        string? SellerApplicationStatus = null,
        int Page = 1,
        int PageSize = 20);
}
