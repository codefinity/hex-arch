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

            // Every event resolves the same scoped instance rather than a new one per interface.
            services.AddScoped<UserViewModelProjectionHandler>();
            services.AddScoped<IEventHandler<UserDeactivated>>(p => p.GetRequiredService<UserViewModelProjectionHandler>());
            services.AddScoped<IEventHandler<UserReactivated>>(p => p.GetRequiredService<UserViewModelProjectionHandler>());
            services.AddScoped<IEventHandler<UserRolesChanged>>(p => p.GetRequiredService<UserViewModelProjectionHandler>());
            services.AddScoped<IEventHandler<UserAccountDetailsUpdated>>(p => p.GetRequiredService<UserViewModelProjectionHandler>());
            services.AddScoped<IEventHandler<UserEmailVerified>>(p => p.GetRequiredService<UserViewModelProjectionHandler>());
            services.AddScoped<IEventHandler<UserAccountClosed>>(p => p.GetRequiredService<UserViewModelProjectionHandler>());

            return services;
        }
    }
}
