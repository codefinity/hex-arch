using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;

namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers
{
    public class SearchUsersResult
    {
        public bool Success { get; }
        public IReadOnlyList<UserSummaryReadModel> Users { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public IReadOnlyList<string> Errors { get; }

        private SearchUsersResult(
            bool success, IReadOnlyList<UserSummaryReadModel> users, int totalCount, int page, int pageSize, IReadOnlyList<string> errors)
        {
            Success = success;
            Users = users;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            Errors = errors;
        }

        public static SearchUsersResult Succeeded(UserSearchPage result, int page, int pageSize) =>
            new(true, result.Users, result.TotalCount, page, pageSize, Array.Empty<string>());

        public static SearchUsersResult Failed(params string[] errors) =>
            new(false, Array.Empty<UserSummaryReadModel>(), 0, 0, 0, errors);
    }
}
