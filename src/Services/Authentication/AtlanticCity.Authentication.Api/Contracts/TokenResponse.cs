namespace AtlanticCity.Authentication.Api.Contracts;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    DateTimeOffset ExpiresAt);