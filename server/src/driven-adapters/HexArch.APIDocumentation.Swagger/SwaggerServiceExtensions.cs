using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace HexArch.APIDocumentation.Swagger;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddHexArchSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "HexArch API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT access token."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }

    public static WebApplication UseHexArchSwagger(this WebApplication app)
    {
        // The end-to-end test suite runs the API as ASPNETCORE_ENVIRONMENT=IntegrationTest and
        // uses /swagger/index.html as its server-readiness probe, so serve the docs there too.
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("IntegrationTest"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}
