using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>XP, níveis e sequência diária (streak).</summary>
public static class Gamification
{
    public const int XpPerLevel = 100;

    public static int Level(int xp) => Math.Max(1, xp / XpPerLevel + 1);

    /// <summary>Progresso (0-100) dentro do nível atual.</summary>
    public static double LevelProgress(int xp) => xp % XpPerLevel;

    public static int XpToNextLevel(int xp) => XpPerLevel - (xp % XpPerLevel);

    /// <summary>Marca um dia de estudo e atualiza a sequência.</summary>
    public static void RegisterStudyActivity(UserProfile profile, DateTime now)
    {
        var today = now.Date;
        if (profile.LastStudyDate.Date == today)
            return;

        profile.StreakDays = profile.LastStudyDate.Date == today.AddDays(-1)
            ? profile.StreakDays + 1
            : 1;
        profile.LastStudyDate = today;
    }
}
