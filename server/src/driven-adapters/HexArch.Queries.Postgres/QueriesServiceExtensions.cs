using Dapper;
using HexArch.Queries.Postgres.TypeHandlers;
using HexArch.Services.IdentityAccess.Ports.Output.Queries;
using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace HexArch.Queries.Postgres
{
    public static class QueriesServiceExtensions
    {
        public static IServiceCollection AddHexArchQueries(this IServiceCollection services, string connectionString)
        {
            SqlMapper.AddTypeHandler(new JsonbTypeHandler<IReadOnlyList<RoleReadModel>>());

            services.TryAddSingleton(_ => NpgsqlDataSource.Create(connectionString));
            services.AddScoped<IUserProfileQuery, UserProfileQuery>();

            return services;
        }
    }
}
