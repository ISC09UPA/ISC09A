using ImageCards.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageCards.Api.Data.Configurations;

public class CardReviewConfiguration : IEntityTypeConfiguration<CardReview>
{
    public void Configure(EntityTypeBuilder<CardReview> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.NextReviewAt).IsRequired();
        builder.Property(r => r.EaseFactor).IsRequired();

        // One review state per user and card. Its leading column also serves lookups by UserId.
        builder.HasIndex(r => new { r.UserId, r.CardId }).IsUnique();
        // Due cards query: WHERE UserId = @user AND NextReviewAt <= @now ORDER BY NextReviewAt.
        builder.HasIndex(r => new { r.UserId, r.NextReviewAt });
        builder.HasIndex(r => r.CardId);
        builder.HasIndex(r => r.NextReviewAt);

        builder.HasOne(r => r.Card)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQL Server rejects multiple cascade paths (User -> Card -> CardReview and User -> CardReview),
        // so this relationship does not cascade. Deleting a user still removes reviews through its cards.
        builder.HasOne(r => r.User)
            .WithMany(u => u.CardReviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
