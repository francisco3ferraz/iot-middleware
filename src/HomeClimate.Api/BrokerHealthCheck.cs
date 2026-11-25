using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQTTnet;

namespace HomeClimate.Api;

public class BrokerHealthCheck(MqttSettings settings) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(settings.Host, settings.Port)
            .WithCredentials(settings.Username, settings.Password)
            .Build();
        using var client = new MqttClientFactory().CreateMqttClient();
        await client.ConnectAsync(options, cancellationToken);
        await client.DisconnectAsync(cancellationToken: cancellationToken);
        return HealthCheckResult.Healthy();
    }
}