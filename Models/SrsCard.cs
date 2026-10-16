namespace EnglishStudy.Models;

public enum SrsRating
{
    Again = 0,
    Hard = 1,
    Good = 2,
    Easy = 3,
}

public sealed class SrsCard
{
    public string WordId { get; set; } = "";
    public double Ease { get; set; } = 2.5;
    public int IntervalDays { get; set; }
    public int Repetitions { get; set; }
    public int Lapses { get; set; }
    public DateTime Due { get; set; } = DateTime.Today;
    public DateTime? LastReview { get; set; }
}
