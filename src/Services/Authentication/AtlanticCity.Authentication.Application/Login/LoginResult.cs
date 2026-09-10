namespace AtlanticCity.Authentication.Application.Login;

public sealed record LoginResult(
    bool IsSuccess,
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt,
    Guid? UserId,
    string? Email,
    string? DisplayName,
    string? Role,
    IReadOnlyCollection<string> Permissions)
{
    public static LoginResult InvalidCredentials()
    {
        return new LoginResult(
            false,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
    }

    public static LoginResult Success(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        Guid userId,
        string email,
        string displayName,
        string role,
        IReadOnlyCollection<string> permissions)
    {
        return new LoginResult(
            true,
            accessToken,
            refreshToken,
            expiresAt,
            userId,
            email,
            displayName,
            role,
            permissions);
    }
}