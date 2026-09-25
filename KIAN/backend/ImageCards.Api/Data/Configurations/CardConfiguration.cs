using ImageCards.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageCards.Api.Data.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ImageBlobName).IsRequired().HasMaxLength(FieldLengths.BlobName).IsUnicode(false);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.ImageBlobName).IsUnique();

        builder.HasOne(c => c.User)
            .WithMany(u => u.Cards)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
