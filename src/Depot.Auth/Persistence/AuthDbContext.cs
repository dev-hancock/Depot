namespace Depot.Auth.Persistence;

using System.Reactive.Threading.Tasks;
using Domain.Auth;
using Domain.Common;
using Domain.Organisations;
using Domain.Tenants;
using Domain.Users;
using Mestra.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class AuthDbContext : DbContext
{
    private const string Schema = "auth";

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    public DbSet<Organisation> Organisations => Set<Organisation>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Membership> Memberships => Set<Membership>();

    public override async Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        var entities = ChangeTracker
            .Entries<Root>()
            .Select(x => x.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(token);

        var mediator = this.GetService<IMediator>();

        foreach (var entity in entities)
        {
            foreach (var notification in entity.Events)
            {
                await mediator.Publish(notification).ToTask(token);
            }

            entity.Clear();
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);

        builder.Entity<User>(e =>
        {
            e.ToTable("users", Schema);

            e.HasKey(u => u.Id);
            e.Property(u => u.Id)
                .HasConversion(
                    x => x.Value,
                    x => new UserId(x))
                .ValueGeneratedNever();

            e.OwnsOne(
                u => u.Username,
                x =>
                {
                    x.WithOwner()
                        .HasForeignKey(p => p.UserId);

                    x.Property(p => p.Value)
                        .HasMaxLength(64)
                        .IsRequired();

                    x.Property(p => p.Normalized)
                        .HasMaxLength(64)
                        .IsRequired();

                    x.HasIndex(p => p.Normalized)
                        .IsUnique();
                });

            e.OwnsOne(
                u => u.Email,
                x =>
                {
                    x.Property(p => p.Value)
                        .HasMaxLength(128)
                        .IsRequired();

                    x.Property(p => p.Normalized)
                        .HasMaxLength(128)
                        .IsRequired();

                    x.HasIndex(p => p.Normalized)
                        .IsUnique();
                });

            e.Property(x => x.Password)
                .HasConversion(
                    x => x.Encoded,
                    x => Password.Create(x))
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Entity<Organisation>(e =>
        {
            e.ToTable("organisations", Schema);

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

            e.Property(o => o.CreatedBy)
                .HasConversion(
                    x => x.Value,
                    x => new UserId(x))
                .IsRequired();

            e.HasIndex(o => o.Slug).IsUnique();
        });

        builder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants", Schema);

            e.HasKey(t => t.Id);
            e.Property(t => t.Id).ValueGeneratedNever();

            e.Property(t => t.Name).HasMaxLength(128).IsRequired();
            e.Property(o => o.Slug)
                .HasConversion(
                    x => x.Value,
                    x => Slug.Create(x))
                .HasMaxLength(128)
                .IsRequired();

            e.Property(o => o.CreatedBy)
                .HasConversion(
                    x => x.Value,
                    x => new UserId(x))
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
            e.ToTable("roles", Schema);

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
            e.ToTable("permissions", Schema);

            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();

            e.Property(p => p.Name).HasMaxLength(128).IsRequired();
            e.HasIndex(p => p.Name).IsUnique();
        });

        builder.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions", Schema);

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
            e.ToTable("memberships", Schema);

            e.Property(x => x.UserId)
                .HasConversion(
                    x => x.Value,
                    x => new UserId(x));

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

        builder.Entity<Session>(e =>
        {
            e.ToTable("sessions", Schema);

            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .HasConversion(
                    x => x.Value,
                    x => new SessionId(x))
                .ValueGeneratedNever();

            e.Property(x => x.UserId)
                .HasConversion(
                    x => x.Value,
                    x => new UserId(x))
                .IsRequired();

            e.OwnsOne(
                x => x.RefreshToken,
                t =>
                {
                    t.Property(p => p.Value).HasMaxLength(512).IsRequired();
                    t.Property(p => p.ExpiresAt).IsRequired();

                    t.HasIndex(p => p.Value).IsUnique();
                });

            e.Property(x => x.IsRevoked).IsRequired();

            e.HasOne(x => x.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}