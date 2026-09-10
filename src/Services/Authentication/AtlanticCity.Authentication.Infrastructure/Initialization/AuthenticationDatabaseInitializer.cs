using AtlanticCity.Authentication.Application.Security;
using AtlanticCity.Authentication.Application.Users;
using AtlanticCity.Authentication.Domain.Users;
using AtlanticCity.Authentication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AtlanticCity.Authentication.Infrastructure.Initialization;

internal sealed class AuthenticationDatabaseInitializer(
    AuthenticationDbContext dbContext,
    IUserRepository userRepository,
    IPasswordHashService passwordHashService,
    IOptions<SeedUserOptions> seedOptions,
    ILogger<AuthenticationDatabaseInitializer> logger)
{
    private readonly SeedUserOptions _seedOptions =
        seedOptions.Value;

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(
            cancellationToken);

        if (!_seedOptions.Enabled)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            _seedOptions.Email);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            _seedOptions.DisplayName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            _seedOptions.Password);

        var exists =
            await userRepository.ExistsByEmailAsync(
                _seedOptions.Email,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var passwordHash =
            passwordHashService.Hash(
                _seedOptions.Password);

        var user =
            User.Create(
                _seedOptions.Email,
                _seedOptions.DisplayName,
                passwordHash,
                UserRole.Admin);

        userRepository.Add(user);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        logger.LogInformation(
            "Seed authentication user created for {Email}",
            user.Email);
    }
}