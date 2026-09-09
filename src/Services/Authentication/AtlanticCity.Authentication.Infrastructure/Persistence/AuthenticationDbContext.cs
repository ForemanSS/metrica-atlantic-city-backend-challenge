using AtlanticCity.Authentication.Domain.Tokens;
using AtlanticCity.Authentication.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AtlanticCity.Authentication.Infrastructure.Persistence;

public sealed class AuthenticationDbContext(
    DbContextOptions<AuthenticationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureRefreshTokens(modelBuilder);
    }

    private static void ConfigureUsers(
        ModelBuilder modelBuilder)
    {
        var entity =
            modelBuilder.Entity<User>();

        entity.ToTable("auth_user");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id");

        entity.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(1000)
            .IsRequired();

        entity.Property(x => x.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        entity.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        entity.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName(
                "ux_auth_user_email");
    }

    private static void ConfigureRefreshTokens(
        ModelBuilder modelBuilder)
    {
        var entity =
            modelBuilder.Entity<RefreshToken>();

        entity.ToTable("auth_refresh_token");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id");

        entity.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        entity.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        entity.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        entity.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at");

        entity.Property(x => x.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(100);

        entity.Property(x => x.RevokedByIp)
            .HasColumnName("revoked_by_ip")
            .HasMaxLength(100);

        entity.Property(x => x.ReplacedByTokenId)
            .HasColumnName("replaced_by_token_id");

        entity.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName(
                "ux_auth_refresh_token_hash");

        entity.HasIndex(x => x.UserId)
            .HasDatabaseName(
                "ix_auth_refresh_token_user_id");

        entity.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName(
                "ix_auth_refresh_token_expires_at");

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(x => x.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}