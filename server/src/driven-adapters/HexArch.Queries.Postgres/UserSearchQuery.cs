using Dapper;
using HexArch.Services.IdentityAccess.Ports.Output.Queries;
using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;
using Npgsql;

namespace HexArch.Queries.Postgres
{
    public sealed class UserSearchQuery : IUserSearchQuery
    {
        private const string Sql =
            "SELECT * FROM viewmodels.search_user_viewmodels(@Search, @Role, @Active, @Offset, @Limit)";

        private readonly NpgsqlDataSource dataSource;

        public UserSearchQuery(NpgsqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public async Task<UserSearchPage> SearchUsers(UserSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            var rows = (await connection.QueryAsync<UserSearchRow>(
                new CommandDefinition(Sql, criteria, cancellationToken: cancellationToken))).ToList();

            // Every row carries the same window-function total; an empty page (e.g. past the end)
            // carries none, so fall back to a count only in that case.
            var totalCount = rows.Count > 0
                ? (int)rows[0].TotalCount
                : await CountAll(connection, criteria, cancellationToken);

            var users = rows.Select(row => new UserSummaryReadModel(
                row.UserId, row.Name, row.Email, row.Active, row.EmailVerified, row.RegisteredOn,
                row.Roles ?? Array.Empty<RoleReadModel>(), row.ClosedOn)).ToList();

            return new UserSearchPage(users, totalCount);
        }

        private static async Task<int> CountAll(NpgsqlConnection connection, UserSearchCriteria criteria, CancellationToken cancellationToken)
        {
            if (criteria.Offset == 0)
            {
                return 0;
            }

            var firstRow = await connection.QueryFirstOrDefaultAsync<UserSearchRow>(
                new CommandDefinition(Sql, criteria with { Offset = 0, Limit = 1 }, cancellationToken: cancellationToken));

            return firstRow is null ? 0 : (int)firstRow.TotalCount;
        }

        // One row of search_user_viewmodels: the summary plus the window-function total.
        private sealed class UserSearchRow
        {
            public Guid UserId { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public bool Active { get; init; }
            public bool EmailVerified { get; init; }
            public DateTime RegisteredOn { get; init; }
            public IReadOnlyList<RoleReadModel>? Roles { get; init; }
            public DateTime? ClosedOn { get; init; }
            public long TotalCount { get; init; }
        }
    }
}
