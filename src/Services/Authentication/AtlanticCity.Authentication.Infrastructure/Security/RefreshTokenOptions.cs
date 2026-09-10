namespace AtlanticCity.Authentication.Infrastructure.Security;

internal sealed class RefreshTokenOptions
{
    public const string SectionName =
        "RefreshToken";

    public int LifetimeDays { get; init; } = 7;
}