using MQTTnet;

namespace HomeClimate.Api.Mqtt;

public record MqttSettings(string Host, int Port, string Username, string Password)
{
    // Build per client: each Build() picks a new client id, and two clients sharing one kick each other off
    public MqttClientOptions ClientOptions() => new MqttClientOptionsBuilder()
        .WithTcpServer(Host, Port)
        .WithCredentials(Username, Password)
        .Build();
}
