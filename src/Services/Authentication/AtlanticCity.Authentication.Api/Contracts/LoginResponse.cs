namespace AtlanticCity.Authentication.Api.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthenticatedUserResponse User);

public sealed record AuthenticatedUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    IReadOnlyCollection<string> Permissions);