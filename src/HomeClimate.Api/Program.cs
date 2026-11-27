using System.Security.Claims;
using HomeClimate.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Aspire names its check "PostgreSql"; we register the same check as "db"
builder.AddNpgsqlDataSource("climate", settings => settings.DisableHealthChecks = true);
// Binding to a record throws at startup if any key is missing
builder.Services.AddSingleton(builder.Configuration.GetRequiredSection("Mqtt").Get<MqttSettings>()!);
builder.Services.AddSingleton(builder.Configuration.GetRequiredSection("Admin").Get<AdminSettings>()!);
builder.Services.AddAuthentication().AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddNpgSql(services => services.GetRequiredService<NpgsqlDataSource>(), name: "db", tags: ["api"])
    .AddCheck<BrokerHealthCheck>("broker", tags: ["api"]);

var app = builder.Build();
app.MapDefaultEndpoints();

var api = app.MapGroup("/api").RequireAuthorization();
api.MapGet("/health", async (HealthCheckService health, CancellationToken ct) =>
{
    var report = await health.CheckHealthAsync(check => check.Tags.Contains("api"), ct);
    return new { Checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()) };
});
api.MapPost("/login", (LoginRequest request, AdminSettings admin) =>
{
    var verifier = new PasswordHasher<string>().VerifyHashedPassword(request.Username, admin.PasswordHash, request.Password);
    if (request.Username != admin.Username || verifier == PasswordVerificationResult.Failed)
    {
        return TypedResults.Problem("credenciais inválidas", statusCode: 401);
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, admin.Username)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    return Results.SignIn(principal);
}).AllowAnonymous();

app.Run();