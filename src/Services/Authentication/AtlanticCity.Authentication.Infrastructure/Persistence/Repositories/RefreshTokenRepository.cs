using AtlanticCity.Authentication.Application.Tokens;
using AtlanticCity.Authentication.Domain.Tokens;
using Microsoft.EntityFrameworkCore;

namespace AtlanticCity.Authentication.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(
    AuthenticationDbContext dbContext)
    : IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefreshToken>>
        FindActiveByUserIdAsync(
            Guid userId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
    {
        return await dbContext.RefreshTokens
            .Where(
                x =>
                    x.UserId == userId &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public void Add(
        RefreshToken refreshToken)
    {
        ArgumentNullException.ThrowIfNull(
            refreshToken);

        dbContext.RefreshTokens.Add(
            refreshToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new RefreshTokenConcurrencyException(
                exception);
        }
    }
}