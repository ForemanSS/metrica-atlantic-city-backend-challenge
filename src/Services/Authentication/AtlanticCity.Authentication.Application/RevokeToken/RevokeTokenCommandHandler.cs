using AtlanticCity.Authentication.Application.Security;
using AtlanticCity.Authentication.Application.Tokens;

namespace AtlanticCity.Authentication.Application.RevokeToken;

public sealed class RevokeTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenService refreshTokenService)
{
    public async Task<bool> HandleAsync(
        RevokeTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(
                command.RefreshToken))
        {
            return false;
        }

        var tokenHash =
            refreshTokenService.Hash(
                command.RefreshToken);

        var token =
            await refreshTokenRepository
                .FindByHashAsync(
                    tokenHash,
                    cancellationToken);

        if (token is null)
        {
            return false;
        }

        if (token.RevokedAt is not null)
        {
            return true;
        }

        token.Revoke(
            command.ClientIp);

        try
        {
            await refreshTokenRepository
                .SaveChangesAsync(
                    cancellationToken);
        }
        catch (RefreshTokenConcurrencyException)
        {
            return true;
        }

        return true;
    }
}