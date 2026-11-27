using System.Security.Claims;
using HomeClimate.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
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
builder.Services.AddAuthentication().AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    // .NET only skips the login redirect for JSON endpoints; the SSE stream must get a 401 too
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
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
api.MapPost("/login", Results<ProblemHttpResult, SignInHttpResult> (LoginRequest request, AdminSettings admin) =>
{
    var passwordCheck = new PasswordHasher<string>().VerifyHashedPassword(request.Username, admin.PasswordHash, request.Password);
    if (request.Username != admin.Username || passwordCheck == PasswordVerificationResult.Failed)
    {
        return TypedResults.Problem("credenciais inválidas", statusCode: 401);
    }

    return TypedResults.SignIn(new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim(ClaimTypes.Name, admin.Username)], CookieAuthenticationDefaults.AuthenticationScheme)));
}).AllowAnonymous();

app.Run();