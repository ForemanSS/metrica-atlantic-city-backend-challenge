using System.Security.Cryptography;
using System.Text;
using AtlanticCity.Authentication.Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AtlanticCity.Authentication.Infrastructure.Security;

internal sealed class RefreshTokenService(
    IOptions<RefreshTokenOptions> options)
    : IRefreshTokenService
{
    private readonly RefreshTokenOptions _options =
        options.Value;

    public RefreshTokenValue Generate()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        var token =
            Base64UrlEncoder.Encode(
                randomBytes);

        var tokenHash =
            Hash(token);

        var expiresAt =
            DateTimeOffset.UtcNow.AddDays(
                _options.LifetimeDays);

        return new RefreshTokenValue(
            token,
            tokenHash,
            expiresAt);
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            token);

        var tokenBytes =
            Encoding.UTF8.GetBytes(
                token);

        var hashBytes =
            SHA256.HashData(
                tokenBytes);

        return Convert
            .ToHexString(hashBytes)
            .ToLowerInvariant();
    }
}