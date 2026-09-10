using AtlanticCity.Authentication.Application.Users;
using AtlanticCity.Authentication.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AtlanticCity.Authentication.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(
    AuthenticationDbContext dbContext)
    : IUserRepository
{
    public Task<User?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .SingleOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);
    }

    public Task<User?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return dbContext.Users
            .SingleOrDefaultAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return dbContext.Users.AnyAsync(
            x => x.Email == normalizedEmail,
            cancellationToken);
    }

    public void Add(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        dbContext.Users.Add(user);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }
}