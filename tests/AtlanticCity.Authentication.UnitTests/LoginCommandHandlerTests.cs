using AtlanticCity.Authentication.Application.Login;
using AtlanticCity.Authentication.Application.Security;
using AtlanticCity.Authentication.Application.Tokens;
using AtlanticCity.Authentication.Application.Users;
using AtlanticCity.Authentication.Domain.Tokens;
using AtlanticCity.Authentication.Domain.Users;

namespace AtlanticCity.Authentication.UnitTests;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnAccessToken()
    {
        // Arrange
        var user =
            CreateUser();

        var handler =
            CreateHandler(
                user,
                passwordIsValid: true);

        var command =
            new LoginCommand(
                user.Email,
                "correct-password");

        // Act
        var result =
            await handler.HandleAsync(
                command);

        // Assert
        Assert.True(
            result.IsSuccess);

        Assert.Equal(
            FakeAccessTokenService.Token,
            result.AccessToken);

        Assert.Equal(
            user.Id,
            result.UserId);

        Assert.Equal(
            user.Email,
            result.Email);

        Assert.Equal(
            user.DisplayName,
            result.DisplayName);

        Assert.Equal(
            user.Role.ToString(),
            result.Role);

        Assert.Contains(
            PermissionCatalog.MassLoadExecute,
            result.Permissions);

        Assert.Contains(
            PermissionCatalog.MassLoadRead,
            result.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUser_ShouldReturnInvalidCredentials()
    {
        // Arrange
        var handler =
            CreateHandler(
                user: null,
                passwordIsValid: true);

        var command =
            new LoginCommand(
                "unknown@atlanticcity.pe",
                "password");

        // Act
        var result =
            await handler.HandleAsync(
                command);

        // Assert
        Assert.False(
            result.IsSuccess);

        Assert.Null(
            result.AccessToken);

        Assert.Null(
            result.RefreshToken);

        Assert.Null(
            result.UserId);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPassword_ShouldReturnInvalidCredentials()
    {
        // Arrange
        var user =
            CreateUser();

        var handler =
            CreateHandler(
                user,
                passwordIsValid: false);

        var command =
            new LoginCommand(
                user.Email,
                "wrong-password");

        // Act
        var result =
            await handler.HandleAsync(
                command);

        // Assert
        Assert.False(
            result.IsSuccess);

        Assert.Null(
            result.AccessToken);

        Assert.Null(
            result.RefreshToken);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ShouldReturnInvalidCredentials()
    {
        // Arrange
        var user =
            CreateUser();

        user.Deactivate();

        var handler =
            CreateHandler(
                user,
                passwordIsValid: true);

        var command =
            new LoginCommand(
                user.Email,
                "correct-password");

        // Act
        var result =
            await handler.HandleAsync(
                command);

        // Assert
        Assert.False(
            result.IsSuccess);

        Assert.Null(
            result.AccessToken);

        Assert.Null(
            result.RefreshToken);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldCreateRefreshToken()
    {
        // Arrange
        var user =
            CreateUser();

        var refreshTokenRepository =
            new FakeRefreshTokenRepository();

        var handler =
            CreateHandler(
                user,
                passwordIsValid: true,
                refreshTokenRepository);

        var command =
            new LoginCommand(
                user.Email,
                "valid-password",
                "127.0.0.1");

        // Act
        var result =
            await handler.HandleAsync(
                command);

        // Assert
        Assert.True(
            result.IsSuccess);

        Assert.Equal(
            FakeRefreshTokenService.RawToken,
            result.RefreshToken);

        var storedToken =
            Assert.Single(
                refreshTokenRepository.Tokens);

        Assert.Equal(
            user.Id,
            storedToken.UserId);

        Assert.Equal(
            FakeRefreshTokenService.TokenHash,
            storedToken.TokenHash);

        Assert.Equal(
            "127.0.0.1",
            storedToken.CreatedByIp);

        Assert.True(
            storedToken.ExpiresAt >
            storedToken.CreatedAt);

        Assert.Null(
            storedToken.RevokedAt);

        Assert.Null(
            storedToken.ReplacedByTokenId);
    }

    private static LoginCommandHandler CreateHandler(
        User? user,
        bool passwordIsValid,
        FakeRefreshTokenRepository? refreshTokenRepository = null)
    {
        return new LoginCommandHandler(
            new FakeUserRepository(user),
            refreshTokenRepository ??
                new FakeRefreshTokenRepository(),
            new FakePasswordHashService(
                passwordIsValid),
            new FakeAccessTokenService(),
            new FakeRefreshTokenService());
    }

    private static User CreateUser()
    {
        return User.Create(
            "admin@atlanticcity.pe",
            "Administrator",
            "stored-password-hash",
            UserRole.Admin);
    }

    private sealed class FakeUserRepository(
        User? user)
        : IUserRepository
    {
        public Task<User?> FindByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var foundUser =
                user?.Id == userId
                    ? user
                    : null;

            return Task.FromResult(
                foundUser);
        }

        public Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            if (user is null)
            {
                return Task.FromResult<User?>(
                    null);
            }

            var normalizedEmail =
                email
                    .Trim()
                    .ToLowerInvariant();

            var foundUser =
                user.Email == normalizedEmail
                    ? user
                    : null;

            return Task.FromResult(
                foundUser);
        }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            if (user is null)
            {
                return Task.FromResult(
                    false);
            }

            var normalizedEmail =
                email
                    .Trim()
                    .ToLowerInvariant();

            return Task.FromResult(
                user.Email == normalizedEmail);
        }

        public void Add(
            User user)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHashService(
        bool passwordIsValid)
        : IPasswordHashService
    {
        public string Hash(
            string password)
        {
            return "fake-password-hash";
        }

        public bool Verify(
            string passwordHash,
            string providedPassword)
        {
            return passwordIsValid;
        }
    }

    private sealed class FakeAccessTokenService
        : IAccessTokenService
    {
        public const string Token =
            "fake-access-token";

        public AccessTokenResult Generate(
            User user,
            IReadOnlyCollection<string> permissions)
        {
            return new AccessTokenResult(
                Token,
                DateTimeOffset.UtcNow
                    .AddMinutes(15));
        }
    }

    private sealed class FakeRefreshTokenService
        : IRefreshTokenService
    {
        public const string RawToken =
            "fake-refresh-token";

        public const string TokenHash =
            "fake-refresh-token-hash";

        public RefreshTokenValue Generate()
        {
            return new RefreshTokenValue(
                RawToken,
                TokenHash,
                DateTimeOffset.UtcNow
                    .AddDays(7));
        }

        public string Hash(
            string token)
        {
            return TokenHash;
        }
    }

    private sealed class FakeRefreshTokenRepository
        : IRefreshTokenRepository
    {
        private readonly List<RefreshToken> _tokens =
            [];

        public IReadOnlyCollection<RefreshToken> Tokens =>
            _tokens;

        public Task<RefreshToken?> FindByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            var token =
                _tokens.SingleOrDefault(
                    x => x.TokenHash == tokenHash);

            return Task.FromResult(
                token);
        }

        public Task<IReadOnlyCollection<RefreshToken>>
            FindActiveByUserIdAsync(
                Guid userId,
                DateTimeOffset now,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<RefreshToken> tokens =
                _tokens
                    .Where(
                        x =>
                            x.UserId == userId &&
                            x.IsActiveAt(now))
                    .ToArray();

            return Task.FromResult(
                tokens);
        }

        public void Add(
            RefreshToken refreshToken)
        {
            _tokens.Add(
                refreshToken);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}