using Aspire.Hosting;

namespace HomeClimate.Tests;

public sealed class AppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.HomeClimate_AppHost>(cts.Token);
        App = await appHost.BuildAsync(cts.Token);
        await App.StartAsync(cts.Token);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("api", cts.Token);
    }

    public async Task DisposeAsync() => await App.DisposeAsync();
}

[CollectionDefinition(Name)]
public class AppHostCollection : ICollectionFixture<AppHostFixture>
{
    public const string Name = "AppHost";
}