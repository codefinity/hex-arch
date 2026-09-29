using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using Microsoft.Extensions.DependencyInjection;

namespace HexArch.SystemClock
{
    public static class SystemClockServiceExtensions
    {
        public static IServiceCollection AddHexArchSystemClock(this IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, SystemClock>();

            return services;
        }
    }
}
