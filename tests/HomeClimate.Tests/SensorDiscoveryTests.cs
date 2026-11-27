using System.Net.Http.Json;
using MQTTnet;
using MQTTnet.Protocol;

namespace HomeClimate.Tests;

[Collection(AppHostCollection.Name)]
public class SensorDiscoveryTests(AppHostFixture fixture)
{
    [Fact]
    public async Task Retained_meta_lists_the_sensor_with_its_interval_and_metrics()
    {
        using var sensor = await fixture.ConnectMqttAsync("sensor", "mqtt-sensor-password");
        await sensor.PublishStringAsync(
            "home/sensors/co2-sister/meta",
            """{"sensorId":"co2-sister","model":"sim-co2","firmware":"1.0","intervalSeconds":30,"metrics":[{"kind":"co2","unit":"ppm"}]}""",
            MqttQualityOfServiceLevel.AtLeastOnce,
            retain: true);

        var listed = await WaitForSensorAsync(await fixture.CreateLoggedInClientAsync(), "co2-sister");

        Assert.Equal(30, listed.IntervalSeconds);
        Assert.Equal([new MetricResponse("co2", "ppm")], listed.Metrics);
    }

    // Discovery runs in the background, so the sensor shows up a moment after the publish
    private static async Task<SensorResponse> WaitForSensorAsync(HttpClient client, string id)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var sensors = await client.GetFromJsonAsync<List<SensorResponse>>("/api/sensors", timeout.Token);
            var found = sensors!.SingleOrDefault(s => s.Id == id);
            if (found is not null) return found;
            await Task.Delay(TimeSpan.FromMilliseconds(200), timeout.Token);
        }
    }

    private record MetricResponse(string Kind, string Unit);

    private record SensorResponse(string Id, int IntervalSeconds, List<MetricResponse> Metrics);
}
