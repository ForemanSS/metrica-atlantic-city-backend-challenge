namespace AtlanticCity.Gateway.Configuration;

internal sealed class JwtOptions
{
    public const string SectionName =
        "Jwt";

    public string Issuer { get; init; } =
        null!;

    public string Audience { get; init; } =
        null!;

    public string SigningKey { get; init; } =
        null!;
}