using System.Net.Http.Json;

namespace HomeClimate.Tests;

[Collection(AppHostCollection.Name)]
public class AuthTests(AppHostFixture fixture)
{
    [Fact]
    public async Task Health_without_session_is_unauthorized()
    {
        var response = await fixture.CreateClient().GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_then_health_is_ok()
    {
        var client = fixture.CreateClient();

        var response = await AppHostFixture.LoginAsync(client);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith(".AspNetCore.Cookies="));

        response = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(AppHostFixture.AdminUsername, "wrong")]
    [InlineData("someone", AppHostFixture.AdminPassword)]
    public async Task Login_with_wrong_credentials_is_unauthorized_without_saying_which(string username, string password)
    {
        var response = await fixture.CreateClient()
            .PostAsJsonAsync("/api/login", new { Username = username, Password = password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal("credenciais inválidas", problem!.Detail);
    }

    private record ProblemResponse(string Detail);
}