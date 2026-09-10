using AtlanticCity.Authentication.Domain.Tokens;

namespace AtlanticCity.Authentication.UnitTests;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Create_WithFutureExpiration_ShouldCreateActiveToken()
    {
        var now =
            new DateTimeOffset(
                2026,
                9,
                9,
                10,
                0,
                0,
                TimeSpan.Zero);

        var token =
            RefreshToken.Create(
                Guid.NewGuid(),
                "token-hash",
                now.AddDays(7),
                "127.0.0.1",
                now);

        Assert.True(
            token.IsActiveAt(
                now.AddMinutes(1)));
    }

    [Fact]
    public void Create_WithExpiredExpiration_ShouldThrow()
    {
        var now =
            DateTimeOffset.UtcNow;

        Assert.Throws<
            ArgumentOutOfRangeException>(
            () =>
                RefreshToken.Create(
                    Guid.NewGuid(),
                    "token-hash",
                    now.AddMinutes(-1),
                    createdAt: now));
    }

    [Fact]
    public void Revoke_ShouldDeactivateToken()
    {
        var now =
            DateTimeOffset.UtcNow;

        var replacementId =
            Guid.NewGuid();

        var token =
            RefreshToken.Create(
                Guid.NewGuid(),
                "token-hash",
                now.AddDays(7),
                createdAt: now);

        token.Revoke(
            replacedByTokenId:
                replacementId,
            revokedAt:
                now.AddMinutes(1));

        Assert.False(
            token.IsActiveAt(
                now.AddMinutes(2)));

        Assert.Equal(
            replacementId,
            token.ReplacedByTokenId);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldThrow()
    {
        var now =
            DateTimeOffset.UtcNow;

        var token =
            RefreshToken.Create(
                Guid.NewGuid(),
                "token-hash",
                now.AddDays(7),
                createdAt: now);

        token.Revoke(
            revokedAt:
                now.AddMinutes(1));

        Assert.Throws<
            InvalidOperationException>(
            () =>
                token.Revoke(
                    revokedAt:
                        now.AddMinutes(2)));
    }

    [Fact]
    public void Revoke_ShouldIncrementVersion()
    {
        var createdAt =
            DateTimeOffset.UtcNow;

        var token =
            RefreshToken.Create(
                Guid.NewGuid(),
                "token-hash",
                createdAt.AddDays(7),
                "127.0.0.1",
                createdAt);

        Assert.Equal(
            0,
            token.Version);

        token.Revoke(
            "127.0.0.1",
            revokedAt:
                createdAt.AddMinutes(1));

        Assert.Equal(
            1,
            token.Version);
    }
}