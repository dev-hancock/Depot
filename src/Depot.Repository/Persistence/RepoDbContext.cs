namespace Depot.Repository.Persistence;

using Domain;
using Microsoft.EntityFrameworkCore;

public class RepoDbContext : DbContext
{
    public RepoDbContext(DbContextOptions<RepoDbContext> options) : base(options)
    {
    }

    public DbSet<Artifact> Artifacts => Set<Artifact>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Artifact>(b =>
        {
            b.ToTable("artifacts");

            b.HasKey(a => a.Id);
            b.Property(a => a.Id).ValueGeneratedNever();

            b.Property(a => a.Name).HasMaxLength(200).IsRequired();
            b.Property(a => a.Repository).HasMaxLength(100).IsRequired();
            b.Property(a => a.Location).HasMaxLength(500).IsRequired();
            b.Property(a => a.ContentType).HasMaxLength(127).IsRequired();
            b.Property(a => a.Extension).HasMaxLength(10).IsRequired();
            b.Property(a => a.Hash).HasMaxLength(128).IsRequired();
            b.Property(a => a.Size).HasColumnType("bigint").IsRequired();
            b.Property(a => a.CreatedAt).HasColumnType("timestamptz").IsRequired();
            b.Property(a => a.CreatedBy).HasMaxLength(100).IsRequired();

            b.HasIndex(a => new
            {
                a.Repository,
                a.Name
            });
            b.HasIndex(a => a.Hash);
            b.HasIndex(a => a.Location).IsUnique();
        });
    }
}