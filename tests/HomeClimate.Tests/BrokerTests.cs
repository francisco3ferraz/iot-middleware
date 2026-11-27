using MQTTnet;
using MQTTnet.Protocol;

namespace HomeClimate.Tests;

[Collection(AppHostCollection.Name)]
public class BrokerTests(AppHostFixture fixture)
{
    [Fact]
    public async Task Anonymous_connect_is_refused()
    {
        using var client = new MqttClientFactory().CreateMqttClient();

        var result = await client.ConnectAsync(fixture.BrokerOptions().Build());

        Assert.Equal(MqttClientConnectResultCode.NotAuthorized, result.ResultCode);
    }

    [Fact]
    public async Task Middleware_receives_what_sensor_publishes()
    {
        using var middleware = await fixture.ConnectMqttAsync("middleware", "mqtt-middleware-password");
        var received = new TaskCompletionSource<string>();
        middleware.ApplicationMessageReceivedAsync += e =>
        {
            received.TrySetResult(e.ApplicationMessage.ConvertPayloadToString());
            return Task.CompletedTask;
        };
        // Other tests leave retained messages under home/sensors/#
        await middleware.SubscribeAsync("home/sensors/test/meta");

        using var sensor = await fixture.ConnectMqttAsync("sensor", "mqtt-sensor-password");
        await sensor.PublishStringAsync("home/sensors/test/meta", "hello");

        Assert.Equal("hello", await received.Task.WaitAsync(TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public async Task Middleware_cannot_publish_on_sensor_topics()
    {
        using var middleware = await fixture.ConnectMqttAsync("middleware", "mqtt-middleware-password");
        var received = new TaskCompletionSource<string>();
        middleware.ApplicationMessageReceivedAsync += e =>
        {
            received.TrySetResult(e.ApplicationMessage.ConvertPayloadToString());
            return Task.CompletedTask;
        };
        await middleware.SubscribeAsync("home/sensors/acl/meta");
        using var sensor = await fixture.ConnectMqttAsync("sensor", "mqtt-sensor-password");

        // QoS 1 waits for the broker to handle each publish, so the sensor's
        // message can only arrive first if the middleware's was dropped
        await middleware.PublishStringAsync("home/sensors/acl/meta", "from middleware", MqttQualityOfServiceLevel.AtLeastOnce);
        await sensor.PublishStringAsync("home/sensors/acl/meta", "from sensor", MqttQualityOfServiceLevel.AtLeastOnce);

        Assert.Equal("from sensor", await received.Task.WaitAsync(TimeSpan.FromSeconds(10)));
    }

}