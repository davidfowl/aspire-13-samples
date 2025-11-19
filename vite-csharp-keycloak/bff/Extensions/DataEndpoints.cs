using System.Security.Claims;

namespace Bff.Extensions;

public static class DataEndpoints
{
    public static IEndpointRouteBuilder MapDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/data").RequireAuthorization();

        // Protected endpoint that returns user-specific data
        group.MapGet("/profile", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                message = "This is protected data from the BFF",
                username = user.Identity?.Name,
                email = user.FindFirst(ClaimTypes.Email)?.Value,
                timestamp = DateTime.UtcNow
            });
        });

        return app;
    }
}
