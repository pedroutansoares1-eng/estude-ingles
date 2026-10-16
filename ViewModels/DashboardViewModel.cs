using System.Windows.Input;
using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class DashboardViewModel : ObservableBase, IRefreshable
{
    private static readonly string[] Tips =
    {
        "Revise um pouco todos os dias: 15 minutos valem mais que 2 horas no domingo.",
        "Fale em voz alta! A pronúncia melhora com repetição.",
        "Não traduza palavra por palavra: pense na frase inteira.",
        "Errar faz parte: cada erro revisado vira memória.",
        "Use o modo Conversar para praticar inglês de verdade com a Mia.",
        "Palavras novas ficam melhores quando você cria uma frase sua com elas.",
    };

    private readonly AppHost _host;

    private string _greeting = "";
    private string _streakText = "";
    private string _levelText = "";
    private string _xpText = "";
    private string _dueText = "";
    private string _dueDetail = "";
    private string _learnedText = "";
    private string _tip = "";
    private string _statsText = "";
    private double _levelProgress;
    private double _learnedProgress;
    private bool _hasDue;

    public DashboardViewModel(AppHost host)
    {
        _host = host;
        GoFlashcards = new RelayCommand(() => _host.Navigate?.Invoke("flashcards"));
        GoLessons = new RelayCommand(() => _host.Navigate?.Invoke("lessons"));
        GoQuiz = new RelayCommand(() => _host.Navigate?.Invoke("quiz"));
        GoChat = new RelayCommand(() => _host.Navigate?.Invoke("chat"));
    }

    public ICommand GoFlashcards { get; }
    public ICommand GoLessons { get; }
    public ICommand GoQuiz { get; }
    public ICommand GoChat { get; }

    public string Greeting { get => _greeting; private set => Set(ref _greeting, value); }
    public string StreakText { get => _streakText; private set => Set(ref _streakText, value); }
    public string LevelText { get => _levelText; private set => Set(ref _levelText, value); }
    public string XpText { get => _xpText; private set => Set(ref _xpText, value); }
    public string DueText { get => _dueText; private set => Set(ref _dueText, value); }
    public string DueDetail { get => _dueDetail; private set => Set(ref _dueDetail, value); }
    public string LearnedText { get => _learnedText; private set => Set(ref _learnedText, value); }
    public string Tip { get => _tip; private set => Set(ref _tip, value); }
    public string StatsText { get => _statsText; private set => Set(ref _statsText, value); }
    public double LevelProgress { get => _levelProgress; private set => Set(ref _levelProgress, value); }
    public double LearnedProgress { get => _learnedProgress; private set => Set(ref _learnedProgress, value); }
    public bool HasDue { get => _hasDue; private set => Set(ref _hasDue, value); }

    public void Refresh()
    {
        var data = _host.Data;
        var profile = data.Profile;
        var now = DateTime.Now;

        var hour = now.Hour;
        var partOfDay = hour < 12 ? "Bom dia" : hour < 18 ? "Boa tarde" : "Boa noite";
        Greeting = string.IsNullOrWhiteSpace(profile.Name)
            ? $"{partOfDay}! Pronto para estudar inglês?"
            : $"{partOfDay}, {profile.Name}!";

        StreakText = profile.StreakDays switch
        {
            0 => "Comece hoje a sua sequência 🔥",
            1 => "1 dia de sequência 🔥",
            _ => $"{profile.StreakDays} dias de sequência 🔥",
        };

        var level = Gamification.Level(profile.Xp);
        LevelText = $"Nível {level}";
        XpText = $"{profile.Xp} XP · faltam {Gamification.XpToNextLevel(profile.Xp)} XP para o nível {level + 1}";
        LevelProgress = Gamification.LevelProgress(profile.Xp);

        var today = now.Date;
        var due = data.Cards.Count(c => !SrsEngine.IsNew(c) && SrsEngine.IsDue(c, today));
        var fresh = data.Cards.Count(SrsEngine.IsNew);
        var remaining = Math.Min(Math.Max(0, data.Settings.NewCardsPerDay), fresh);
        HasDue = due + remaining > 0;
        DueText = HasDue
            ? $"{due + remaining} cartões para praticar"
            : "Tudo em dia por hoje! 🎉";
        DueDetail = $"{due} para revisar · {remaining} palavras novas liberadas hoje";

        var learned = data.Cards.Count(c => c.Repetitions > 0);
        var total = data.Words.Count;
        LearnedText = $"Você já treinou {learned} de {total} palavras";
        LearnedProgress = total == 0 ? 0 : learned * 100.0 / total;

        StatsText = $"{profile.TotalReviews} revisões · {profile.LessonsDone} lições · {profile.QuizzesDone} quizzes";
        Tip = Tips[now.DayOfYear % Tips.Length];
    }
}
