using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>Agendamento de revisões no estilo SM-2 (base do Anki).</summary>
public static class SrsEngine
{
    public const double MinEase = 1.3;
    public const double MaxEase = 3.0;
    public const int MaxIntervalDays = 365;

    public static SrsCard NewCard(string wordId) => new()
    {
        WordId = wordId,
        Ease = 2.5,
        IntervalDays = 0,
        Repetitions = 0,
        Due = DateTime.Today,
    };

    public static bool IsNew(SrsCard card) => card.Repetitions == 0;

    public static bool IsDue(SrsCard card, DateTime today) => card.Due.Date <= today.Date;

    /// <summary>Aplica uma resposta do usuário e recalcula o próximo vencimento.</summary>
    public static void Apply(SrsCard card, SrsRating rating, DateTime now)
    {
        if (rating == SrsRating.Again)
        {
            card.Lapses++;
            card.Repetitions = 0;
            card.IntervalDays = 0;
            card.Ease = Math.Max(MinEase, card.Ease - 0.2);
            card.Due = now.Date; // continua vencido hoje para reaparecer na sessão
            card.LastReview = now;
            return;
        }

        if (rating == SrsRating.Hard)
            card.Ease = Math.Max(MinEase, card.Ease - 0.15);
        else if (rating == SrsRating.Easy)
            card.Ease = Math.Min(MaxEase, card.Ease + 0.15);

        int interval;
        if (card.Repetitions == 0)
        {
            interval = rating switch
            {
                SrsRating.Easy => 3,
                SrsRating.Hard => 1,
                _ => 2,
            };
        }
        else
        {
            var multiplier = rating switch
            {
                SrsRating.Hard => 1.2,
                SrsRating.Easy => card.Ease * 1.3,
                _ => card.Ease,
            };
            interval = Math.Max(1, (int)Math.Round(card.IntervalDays * multiplier));
        }

        card.Repetitions++;
        card.IntervalDays = Math.Min(MaxIntervalDays, interval);
        card.Due = now.Date.AddDays(card.IntervalDays);
        card.LastReview = now;
    }

    /// <summary>XP ganho por resposta no modo flashcards.</summary>
    public static int XpFor(SrsRating rating) => rating switch
    {
        SrsRating.Again => 0,
        SrsRating.Hard => 1,
        SrsRating.Good => 3,
        SrsRating.Easy => 4,
        _ => 0,
    };
}
