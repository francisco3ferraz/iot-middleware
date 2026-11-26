using HomeClimate.Api;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Aspire names its check "PostgreSql"; we register the same check as "db"
builder.AddNpgsqlDataSource("climate", settings => settings.DisableHealthChecks = true);
// Binding to a record throws at startup if any Mqtt key is missing
builder.Services.AddSingleton(builder.Configuration.GetRequiredSection("Mqtt").Get<MqttSettings>()!);
builder.Services.AddHealthChecks()
    .AddNpgSql(services => services.GetRequiredService<NpgsqlDataSource>(), name: "db", tags: ["api"])
    .AddCheck<BrokerHealthCheck>("broker", tags: ["api"]);

var app = builder.Build();
app.MapDefaultEndpoints();

app.MapGet("/api/health", async (HealthCheckService health, CancellationToken ct) =>
{
    var report = await health.CheckHealthAsync(check => check.Tags.Contains("api"), ct);
    return new { Checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()) };
});

app.Run();