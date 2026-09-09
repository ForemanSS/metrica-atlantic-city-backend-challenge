namespace AtlanticCity.Authentication.Domain.Users;

public sealed class User
{
    private User()
    {
    }

    private User(
        Guid id,
        string email,
        string displayName,
        string passwordHash,
        UserRole role,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static User Create(
        string email,
        string displayName,
        string passwordHash,
        UserRole role,
        DateTimeOffset? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(
            Guid.NewGuid(),
            NormalizeEmail(email),
            displayName.Trim(),
            passwordHash,
            role,
            createdAt ?? DateTimeOffset.UtcNow);
    }

    public void ChangePasswordHash(
        string passwordHash,
        DateTimeOffset? updatedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public void Deactivate(
        DateTimeOffset? updatedAt = null)
    {
        IsActive = false;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}