using AtlanticCity.Authentication.Application.Security;
using AtlanticCity.Authentication.Application.Tokens;
using AtlanticCity.Authentication.Application.Users;
using AtlanticCity.Authentication.Domain.Tokens;

namespace AtlanticCity.Authentication.Application.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHashService passwordHashService,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService)
{
    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password))
        {
            return LoginResult.InvalidCredentials();
        }

        var email =
            command.Email.Trim().ToLowerInvariant();

        var user =
            await userRepository.FindByEmailAsync(
                email,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            return LoginResult.InvalidCredentials();
        }

        var passwordIsValid =
            passwordHashService.Verify(
                user.PasswordHash,
                command.Password);

        if (!passwordIsValid)
        {
            return LoginResult.InvalidCredentials();
        }

        var permissions =
            PermissionCatalog.ForRole(
                user.Role);

        var accessToken =
            accessTokenService.Generate(
                user,
                permissions);

        var refreshTokenValue =
            refreshTokenService.Generate();

        var refreshToken =
            RefreshToken.Create(
                user.Id,
                refreshTokenValue.TokenHash,
                refreshTokenValue.ExpiresAt,
                command.ClientIp);

        refreshTokenRepository.Add(
            refreshToken);

        await refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        return LoginResult.Success(
            accessToken.Token,
            refreshTokenValue.Token,
            accessToken.ExpiresAt,
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString(),
            permissions);
    }
}