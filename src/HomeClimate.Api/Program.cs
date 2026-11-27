using HomeClimate.Api.Auth;
using HomeClimate.Api.Health;
using HomeClimate.Api.Mqtt;
using HomeClimate.Api.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Aspire names its check "PostgreSql"; we register the same check as "db"
builder.AddNpgsqlDataSource("climate", settings => settings.DisableHealthChecks = true);
builder.Services.AddDbContext<ClimateDb>((services, options) => options.UseNpgsql(services.GetRequiredService<NpgsqlDataSource>()));
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
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<ClimateDb>().Database.MigrateAsync();
app.MapDefaultEndpoints();

var api = app.MapGroup("/api").RequireAuthorization();
api.MapHealthEndpoints();
api.MapAuthEndpoints();

app.Run();