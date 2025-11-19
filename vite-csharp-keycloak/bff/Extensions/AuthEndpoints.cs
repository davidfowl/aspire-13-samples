using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Bff.Extensions;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        // Login endpoint - triggers OIDC flow
        group.MapGet("/login", async (HttpContext context, string? returnUrl = null) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? "/"
            };

            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);
        });

        // Logout endpoint - redirects to perform full logout from Keycloak
        group.MapPost("/logout", async (HttpContext context) =>
        {
            // Sign out of both cookie (local) and OIDC (Keycloak)
            // This will redirect to Keycloak's end_session_endpoint and then back to signout callback
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        });

        // Signout callback - handles the return from Keycloak after logout
        // Redirects to clean root URL without query parameters
        group.MapGet("/signout-callback-oidc", (HttpContext context) =>
        {
            return Results.Redirect("/");
        }).ExcludeFromDescription();

        // Get current user info
        group.MapGet("/user", (ClaimsPrincipal user) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Ok(new { authenticated = false });
            }

            return Results.Ok(new
            {
                authenticated = true,
                username = user.FindFirst("preferred_username")?.Value ?? user.Identity.Name,
                email = user.FindFirst("email")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value,
                firstName = user.FindFirst("given_name")?.Value ?? user.FindFirst(ClaimTypes.GivenName)?.Value,
                lastName = user.FindFirst("family_name")?.Value ?? user.FindFirst(ClaimTypes.Surname)?.Value,
                claims = user.Claims.Select(c => new { c.Type, c.Value })
            });
        });

        return app;
    }
}
