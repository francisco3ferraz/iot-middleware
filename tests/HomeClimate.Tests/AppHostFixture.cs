using System.Net.Http.Json;
using Aspire.Hosting;
using MQTTnet;

namespace HomeClimate.Tests;

public sealed class AppHostFixture : IAsyncLifetime
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "test-password";
    // tools/hash-password.cs output for AdminPassword
    private const string AdminPasswordHash =
        "AQAAAAIAAYagAAAAEIzq2o8ZekaMLGF59XHc84QIwTA6TZ4eWNgVXz1lFn354CTImsl/ClWGTitky051DA==";

    public DistributedApplication App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.HomeClimate_AppHost>(
            [
                $"Parameters:admin-username={AdminUsername}",
                $"Parameters:admin-password-hash={AdminPasswordHash}",
            ],
            cts.Token);
        App = await appHost.BuildAsync(cts.Token);
        await App.StartAsync(cts.Token);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("api", cts.Token);
    }

    public async Task DisposeAsync() => await App.DisposeAsync();

    // App.CreateHttpClient pools one handler, so one login's cookie would reach every test
    public HttpClient CreateClient() => new() { BaseAddress = App.GetEndpoint("api", "http") };

    public static Task<HttpResponseMessage> LoginAsync(HttpClient client) =>
        client.PostAsJsonAsync("/api/login", new { Username = AdminUsername, Password = AdminPassword });

    public async Task<HttpClient> CreateLoggedInClientAsync()
    {
        var client = CreateClient();
        (await LoginAsync(client)).EnsureSuccessStatusCode();
        return client;
    }

    public MqttClientOptionsBuilder BrokerOptions()
    {
        var endpoint = App.GetEndpoint("mosquitto", "mqtt");
        return new MqttClientOptionsBuilder().WithTcpServer(endpoint.Host, endpoint.Port);
    }

    public async Task<IMqttClient> ConnectMqttAsync(string username, string passwordParameter)
    {
        var password = await App.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<ParameterResource>().Single(r => r.Name == passwordParameter)
            .GetValueAsync(CancellationToken.None);
        var client = new MqttClientFactory().CreateMqttClient();
        var result = await client.ConnectAsync(BrokerOptions().WithCredentials(username, password).Build());
        Assert.Equal(MqttClientConnectResultCode.Success, result.ResultCode);
        return client;
    }
}

[CollectionDefinition(Name)]
public class AppHostCollection : ICollectionFixture<AppHostFixture>
{
    public const string Name = "AppHost";
}