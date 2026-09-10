namespace AtlanticCity.Authentication.Application.Security;

public sealed record AccessTokenResult(
    string Token,
    DateTimeOffset ExpiresAt);