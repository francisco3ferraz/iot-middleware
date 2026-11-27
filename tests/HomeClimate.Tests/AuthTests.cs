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

        var response = await client.PostAsJsonAsync("/api/login", new { Username = AppHostFixture.AdminUsername, Password = AppHostFixture.AdminPassword });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith(".AspNetCore.Cookies="));

        response = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
 
    [Fact]
    public async Task Login_with_wrong_password_is_unauthorized()
    {
        var response = await fixture.CreateClient()
            .PostAsJsonAsync("/api/login", new { Username = AppHostFixture.AdminUsername, Password = "wrong" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal("credenciais inválidas", login!.Detail);
    }

    private record ProblemResponse(string Detail);
}