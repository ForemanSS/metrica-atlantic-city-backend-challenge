namespace AtlanticCity.Authentication.Application.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? ClientIp = null);