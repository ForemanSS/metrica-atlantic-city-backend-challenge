namespace AtlanticCity.Authentication.Application.RevokeToken;

public sealed record RevokeTokenCommand(
    string RefreshToken,
    string? ClientIp = null);