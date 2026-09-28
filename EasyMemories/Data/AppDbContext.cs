using EasyMemories.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EasyMemories.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Space> Spaces => Set<Space>();

    public DbSet<Memory> Memories => Set<Memory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Space>(entity =>
        {
            entity.HasIndex(s => s.JoinCode).IsUnique();
            entity.Property(s => s.Name).HasMaxLength(120).IsRequired();
            entity.Property(s => s.JoinCode).HasMaxLength(16).IsRequired();

            entity.HasOne(s => s.Owner)
                .WithMany(u => u.Spaces)
                .HasForeignKey(s => s.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Memory>(entity =>
        {
            entity.Property(m => m.GuestName).HasMaxLength(80);
            entity.Property(m => m.Comment).HasMaxLength(500);

            entity.HasOne(m => m.Space)
                .WithMany(s => s.Memories)
                .HasForeignKey(m => m.SpaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
