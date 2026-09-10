namespace AtlanticCity.Authentication.Domain.Tokens;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        string? createdByIp)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        Version = 0;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public long Version { get; private set; }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? createdByIp = null,
        DateTimeOffset? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            tokenHash);

        var now =
            createdAt ??
            DateTimeOffset.UtcNow;

        if (expiresAt <= now)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresAt),
                "Refresh token expiration must be in the future.");
        }

        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            tokenHash,
            now,
            expiresAt,
            createdByIp);
    }

    public bool IsActiveAt(
        DateTimeOffset instant)
    {
        return RevokedAt is null &&
               ExpiresAt > instant;
    }

    public void Revoke(
        string? revokedByIp = null,
        Guid? replacedByTokenId = null,
        DateTimeOffset? revokedAt = null)
    {
        if (RevokedAt is not null)
        {
            throw new InvalidOperationException(
                "Refresh token has already been revoked.");
        }

        RevokedAt =
            revokedAt ??
            DateTimeOffset.UtcNow;

        RevokedByIp =
            revokedByIp;

        ReplacedByTokenId =
            replacedByTokenId;

        Version++;
    }
}