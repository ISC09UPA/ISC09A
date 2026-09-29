using KIAN.Models;
using Microsoft.EntityFrameworkCore;

namespace KIAN.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<CardTranslation> CardTranslations => Set<CardTranslation>();
    public DbSet<CardReview> CardReviews => Set<CardReview>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<Card>(e =>
        {
            e.ToTable("Cards");
            e.Property(x => x.ImageBlobName).HasMaxLength(500).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
             .WithMany(u => u.Cards)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CardTranslation>(e =>
        {
            e.ToTable("CardTranslations");
            e.Property(x => x.LanguageCode).HasMaxLength(10).IsRequired();
            e.Property(x => x.TranslatedText).HasMaxLength(500).IsRequired();
            e.Property(x => x.ExampleSentence).HasMaxLength(1000);
            e.HasIndex(x => x.LanguageCode);
            e.HasIndex(x => new { x.CardId, x.LanguageCode }).IsUnique();
            e.HasOne(x => x.Card)
             .WithMany(c => c.Translations)
             .HasForeignKey(x => x.CardId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CardReview>(e =>
        {
            e.ToTable("CardReviews");
            e.Property(x => x.NextReviewAt).HasColumnType("datetime2");
            e.Property(x => x.EaseFactor).HasColumnType("decimal(5,2)");
            e.HasIndex(x => x.NextReviewAt);
            e.HasIndex(x => new { x.UserId, x.CardId }).IsUnique();

            e.HasOne(x => x.Card)
             .WithMany(c => c.Reviews)
             .HasForeignKey(x => x.CardId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.User)
             .WithMany(u => u.Reviews)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}