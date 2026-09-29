namespace HexArch.Services.IdentityAccess.Ports.Output.Queries
{
    /// <summary>Every filter is optional; a null filter matches everything.</summary>
    public sealed record UserSearchCriteria(
        string? Search,
        string? Role,
        bool? Active,
        int Offset,
        int Limit);
}
