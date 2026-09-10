using System.Security.Claims;
using AtlanticCity.Gateway.Security;

namespace AtlanticCity.Gateway.Endpoints;

internal static class GatewayIdentityEndpoints
{
    public static IEndpointRouteBuilder MapGatewayIdentityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(
                "/gateway/me",
                (ClaimsPrincipal user) =>
                {
                    var permissions =
                        user.FindAll("permission")
                            .Select(x => x.Value)
                            .ToArray();

                    return Results.Ok(
                        new
                        {
                            subject =
                                user.FindFirstValue("sub"),

                            email =
                                user.FindFirstValue("email"),

                            name =
                                user.FindFirstValue("name"),

                            role =
                                user.FindFirstValue("role"),

                            permissions
                        });
                })
            .RequireAuthorization(
                AuthorizationPolicies.MassLoadRead);

        return endpoints;
    }
}