using KIAN.Models;

namespace KIAN.Services;

/// <summary>
/// Versión simplificada de SM-2.
/// Calidad: 0 = Otra vez, 1 = Difícil, 2 = Bien, 3 = Fácil
/// </summary>
public class SpacedRepetitionService
{
    private const decimal MinEase = 1.30m;
    private const decimal MaxEase = 5.00m;

    public void Apply(CardReview review, int quality)
    {
        var now = DateTime.UtcNow;

        switch (quality)
        {
            case 0: // Otra vez
                review.Lapses++;
                review.Repetitions = 0;
                review.IntervalDays = 0;
                review.EaseFactor = Clamp(review.EaseFactor - 0.20m);
                review.NextReviewAt = now.AddMinutes(10);
                return;

            case 1: // Difícil
                review.EaseFactor = Clamp(review.EaseFactor - 0.15m);
                review.IntervalDays = Math.Max(1, (int)Math.Round(review.IntervalDays * 1.2));
                break;

            case 2: // Bien
                review.IntervalDays = review.Repetitions switch
                {
                    0 => 1,
                    1 => 3,
                    _ => Math.Max(1, (int)Math.Round(review.IntervalDays * (double)review.EaseFactor))
                };
                break;

            case 3: // Fácil
                review.EaseFactor = Clamp(review.EaseFactor + 0.15m);
                review.IntervalDays = review.Repetitions switch
                {
                    0 => 3,
                    1 => 5,
                    _ => Math.Max(1, (int)Math.Round(review.IntervalDays * (double)review.EaseFactor * 1.3))
                };
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(quality), "La calidad debe estar entre 0 y 3.");
        }

        review.Repetitions++;
        review.NextReviewAt = now.AddDays(review.IntervalDays);
    }

    private static decimal Clamp(decimal value) => Math.Min(MaxEase, Math.Max(MinEase, value));
}