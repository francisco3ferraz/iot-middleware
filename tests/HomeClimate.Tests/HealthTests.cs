using System.Net.Http.Json;

namespace HomeClimate.Tests;

public class HealthTests
{
    [Fact]
    public async Task Health_reports_db_and_broker_healthy()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.HomeClimate_AppHost>(cts.Token);
        await using var app = await appHost.BuildAsync(cts.Token);
        await app.StartAsync(cts.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("api", cts.Token);

        var response = await app.CreateHttpClient("api").GetAsync("/api/health", cts.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>(cts.Token);
        Assert.Equal(
            new Dictionary<string, string> { ["db"] = "Healthy", ["broker"] = "Healthy" },
            health!.Checks);
    }

    private record HealthResponse(Dictionary<string, string> Checks);
}