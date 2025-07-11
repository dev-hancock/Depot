namespace Depot.Auth.Persistence;

using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Domain.Auth;
using Domain.Common;
using Domain.Organisations;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Services;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Organisation> Organisations => Set<Organisation>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Membership> Memberships => Set<Membership>();

    public DbSet<Token> Tokens => Set<Token>();

    public override async Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        var entities = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(x => x.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(token);

        foreach (var entity in entities)
        {
            await entity.Events
                .Select(x => Mediator.Instance.Publish(x))
                .ToObservable()
                .Merge()
                .ToTask(token);

            entity.Events.Clear();
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        foreach (var type in builder.Model.GetEntityTypes())
        {
            if (typeof(AggregateRoot).IsAssignableFrom(type.ClrType))
            {
                builder.Entity(type.ClrType).Ignore(nameof(AggregateRoot.Events));
            }
        }

        builder.Entity<User>(e =>
        {
            e.ToTable("users");

            e.HasKey(u => u.Id);
            e.Property(u => u.Id).ValueGeneratedNever();

            e.Property(u => u.Username).HasMaxLength(64).IsRequired();
            e.Property(o => o.Email)
                .HasConversion(
                    x => x.Value,
                    x => Email.Create(x))
                .HasMaxLength(128)
                .IsRequired();
            e.Property(x => x.Password)
                .HasConversion(
                    x => x.Encoded,
                    x => Password.Create(x))
                .HasMaxLength(200)
                .IsRequired();

            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
        });

        builder.Entity<Organisation>(e =>
        {
            e.ToTable("organisations");

            e.HasKey(o => o.Id);
            e.Property(o => o.Id).ValueGeneratedNever();

            e.Property(o => o.Name).HasMaxLength(128).IsRequired();
            e.Property(o => o.Slug).HasMaxLength(128).IsRequired();
            e.Property(o => o.Slug)
                .HasConversion(
                    x => x.Value,
                    x => Slug.Create(x))
                .HasMaxLength(128)
                .IsRequired();

            e.HasIndex(o => o.Slug).IsUnique();
        });

        builder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");

            e.HasKey(t => t.Id);
            e.Property(t => t.Id).ValueGeneratedNever();

            e.Property(t => t.Name).HasMaxLength(128).IsRequired();
            e.Property(o => o.Slug)
                .HasConversion(
                    x => x.Value,
                    x => Slug.Create(x))
                .HasMaxLength(128)
                .IsRequired();

            e.HasIndex(t => new
            {
                t.OrganisationId,
                t.Slug
            }).IsUnique();

            e.HasOne(t => t.Organisation)
                .WithMany(o => o.Tenants)
                .HasForeignKey(t => t.OrganisationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Role>(e =>
        {
            e.ToTable("roles");

            e.HasKey(r => r.Id);
            e.Property(r => r.Id).ValueGeneratedNever();

            e.Property(r => r.Name).HasMaxLength(64).IsRequired();

            e.HasIndex(r => new
            {
                r.TenantId,
                r.Name
            }).IsUnique();

            e.HasOne(r => r.Tenant)
                .WithMany(t => t.Roles)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Permission>(e =>
        {
            e.ToTable("permissions");

            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();

            e.Property(p => p.Name).HasMaxLength(128).IsRequired();
            e.HasIndex(p => p.Name).IsUnique();
        });

        builder.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions");

            e.HasKey(rp => new
            {
                rp.RoleId,
                rp.PermissionId
            });

            e.HasOne(rp => rp.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(rp => rp.RoleId);

            e.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        builder.Entity<Membership>(e =>
        {
            e.ToTable("memberships");

            e.HasKey(m => new
            {
                m.UserId,
                m.TenantId,
                m.RoleId
            });

            e.HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId);

            e.HasOne(m => m.Role)
                .WithMany(r => r.Memberships)
                .HasForeignKey(m => m.RoleId);

            e.HasOne(m => m.Tenant)
                .WithMany(t => t.Memberships)
                .HasForeignKey(m => m.TenantId);
        });

        builder.Entity<Token>(e =>
        {
            e.ToTable("tokens");

            e.HasKey(t => t.Id);
            e.Property(t => t.Id).ValueGeneratedNever();

            e.Property(t => t.Type).HasMaxLength(20).IsRequired();
            e.Property(t => t.Value).HasMaxLength(512).IsRequired();
            e.Property(t => t.CreatedAt).IsRequired();
            e.Property(t => t.ExpiresAt).IsRequired();
            e.Property(t => t.IsRevoked).IsRequired();

            e.HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(t => t.Value).IsUnique();
        });
    }
}