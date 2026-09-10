using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using AtlanticCity.Gateway.Configuration;
using AtlanticCity.Gateway.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AtlanticCity.Gateway.Extensions;

internal static class GatewayServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(
            configuration);

        services.AddGatewayAuthorization();

        services.AddGatewayRateLimiting();

        services
            .AddReverseProxy()
            .LoadFromConfig(
                configuration.GetSection(
                    "ReverseProxy"));

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions =
            configuration
                .GetSection(
                    JwtOptions.SectionName)
                .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration was not found.");

        if (string.IsNullOrWhiteSpace(
                jwtOptions.Issuer))
        {
            throw new InvalidOperationException(
                "Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(
                jwtOptions.Audience))
        {
            throw new InvalidOperationException(
                "Jwt:Audience is required.");
        }

        if (string.IsNullOrWhiteSpace(
                jwtOptions.SigningKey) ||
            Encoding.UTF8.GetByteCount(
                jwtOptions.SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must contain at least 32 bytes.");
        }

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                {
                    options.MapInboundClaims =
                        false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer =
                                jwtOptions.Issuer,

                            ValidateAudience = true,
                            ValidAudience =
                                jwtOptions.Audience,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtOptions.SigningKey)),

                            ValidateLifetime = true,

                            ClockSkew =
                                TimeSpan.FromSeconds(30),

                            NameClaimType =
                                "name",

                            RoleClaimType =
                                "role"
                        };
                });

        return services;
    }

    private static IServiceCollection AddGatewayAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(
            options =>
            {
                options.AddPolicy(
                    AuthorizationPolicies.MassLoadRead,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.RequireClaim(
                            "permission",
                            AuthorizationPolicies
                                .MassLoadReadPermission);
                    });

                options.AddPolicy(
                    AuthorizationPolicies.MassLoadExecute,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.RequireClaim(
                            "permission",
                            AuthorizationPolicies
                                .MassLoadExecutePermission);
                    });
            });

        return services;
    }

    private static IServiceCollection AddGatewayRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(
            options =>
            {
                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter =
                    PartitionedRateLimiter.Create<HttpContext, string>(
                        httpContext =>
                        {
                            var subject =
                                httpContext.User.FindFirstValue(
                                    "sub");

                            var clientIp =
                                httpContext.Connection
                                    .RemoteIpAddress?
                                    .ToString();

                            var partitionKey =
                                !string.IsNullOrWhiteSpace(subject)
                                    ? $"user:{subject}"
                                    : $"ip:{clientIp ?? "unknown"}";

                            return RateLimitPartition
                                .GetFixedWindowLimiter(
                                    partitionKey,
                                    _ =>
                                        new FixedWindowRateLimiterOptions
                                        {
                                            PermitLimit = 100,
                                            Window =
                                                TimeSpan.FromMinutes(1),
                                            QueueLimit = 0,
                                            AutoReplenishment = true
                                        });
                        });

                options.OnRejected =
                    async (context, cancellationToken) =>
                    {
                        context.HttpContext.Response.ContentType =
                            "application/problem+json";

                        await context.HttpContext.Response.WriteAsJsonAsync(
                            new
                            {
                                type =
                                    "https://httpstatuses.com/429",
                                title =
                                    "Too many requests",
                                status =
                                    StatusCodes.Status429TooManyRequests,
                                detail =
                                    "The request rate limit has been exceeded."
                            },
                            cancellationToken);
                    };
            });

        return services;
    }
}