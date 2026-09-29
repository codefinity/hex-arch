using HexArch.EMail.Handlers;
using HexArch.Events;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexArch.EMail
{
    public static class EMailServiceExtensions
    {
        public static IServiceCollection AddHexArchEMail(this IServiceCollection services, SmtpOptions options)
        {
            services.AddSingleton(options);
            services.AddScoped<IEmailSender>(provider =>
                new SmtpEmailSender(options, provider.GetRequiredService<ILogger<SmtpEmailSender>>()));
            services.AddScoped<IEventHandler<UserRegistered>, WelcomeEmailHandler>();
            services.AddScoped<IEventHandler<UserPasswordChanged>, PasswordChangedEmailHandler>();
            return services;
        }
    }
}
