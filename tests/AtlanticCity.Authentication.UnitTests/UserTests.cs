using AtlanticCity.Authentication.Domain.Users;

namespace AtlanticCity.Authentication.UnitTests;

public sealed class UserTests
{
    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        var user =
            User.Create(
                "  ADMIN@AtlanticCity.PE ",
                "Administrator",
                "password-hash",
                UserRole.Admin);

        Assert.Equal(
            "admin@atlanticcity.pe",
            user.Email);

        Assert.True(user.IsActive);
        Assert.Equal(
            UserRole.Admin,
            user.Role);
    }

    [Fact]
    public void Deactivate_ShouldDisableUser()
    {
        var user =
            User.Create(
                "admin@atlanticcity.pe",
                "Administrator",
                "password-hash",
                UserRole.Admin);

        user.Deactivate();

        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangePasswordHash_ShouldReplaceHash()
    {
        var user =
            User.Create(
                "admin@atlanticcity.pe",
                "Administrator",
                "old-hash",
                UserRole.Admin);

        user.ChangePasswordHash(
            "new-hash");

        Assert.Equal(
            "new-hash",
            user.PasswordHash);

        Assert.NotNull(user.UpdatedAt);
    }
}