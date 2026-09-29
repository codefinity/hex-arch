using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.CloseAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestEmailVerification;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignOut;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession;
using HexArch.Services.IdentityAccess.UseCases.ApplyForSellerAccount;
using HexArch.Services.IdentityAccess.UseCases.ApproveSellerApplication;
using HexArch.Services.IdentityAccess.UseCases.AssignRole;
using HexArch.Services.IdentityAccess.UseCases.ChangeEmail;
using HexArch.Services.IdentityAccess.UseCases.ChangePassword;
using HexArch.Services.IdentityAccess.UseCases.CloseAccount;
using HexArch.Services.IdentityAccess.UseCases.DeactivateUser;
using HexArch.Services.IdentityAccess.UseCases.ReactivateUser;
using HexArch.Services.IdentityAccess.UseCases.RegisterUser;
using HexArch.Services.IdentityAccess.UseCases.RejectSellerApplication;
using HexArch.Services.IdentityAccess.UseCases.RequestEmailVerification;
using HexArch.Services.IdentityAccess.UseCases.RequestPasswordReset;
using HexArch.Services.IdentityAccess.UseCases.ResetPassword;
using HexArch.Services.IdentityAccess.UseCases.RevokeRole;
using HexArch.Services.IdentityAccess.UseCases.RevokeSessions;
using HexArch.Services.IdentityAccess.UseCases.SearchUsers;
using HexArch.Services.IdentityAccess.UseCases.ShowUser;
using HexArch.Services.IdentityAccess.UseCases.ShowUserProfile;
using HexArch.Services.IdentityAccess.UseCases.SignIn;
using HexArch.Services.IdentityAccess.UseCases.SignOut;
using HexArch.Services.IdentityAccess.UseCases.UpdateAccountDetails;
using HexArch.Services.IdentityAccess.UseCases.UpdateProfile;
using HexArch.Services.IdentityAccess.UseCases.ValidateSession;
using HexArch.Services.IdentityAccess.UseCases.VerifyEmail;
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

            services.AddScoped<IReactivateUserCommandHandler, ReactivateUserCommandHandler>();
            services.AddSingleton<IValidator<ReactivateUserCommand>, ReactivateUserCommandValidator>();

            services.AddScoped<IAssignRoleCommandHandler, AssignRoleCommandHandler>();
            services.AddSingleton<IValidator<AssignRoleCommand>, AssignRoleCommandValidator>();

            services.AddScoped<IRevokeRoleCommandHandler, RevokeRoleCommandHandler>();
            services.AddSingleton<IValidator<RevokeRoleCommand>, RevokeRoleCommandValidator>();

            services.AddScoped<ISignOutCommandHandler, SignOutCommandHandler>();

            services.AddScoped<IRevokeSessionsCommandHandler, RevokeSessionsCommandHandler>();
            services.AddSingleton<IValidator<RevokeSessionsCommand>, RevokeSessionsCommandValidator>();

            services.AddScoped<IValidateSessionQueryHandler, ValidateSessionQueryHandler>();

            services.AddScoped<IUpdateAccountDetailsCommandHandler, UpdateAccountDetailsCommandHandler>();
            services.AddSingleton<IValidator<UpdateAccountDetailsCommand>, UpdateAccountDetailsCommandValidator>();

            services.AddScoped<IChangePasswordCommandHandler, ChangePasswordCommandHandler>();
            services.AddSingleton<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();

            services.AddScoped<IRequestPasswordResetCommandHandler, RequestPasswordResetCommandHandler>();
            services.AddSingleton<IValidator<RequestPasswordResetCommand>, RequestPasswordResetCommandValidator>();

            services.AddScoped<IResetPasswordCommandHandler, ResetPasswordCommandHandler>();
            services.AddSingleton<IValidator<ResetPasswordCommand>, ResetPasswordCommandValidator>();

            services.AddScoped<IRequestEmailVerificationCommandHandler, RequestEmailVerificationCommandHandler>();

            services.AddScoped<IVerifyEmailCommandHandler, VerifyEmailCommandHandler>();
            services.AddSingleton<IValidator<VerifyEmailCommand>, VerifyEmailCommandValidator>();

            services.AddScoped<IChangeEmailCommandHandler, ChangeEmailCommandHandler>();
            services.AddSingleton<IValidator<ChangeEmailCommand>, ChangeEmailCommandValidator>();

            services.AddScoped<ICloseAccountCommandHandler, CloseAccountCommandHandler>();
            services.AddSingleton<IValidator<CloseAccountCommand>, CloseAccountCommandValidator>();

            services.AddScoped<ISearchUsersQueryHandler, SearchUsersQueryHandler>();
            services.AddSingleton<IValidator<SearchUsersQuery>, SearchUsersQueryValidator>();

            services.AddScoped<IShowUserQueryHandler, ShowUserQueryHandler>();
            services.AddSingleton<IValidator<ShowUserQuery>, ShowUserQueryValidator>();

            services.AddScoped<IApplyForSellerAccountCommandHandler, ApplyForSellerAccountCommandHandler>();
            services.AddSingleton<IValidator<ApplyForSellerAccountCommand>, ApplyForSellerAccountCommandValidator>();

            services.AddScoped<IApproveSellerApplicationCommandHandler, ApproveSellerApplicationCommandHandler>();
            services.AddSingleton<IValidator<ApproveSellerApplicationCommand>, ApproveSellerApplicationCommandValidator>();

            services.AddScoped<IRejectSellerApplicationCommandHandler, RejectSellerApplicationCommandHandler>();
            services.AddSingleton<IValidator<RejectSellerApplicationCommand>, RejectSellerApplicationCommandValidator>();

            return services;
        }
    }
}
