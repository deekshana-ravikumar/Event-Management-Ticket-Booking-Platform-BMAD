using EventManagement.Application.Common.Interfaces;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ConsentLedger> ConsentLedger => Set<ConsentLedger>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(u => u.Status).HasConversion<string>().HasMaxLength(25);
            entity.Property(u => u.Email).HasMaxLength(255);
            entity.Property(u => u.PasswordHash).HasMaxLength(255);
            entity.Property(u => u.FullName).HasMaxLength(200);
            entity.Property(u => u.Phone).HasMaxLength(15);
            entity.Property(u => u.City).HasMaxLength(100);
        });

        builder.Entity<Organization>(entity =>
        {
            entity.HasIndex(o => o.UserId).IsUnique();
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(25);
            entity.Property(o => o.OrganizationName).HasMaxLength(200);
            entity.Property(o => o.ContactPerson).HasMaxLength(200);
            entity.Property(o => o.Category).HasMaxLength(100);
            entity.HasOne(o => o.User)
                .WithOne(u => u.Organization)
                .HasForeignKey<Organization>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasIndex(t => t.Token).IsUnique();
            entity.Property(t => t.Token).HasMaxLength(128);
            entity.HasOne(t => t.User)
                .WithMany(u => u.EmailVerificationTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(rt => rt.UserId);
            entity.HasIndex(rt => rt.TokenHash);
            entity.Property(rt => rt.TokenHash).HasMaxLength(128);
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConsentLedger>(entity =>
        {
            entity.Property(c => c.ConsentType).HasConversion<string>().HasMaxLength(30);
            entity.Property(c => c.TncVersion).HasMaxLength(20);
            entity.Property(c => c.IpAddress).HasMaxLength(45);
            entity.Property(c => c.UserAgent).HasMaxLength(512);
            entity.Property(c => c.SessionId).HasMaxLength(128);
            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        builder.Entity<LoginAttempt>(entity =>
        {
            entity.HasIndex(l => new { l.Email, l.AttemptedAt });
            entity.Property(l => l.Email).HasMaxLength(255);
            entity.Property(l => l.IpAddress).HasMaxLength(45);
        });
    }
}
