using HexArch.Persistance.Postgres.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HexArch.Persistance.Postgres
{
    public static class PersistanceServiceExtensions
    {
        public static IServiceCollection AddHexArchPersistance(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<IdentityAccessContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();

            return services;
        }
    }
}
