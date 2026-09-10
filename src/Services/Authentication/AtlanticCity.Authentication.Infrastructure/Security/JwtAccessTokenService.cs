using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AtlanticCity.Authentication.Application.Security;
using AtlanticCity.Authentication.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AtlanticCity.Authentication.Infrastructure.Security;

internal sealed class JwtAccessTokenService(
    IOptions<JwtOptions> options)
    : IAccessTokenService
{
    private readonly JwtOptions _options =
        options.Value;

    public AccessTokenResult Generate(
        User user,
        IReadOnlyCollection<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now =
            DateTimeOffset.UtcNow;

        var expiresAt =
            now.AddMinutes(
                _options.AccessTokenMinutes);

        var claims =
            new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new(
                    JwtRegisteredClaimNames.Email,
                    user.Email),

                new(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString("N")),

                new(
                    JwtRegisteredClaimNames.Name,
                    user.DisplayName),

                new(
                    "role",
                    user.Role.ToString())
            };

        claims.AddRange(
            permissions.Select(
                permission =>
                    new Claim(
                        "permission",
                        permission)));

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.SigningKey));

        var credentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer:
                    _options.Issuer,
                audience:
                    _options.Audience,
                claims:
                    claims,
                notBefore:
                    now.UtcDateTime,
                expires:
                    expiresAt.UtcDateTime,
                signingCredentials:
                    credentials);

        var serialized =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new AccessTokenResult(
            serialized,
            expiresAt);
    }
}