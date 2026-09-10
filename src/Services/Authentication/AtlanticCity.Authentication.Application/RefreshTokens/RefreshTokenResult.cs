namespace AtlanticCity.Authentication.Application.RefreshTokens;

public sealed record RefreshTokenResult(
    bool IsSuccess,
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt)
{
    public static RefreshTokenResult Invalid()
    {
        return new RefreshTokenResult(
            false,
            null,
            null,
            null);
    }

    public static RefreshTokenResult Success(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt)
    {
        return new RefreshTokenResult(
            true,
            accessToken,
            refreshToken,
            expiresAt);
    }
}