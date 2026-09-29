using Dapper;
using HexArch.Services.IdentityAccess.Ports.Output.Queries;
using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;
using Npgsql;

namespace HexArch.Queries.Postgres
{
    public sealed class UserProfileQuery : IUserProfileQuery
    {
        private const string Sql = "SELECT * FROM viewmodels.get_user_viewmodel_by_id(@UserId)";

        private readonly NpgsqlDataSource dataSource;

        public UserProfileQuery(NpgsqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public async Task<UserProfileReadModel?> GetUserProfile(Guid userId, CancellationToken cancellationToken = default)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            return await connection.QuerySingleOrDefaultAsync<UserProfileReadModel>(
                new CommandDefinition(Sql, new { UserId = userId }, cancellationToken: cancellationToken));
        }
    }
}
