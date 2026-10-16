namespace EnglishStudy.Models;

public sealed class UserProfile
{
    public string Name { get; set; } = "";
    public int Xp { get; set; }
    public int StreakDays { get; set; }
    public DateTime LastStudyDate { get; set; } = DateTime.MinValue;
    public int TotalReviews { get; set; }
    public int TotalCorrect { get; set; }
    public int LessonsDone { get; set; }
    public int QuizzesDone { get; set; }
}

public sealed class AppSettings
{
    public int NewCardsPerDay { get; set; } = 15;
    public string AiEndpoint { get; set; } = "http://localhost:1234/v1";
    public string AiModel { get; set; } = "";
    public string AiApiKey { get; set; } = "";
}
