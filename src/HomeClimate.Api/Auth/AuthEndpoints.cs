using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace HomeClimate.Api.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost("/login", Results<ProblemHttpResult, SignInHttpResult> (LoginRequest request, AdminSettings admin) =>
        {
            var passwordCheck = new PasswordHasher<string>().VerifyHashedPassword(request.Username, admin.PasswordHash, request.Password);
            if (request.Username != admin.Username || passwordCheck == PasswordVerificationResult.Failed)
            {
                return TypedResults.Problem("credenciais inválidas", statusCode: 401);
            }

            return TypedResults.SignIn(new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, admin.Username)], CookieAuthenticationDefaults.AuthenticationScheme)));
        }).AllowAnonymous();
    }
}
