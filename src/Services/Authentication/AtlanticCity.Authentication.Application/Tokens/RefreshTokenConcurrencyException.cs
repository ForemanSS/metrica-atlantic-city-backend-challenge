namespace AtlanticCity.Authentication.Application.Tokens;

public sealed class RefreshTokenConcurrencyException
    : Exception
{
    public RefreshTokenConcurrencyException(
        Exception innerException)
        : base(
            "The refresh token was modified by another operation.",
            innerException)
    {
    }
}