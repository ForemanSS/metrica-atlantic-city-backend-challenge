namespace AtlanticCity.Authentication.Application.Security;

public sealed record RefreshTokenValue(
    string Token,
    string TokenHash,
    DateTimeOffset ExpiresAt);