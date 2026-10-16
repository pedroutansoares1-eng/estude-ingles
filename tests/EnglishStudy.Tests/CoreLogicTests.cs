using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.Tests;

public class SrsEngineTests
{
    [Fact]
    public void NewCard_IsNewAndDueToday()
    {
        var card = SrsEngine.NewCard("w1");

        Assert.True(SrsEngine.IsNew(card));
        Assert.True(SrsEngine.IsDue(card, DateTime.Today));
        Assert.Equal(2.5, card.Ease);
    }

    [Fact]
    public void FirstGood_ComesBackInTwoDays()
    {
        var card = SrsEngine.NewCard("w1");
        var now = new DateTime(2026, 1, 1, 10, 0, 0);

        SrsEngine.Apply(card, SrsRating.Good, now);

        Assert.Equal(1, card.Repetitions);
        Assert.Equal(2, card.IntervalDays);
        Assert.Equal(new DateTime(2026, 1, 3), card.Due);
        Assert.False(SrsEngine.IsNew(card));
    }

    [Fact]
    public void Again_ResetsProgressAndLowersEase()
    {
        var card = SrsEngine.NewCard("w1");
        var now = DateTime.Today;
        SrsEngine.Apply(card, SrsRating.Good, now);
        var easeBefore = card.Ease;

        SrsEngine.Apply(card, SrsRating.Again, now);

        Assert.Equal(0, card.Repetitions);
        Assert.Equal(0, card.IntervalDays);
        Assert.Equal(1, card.Lapses);
        Assert.True(card.Ease < easeBefore);
        Assert.Equal(now.Date, card.Due);
        Assert.True(SrsEngine.IsDue(card, now));
    }

    [Fact]
    public void Easy_RaisesEaseAndGivesThreeDaysInitially()
    {
        var card = SrsEngine.NewCard("w1");

        SrsEngine.Apply(card, SrsRating.Easy, DateTime.Today);

        Assert.Equal(2.65, card.Ease, 3);
        Assert.Equal(3, card.IntervalDays);
    }

    [Fact]
    public void IntervalIsCappedAtOneYear()
    {
        var card = SrsEngine.NewCard("w1");
        card.Repetitions = 5;
        card.IntervalDays = 400;
        card.Ease = 2.5;

        SrsEngine.Apply(card, SrsRating.Good, DateTime.Today);

        Assert.Equal(SrsEngine.MaxIntervalDays, card.IntervalDays);
    }

    [Fact]
    public void EaseNeverDropsBelowMinimum()
    {
        var card = SrsEngine.NewCard("w1");
        card.Ease = 1.35;

        SrsEngine.Apply(card, SrsRating.Again, DateTime.Today);

        Assert.Equal(SrsEngine.MinEase, card.Ease, 3);
    }

    [Theory]
    [InlineData(SrsRating.Again, 0)]
    [InlineData(SrsRating.Hard, 1)]
    [InlineData(SrsRating.Good, 3)]
    [InlineData(SrsRating.Easy, 4)]
    public void XpPerRating(SrsRating rating, int expected)
    {
        Assert.Equal(expected, SrsEngine.XpFor(rating));
    }
}

public class GamificationTests
{
    private static readonly DateTime Today = new(2026, 3, 10);

    [Fact]
    public void FirstStudy_SetsStreakToOne()
    {
        var profile = new UserProfile();

        Gamification.RegisterStudyActivity(profile, Today);

        Assert.Equal(1, profile.StreakDays);
        Assert.Equal(Today, profile.LastStudyDate);
    }

    [Fact]
    public void ConsecutiveDays_IncrementStreak()
    {
        var profile = new UserProfile { StreakDays = 4, LastStudyDate = Today.AddDays(-1) };

        Gamification.RegisterStudyActivity(profile, Today);

        Assert.Equal(5, profile.StreakDays);
    }

    [Fact]
    public void SkippedDay_ResetsStreak()
    {
        var profile = new UserProfile { StreakDays = 9, LastStudyDate = Today.AddDays(-3) };

        Gamification.RegisterStudyActivity(profile, Today);

        Assert.Equal(1, profile.StreakDays);
    }

    [Fact]
    public void SameDay_DoesNotChangeStreak()
    {
        var profile = new UserProfile { StreakDays = 7, LastStudyDate = Today };

        Gamification.RegisterStudyActivity(profile, Today);

        Assert.Equal(7, profile.StreakDays);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(250, 3)]
    public void LevelFromXp(int xp, int expected)
    {
        Assert.Equal(expected, Gamification.Level(xp));
    }

    [Fact]
    public void LevelProgress_And_RemainingXp()
    {
        Assert.Equal(50, Gamification.LevelProgress(150));
        Assert.Equal(50, Gamification.XpToNextLevel(150));
    }
}
