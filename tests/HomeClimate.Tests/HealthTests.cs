using System.Net.Http.Json;

namespace HomeClimate.Tests;

[Collection(AppHostCollection.Name)]
public class HealthTests(AppHostFixture fixture)
{
    [Fact]
    public async Task Health_reports_db_and_broker_healthy()
    {
        var client = await fixture.CreateLoggedInClientAsync();
        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal(
            new Dictionary<string, string> { ["db"] = "Healthy", ["broker"] = "Healthy" },
            health!.Checks);
    }

    private record HealthResponse(Dictionary<string, string> Checks);
}