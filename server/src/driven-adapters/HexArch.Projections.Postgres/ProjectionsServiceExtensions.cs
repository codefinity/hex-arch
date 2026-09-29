using HexArch.Events;
using HexArch.Events.IdentityAccess;
using HexArch.Projections.Postgres.Handlers;
using HexArch.Services.IdentityAccess.Ports.Output.Projections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace HexArch.Projections.Postgres
{
    public static class ProjectionsServiceExtensions
    {
        public static IServiceCollection AddHexArchProjections(this IServiceCollection services, string connectionString)
        {
            // TryAdd: HexArch.Queries.Postgres registers the same NpgsqlDataSource for the
            // same connection string, and both adapters should share one pooled data source.
            services.TryAddSingleton(_ => NpgsqlDataSource.Create(connectionString));

            services.AddScoped<UserViewModelProjector>();
            services.AddScoped<IUserViewModelReconciler, UserViewModelReconciler>();

            services.AddScoped<IEventHandler<UserRegistered>, UserRegisteredProjectionHandler>();
            services.AddScoped<IEventHandler<UserProfileUpdated>, UserProfileUpdatedProjectionHandler>();

            return services;
        }
    }
}
