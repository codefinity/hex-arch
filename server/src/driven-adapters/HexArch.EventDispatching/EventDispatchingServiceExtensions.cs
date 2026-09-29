using HexArch.Services.IdentityAccess.Ports.Output.Events;
using Microsoft.Extensions.DependencyInjection;

namespace HexArch.EventDispatching
{
    public static class EventDispatchingServiceExtensions
    {
        public static IServiceCollection AddHexArchEventDispatching(this IServiceCollection services)
        {
            // Scoped, so it receives the scoped provider and can reach handlers that hold a
            // Scoped DbContext - the root provider cannot resolve those.
            services.AddScoped<IEventDispatcher, ServiceProviderEventDispatcher>();

            return services;
        }
    }
}
