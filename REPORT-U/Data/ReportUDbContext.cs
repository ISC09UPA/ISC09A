using Microsoft.EntityFrameworkCore;
using ReportU.Models;

namespace ReportU.Data;

/// <summary>Contexto EF Core + PostgreSQL de ReportU.</summary>
public class ReportUDbContext(DbContextOptions<ReportUDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostImage> PostImages => Set<PostImage>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostSupport> PostSupports => Set<PostSupport>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Username).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.DisplayNamePreference).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Post>(e =>
        {
            e.ToTable("posts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.IdentityMode).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.SupportCount);
            e.HasIndex(x => x.AuthorId);
        });

        b.Entity<PostImage>(e =>
        {
            e.ToTable("post_images");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PostId);
        });

        b.Entity<Comment>(e =>
        {
            e.ToTable("comments");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.PostId);
        });

        b.Entity<PostSupport>(e =>
        {
            e.ToTable("post_supports");
            e.HasKey(x => new { x.PostId, x.UserId });
            e.HasOne(x => x.Post)
                .WithMany(p => p.Supports)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany(u => u.Supports)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Bookmark>(e =>
        {
            e.ToTable("bookmarks");
            e.HasKey(x => new { x.PostId, x.UserId });
            e.HasOne(x => x.Post)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is User u)
            {
                if (entry.State == EntityState.Added) { u.CreatedAt = now; u.UpdatedAt = now; }
                else if (entry.State == EntityState.Modified) { u.UpdatedAt = now; }
            }
            else if (entry.Entity is Post p)
            {
                if (entry.State == EntityState.Added) { p.CreatedAt = now; p.UpdatedAt = now; }
                else if (entry.State == EntityState.Modified) { p.UpdatedAt = now; }
            }
            else if (entry.Entity is PostImage img && entry.State == EntityState.Added)
            {
                img.CreatedAt = now;
            }
            else if (entry.Entity is Comment c && entry.State == EntityState.Added)
            {
                c.CreatedAt = now;
            }
            else if (entry.Entity is PostSupport s && entry.State == EntityState.Added)
            {
                s.CreatedAt = now;
            }
            else if (entry.Entity is Bookmark bm && entry.State == EntityState.Added)
            {
                bm.CreatedAt = now;
            }
        }
        return base.SaveChangesAsync(ct);
    }
}
