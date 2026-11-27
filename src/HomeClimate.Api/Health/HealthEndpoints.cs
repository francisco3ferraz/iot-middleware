using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HomeClimate.Api.Health;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/health", async (HealthCheckService health, CancellationToken ct) =>
        {
            var report = await health.CheckHealthAsync(check => check.Tags.Contains("api"), ct);
            return new { Checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()) };
        });
    }
}
