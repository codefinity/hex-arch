namespace HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels
{
    public sealed record UserSearchPage(IReadOnlyList<UserSummaryReadModel> Users, int TotalCount);
}
