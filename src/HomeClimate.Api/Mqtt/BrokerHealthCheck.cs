using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQTTnet;

namespace HomeClimate.Api.Mqtt;

public class BrokerHealthCheck(MqttSettings settings) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        using var client = new MqttClientFactory().CreateMqttClient();
        var result = await client.ConnectAsync(settings.ClientOptions(), cancellationToken);
        if (result.ResultCode != MqttClientConnectResultCode.Success)
            return HealthCheckResult.Unhealthy($"Broker refused connect: {result.ResultCode}");

        await client.DisconnectAsync(cancellationToken: cancellationToken);
        return HealthCheckResult.Healthy();
    }
}
