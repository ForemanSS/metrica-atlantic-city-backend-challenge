using AtlanticCity.Authentication.Domain.Tokens;

namespace AtlanticCity.Authentication.Application.Tokens;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RefreshToken>>
        FindActiveByUserIdAsync(
            Guid userId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default);

    void Add(
        RefreshToken refreshToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}