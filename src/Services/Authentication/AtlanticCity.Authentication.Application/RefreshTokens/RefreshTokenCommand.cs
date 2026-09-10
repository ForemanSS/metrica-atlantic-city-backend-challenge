namespace AtlanticCity.Authentication.Application.RefreshTokens;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? ClientIp = null);