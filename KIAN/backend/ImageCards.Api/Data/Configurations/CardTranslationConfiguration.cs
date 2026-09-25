using ImageCards.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageCards.Api.Data.Configurations;

public class CardTranslationConfiguration : IEntityTypeConfiguration<CardTranslation>
{
    public void Configure(EntityTypeBuilder<CardTranslation> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.LanguageCode).IsRequired().HasMaxLength(FieldLengths.LanguageCode).IsUnicode(false);
        builder.Property(t => t.TranslatedText).IsRequired().HasMaxLength(FieldLengths.TranslatedText);
        builder.Property(t => t.ExampleSentence).HasMaxLength(FieldLengths.ExampleSentence);

        // One translation per language per card. Its leading column also serves lookups by CardId.
        builder.HasIndex(t => new { t.CardId, t.LanguageCode }).IsUnique();

        builder.HasOne(t => t.Card)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.CardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
