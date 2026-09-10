using AtlanticCity.Authentication.Application.Security;
using AtlanticCity.Authentication.Application.Tokens;
using AtlanticCity.Authentication.Application.Users;
using AtlanticCity.Authentication.Domain.Tokens;

namespace AtlanticCity.Authentication.Application.RefreshTokens;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService)
{
    public async Task<RefreshTokenResult> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(
                command.RefreshToken))
        {
            return RefreshTokenResult.Invalid();
        }

        var now =
            DateTimeOffset.UtcNow;

        var tokenHash =
            refreshTokenService.Hash(
                command.RefreshToken);

        var storedToken =
            await refreshTokenRepository
                .FindByHashAsync(
                    tokenHash,
                    cancellationToken);

        if (storedToken is null)
        {
            return RefreshTokenResult.Invalid();
        }

        if (!storedToken.IsActiveAt(now))
        {
            await HandlePossibleReuseAsync(
                storedToken,
                command.ClientIp,
                now,
                cancellationToken);

            return RefreshTokenResult.Invalid();
        }

        var user =
            await userRepository.FindByIdAsync(
                storedToken.UserId,
                cancellationToken);

        if (user is null ||
            !user.IsActive)
        {
            return RefreshTokenResult.Invalid();
        }

        var newTokenValue =
            refreshTokenService.Generate();

        var replacementToken =
            RefreshToken.Create(
                user.Id,
                newTokenValue.TokenHash,
                newTokenValue.ExpiresAt,
                command.ClientIp,
                now);

        storedToken.Revoke(
            command.ClientIp,
            replacementToken.Id,
            now);

        refreshTokenRepository.Add(
            replacementToken);

        try
        {
            await refreshTokenRepository
                .SaveChangesAsync(
                    cancellationToken);
        }
        catch (RefreshTokenConcurrencyException)
        {
            return RefreshTokenResult.Invalid();
        }

        var permissions =
            PermissionCatalog.ForRole(
                user.Role);

        var accessToken =
            accessTokenService.Generate(
                user,
                permissions);

        return RefreshTokenResult.Success(
            accessToken.Token,
            newTokenValue.Token,
            accessToken.ExpiresAt);
    }

    private async Task HandlePossibleReuseAsync(
        RefreshToken token,
        string? clientIp,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (token.RevokedAt is null)
        {
            return;
        }

        var activeTokens =
            await refreshTokenRepository
                .FindActiveByUserIdAsync(
                    token.UserId,
                    now,
                    cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.Revoke(
                clientIp,
                revokedAt: now);
        }

        try
        {
            await refreshTokenRepository
                .SaveChangesAsync(
                    cancellationToken);
        }
        catch (RefreshTokenConcurrencyException)
        {
        }
    }
}