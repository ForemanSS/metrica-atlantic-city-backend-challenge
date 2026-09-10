using AtlanticCity.Authentication.Domain.Users;

namespace AtlanticCity.Authentication.Application.Users;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<User?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    void Add(User user);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}