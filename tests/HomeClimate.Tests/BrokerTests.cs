using MQTTnet;

namespace HomeClimate.Tests;

[Collection(AppHostCollection.Name)]
public class BrokerTests(AppHostFixture fixture)
{
    [Fact]
    public async Task Anonymous_connect_is_refused()
    {
        using var client = new MqttClientFactory().CreateMqttClient();

        var result = await client.ConnectAsync(BrokerOptions().Build());

        Assert.Equal(MqttClientConnectResultCode.NotAuthorized, result.ResultCode);
    }

    [Fact]
    public async Task Middleware_receives_what_sensor_publishes()
    {
        using var middleware = await ConnectAsync("middleware", "mqtt-middleware-password");
        var received = new TaskCompletionSource<string>();
        middleware.ApplicationMessageReceivedAsync += e =>
        {
            received.TrySetResult(e.ApplicationMessage.ConvertPayloadToString());
            return Task.CompletedTask;
        };
        await middleware.SubscribeAsync("home/sensors/#");

        using var sensor = await ConnectAsync("sensor", "mqtt-sensor-password");
        await sensor.PublishStringAsync("home/sensors/test/meta", "hello");

        Assert.Equal("hello", await received.Task.WaitAsync(TimeSpan.FromSeconds(10)));
    }

    private MqttClientOptionsBuilder BrokerOptions()
    {
        var endpoint = fixture.App.GetEndpoint("mosquitto", "mqtt");
        return new MqttClientOptionsBuilder().WithTcpServer(endpoint.Host, endpoint.Port);
    }

    private async Task<IMqttClient> ConnectAsync(string username, string passwordParameter)
    {
        var password = await fixture.App.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<ParameterResource>().Single(r => r.Name == passwordParameter)
            .GetValueAsync(CancellationToken.None);
        var client = new MqttClientFactory().CreateMqttClient();
        var result = await client.ConnectAsync(BrokerOptions().WithCredentials(username, password).Build());
        Assert.Equal(MqttClientConnectResultCode.Success, result.ResultCode);
        return client;
    }
}