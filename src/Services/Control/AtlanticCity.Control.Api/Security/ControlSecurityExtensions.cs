using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AtlanticCity.Control.Api.Security;

internal static class ControlSecurityExtensions
{
    public static IServiceCollection AddControlSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer =
            configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "Jwt:Issuer was not configured.");

        var audience =
            configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "Jwt:Audience was not configured.");

        var signingKey =
            configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "Jwt:SigningKey was not configured.");

        if (Encoding.UTF8.GetByteCount(
                signingKey) < 32)
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
                            ValidIssuer = issuer,

                            ValidateAudience = true,
                            ValidAudience = audience,

                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        signingKey)),

                            ValidateLifetime = true,

                            ClockSkew =
                                TimeSpan.FromSeconds(30),

                            NameClaimType =
                                "name",

                            RoleClaimType =
                                "role"
                        };
                });

        services.AddAuthorization(
            options =>
            {
                options.AddPolicy(
                    ControlAuthorizationPolicies.MassLoadExecute,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.RequireClaim(
                            "permission",
                            ControlAuthorizationPolicies
                                .MassLoadExecutePermission);
                    });

                options.AddPolicy(
                    ControlAuthorizationPolicies.MassLoadRead,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.RequireClaim(
                            "permission",
                            ControlAuthorizationPolicies
                                .MassLoadReadPermission);
                    });
            });

        return services;
    }
}