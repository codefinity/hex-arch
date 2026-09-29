using HexArch.EMail.Handlers;
using HexArch.Events;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Email;
using Microsoft.Extensions.DependencyInjection;

namespace HexArch.EMail
{
    public static class EMailServiceExtensions
    {
        public static IServiceCollection AddHexArchEMail(this IServiceCollection services, SmtpOptions options)
        {
            services.AddSingleton(options);
            services.AddScoped<IEmailSender>(_ => new SmtpEmailSender(options));
            services.AddScoped<IEventHandler<UserRegistered>, WelcomeEmailHandler>();
            return services;
        }
    }
}
