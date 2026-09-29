using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HexArch.Authorization;

public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddHexArchAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        return services;
    }
}
