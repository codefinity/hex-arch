using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile;
using HexArch.Services.IdentityAccess.UseCases.DeactivateUser;
using HexArch.Services.IdentityAccess.UseCases.RegisterUser;
using HexArch.Services.IdentityAccess.UseCases.ShowUserProfile;
using HexArch.Services.IdentityAccess.UseCases.SignIn;
using HexArch.Services.IdentityAccess.UseCases.UpdateProfile;
using Microsoft.Extensions.DependencyInjection;

namespace HexArch.Services
{
    public static class ServicesServiceExtensions
    {
        /// <summary>
        /// Registers HexArch.Services' own use case implementations, the same way each
        /// infrastructure adapter registers itself through its own AddHexArch* extension.
        /// </summary>
        public static IServiceCollection AddHexArchServices(this IServiceCollection services)
        {
            services.AddScoped<IRegisterUserCommandHandler, RegisterUserCommandHandler>();
            services.AddSingleton<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();

            services.AddScoped<IUpdateProfileCommandHandler, UpdateProfileCommandHandler>();
            services.AddSingleton<IValidator<UpdateProfileCommand>, UpdateProfileCommandValidator>();

            services.AddScoped<IShowUserProfileQueryHandler, ShowUserProfileQueryHandler>();

            services.AddScoped<ISignInCommandHandler, SignInCommandHandler>();
            services.AddSingleton<IValidator<SignInCommand>, SignInCommandValidator>();

            services.AddScoped<IDeactivateUserCommandHandler, DeactivateUserCommandHandler>();
            services.AddSingleton<IValidator<DeactivateUserCommand>, DeactivateUserCommandValidator>();

            return services;
        }
    }
}
