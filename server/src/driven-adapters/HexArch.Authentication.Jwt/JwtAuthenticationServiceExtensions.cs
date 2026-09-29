using System.Security.Claims;
using System.Text;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HexArch.Authentication.Jwt;

public static class JwtAuthenticationServiceExtensions
{
    public static IServiceCollection AddHexArchAuthentication(this IServiceCollection services, JwtOptions jwtOptions)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };

                options.Events = new JwtBearerEvents { OnTokenValidated = RejectRevokedSessions };
            });

        services.AddAuthorization();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator>(_ => new JwtTokenGenerator(jwtOptions));
        services.AddSingleton<ISecureTokenGenerator, Sha256SecureTokenGenerator>();

        return services;
    }

    /// <summary>
    /// A valid signature and lifetime only prove the token was issued, not that the session is still
    /// live. This asks the application whether it is, so deactivating a user, changing a password,
    /// revoking a role or signing out takes effect on the very next request rather than at expiry.
    /// Costs one user lookup per authenticated request.
    /// </summary>
    private static async Task RejectRevokedSessions(TokenValidatedContext context)
    {
        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var securityStamp = context.Principal?.FindFirstValue(HexArchClaimTypes.SecurityStamp);

        // Tokens issued before security stamps existed carry none; they are treated as revoked.
        if (!Guid.TryParse(userId, out var parsedUserId) || !Guid.TryParse(securityStamp, out var parsedStamp))
        {
            context.Fail("The token does not identify a session.");
            return;
        }

        var handler = context.HttpContext.RequestServices.GetRequiredService<IValidateSessionQueryHandler>();
        var result = await handler.Handle(
            new ValidateSessionQuery(parsedUserId, parsedStamp), context.HttpContext.RequestAborted);

        if (!result.Success)
        {
            context.Fail(string.Join(" ", result.Errors));
        }
    }
}
