namespace AtlanticCity.Authentication.Infrastructure.Initialization;

internal sealed class SeedUserOptions
{
    public const string SectionName =
        "SeedUser";

    public bool Enabled { get; init; }

    public string Email { get; init; } = null!;

    public string DisplayName { get; init; } = null!;

    public string Password { get; init; } = null!;
}