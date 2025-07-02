namespace Depot.Auth.Persistence;

using Domain;
using Microsoft.EntityFrameworkCore;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Token> Tokens => Set<Token>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasPostgresExtension("citext");

        builder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

            e.Property(x => x.Username).HasMaxLength(64).HasColumnType("citext");
            e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();

            e.HasIndex(x => x.Username).IsUnique();
        });

        builder.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Name).HasMaxLength(64).HasColumnType("citext");

            e.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<UserRole>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => new
            {
                x.UserId,
                x.RoleId
            });

            e.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId);

            e.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId);
        });

        builder.Entity<Token>(e =>
        {
            e.ToTable("tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

            e.Property(x => x.UserId).IsRequired();
            e.Property(x => x.Type).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.ExpiresAt).IsRequired();
            e.Property(x => x.IsRevoked).IsRequired();

            e.HasOne(x => x.User)
                .WithMany(x => x.Tokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.Value).IsUnique();
        });
    }
}