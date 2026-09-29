using Npgsql;

namespace HexArch.Projections.Postgres
{
    /// <summary>
    /// Rebuilds the denormalized user view model for one user from the write tables.
    /// Internal: only the handlers in this module call it.
    /// </summary>
    internal sealed class UserViewModelProjector
    {
        private readonly NpgsqlDataSource dataSource;

        public UserViewModelProjector(NpgsqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public async Task Refresh(Guid userId, CancellationToken cancellationToken = default)
        {
            await using var command = dataSource.CreateCommand("SELECT viewmodels.refresh_user_viewmodel($1)");
            command.Parameters.AddWithValue(userId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
