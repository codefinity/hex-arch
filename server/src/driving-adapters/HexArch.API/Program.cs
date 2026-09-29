using HexArch.APIDocumentation.Swagger;
using HexArch.Authentication.Jwt;
using HexArch.Authorization;
using HexArch.EMail;
using HexArch.EventDispatching;
using HexArch.Listener.RabbitMQ;
using HexArch.Messaging.RabbitMQ;
using HexArch.Messaging.RabbitMQ.Transport;
using HexArch.Persistance.Postgres;
using HexArch.Projections.Postgres;
using HexArch.Queries.Postgres;
using HexArch.Services;
using HexArch.Services.IdentityAccess.Ports.Output.Projections;
using HexArch.SystemClock;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHexArchSwagger();


var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

var smtpOptions = builder.Configuration.GetSection("Smtp").Get<SmtpOptions>()
    ?? throw new InvalidOperationException("Smtp configuration section is missing.");

var identityAccessConnectionString = builder.Configuration.GetConnectionString("IdentityAccess")
    ?? throw new InvalidOperationException("ConnectionStrings:IdentityAccess is missing.");

var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()
    ?? throw new InvalidOperationException("RabbitMq configuration section is missing.");

// The web front end is a separate origin (Next.js on :3000), so the browser
// pre-flights every call it makes. Origins stay in configuration rather than
// in code so a deployment can change them without a rebuild.
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("Cors configuration section is missing.");

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Each project exposes its own AddHexArch* extension, so the composition root is the only
// place that knows which adapters are in play.
builder.Services
    .AddHexArchEventDispatching()
    // First event-handler adapter: the dispatcher aborts remaining handlers if one throws, and this
    // is the only side effect that leaves the process.
    .AddHexArchMessaging(rabbitMqOptions)         // driven: publishes events out to RabbitMQ
    .AddHexArchRabbitMqListener(rabbitMqOptions)  // driving: drains fraud alerts in from RabbitMQ
    .AddHexArchAuthentication(jwtOptions)         // driven: password hashing + JWT issuing
    .AddHexArchAuthorization()                    // driven: resolves the current user from the request
    .AddHexArchPersistance(identityAccessConnectionString)  // driven: write-side repositories (EF Core)
    .AddHexArchProjections(identityAccessConnectionString)  // driven: event handlers that maintain read models
    .AddHexArchQueries(identityAccessConnectionString)      // driven: read-side queries (Dapper)
    .AddHexArchSystemClock()
    .AddHexArchEMail(smtpOptions)
    .AddHexArchServices();


var app = builder.Build();

// Repairs any user view model rows missed by the live projection (e.g. seed data inserted
// directly via SQL, or a listener failure). At the current data volume this is a cheap full
// scan of users; move it to a schedule once that stops being true.
await using (var startupScope = app.Services.CreateAsyncScope())
{
    var reconciler = startupScope.ServiceProvider.GetRequiredService<IUserViewModelReconciler>();
    var repaired = await reconciler.Reconcile();
    if (repaired > 0)
    {
        app.Logger.LogInformation("Reconciled {RepairedCount} user view model row(s) on startup.", repaired);
    }
}

// Configure the HTTP request pipeline.
app.UseHexArchSwagger();

app.UseHttpsRedirection();

// Before authentication, so a pre-flight OPTIONS - which carries no bearer
// token - still gets its CORS headers instead of a 401.
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
