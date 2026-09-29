using HexArch.Services.IdentityAccess.Ports.Output.Projections;
using Npgsql;

namespace HexArch.Projections.Postgres
{
    public sealed class UserViewModelReconciler : IUserViewModelReconciler
    {
        private readonly NpgsqlDataSource dataSource;

        public UserViewModelReconciler(NpgsqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public async Task<int> Reconcile(CancellationToken cancellationToken = default)
        {
            await using var command = dataSource.CreateCommand("SELECT viewmodels.reconcile_user_viewmodels()");
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is int repaired ? repaired : 0;
        }
    }
}
