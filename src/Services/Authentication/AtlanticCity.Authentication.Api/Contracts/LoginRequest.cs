namespace AtlanticCity.Authentication.Api.Contracts;

public sealed record LoginRequest(
    string Email,
    string Password);