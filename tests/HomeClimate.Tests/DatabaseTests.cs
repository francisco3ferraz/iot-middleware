using Npgsql;

namespace HomeClimate.Tests;

[Collection(AppHostCollection.Name)]
public class DatabaseTests(AppHostFixture fixture)
{
    [Fact]
    public async Task Timescaledb_extension_is_installed()
    {
        var connectionString = await fixture.App.GetConnectionStringAsync("climate");
        await using var dataSource = NpgsqlDataSource.Create(connectionString!);
        await using var command = dataSource.CreateCommand(
            "SELECT extname FROM pg_extension WHERE extname = 'timescaledb'");

        var extension = await command.ExecuteScalarAsync();

        Assert.Equal("timescaledb", extension);
    }
}