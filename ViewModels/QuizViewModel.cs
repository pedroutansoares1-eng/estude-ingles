using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class QuizViewModel : ObservableBase, IRefreshable
{
    private const int QuestionCount = 8;
    private const int XpPerCorrect = 3;
    private const int XpPassBonus = 15;

    private readonly AppHost _host;

    private List<GrammarQuestion> _questions = new();
    private int _index;
    private int _correct;
    private bool _answered;
    private string? _lastTopic;

    private bool _isPick = true;
    private bool _isPlaying;
    private bool _isResult;
    private bool _feedbackVisible;
    private bool _feedbackCorrect;
    private string _feedbackText = "";
    private int _xpEarned;
    private bool _passed;

    public QuizViewModel(AppHost host)
    {
        _host = host;
        ContinueCommand = new RelayCommand(Continue);
        RetryCommand = new RelayCommand(() => StartQuiz(_lastTopic));
        BackCommand = new RelayCommand(() =>
        {
            _isResult = false;
            _isPlaying = false;
            _isPick = true;
            Refresh();
            RaiseState();
        });
    }

    public ObservableCollection<CategoryOption> Topics { get; } = new();
    public ObservableCollection<ChoiceOption> Options { get; } = new();

    public ICommand ContinueCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand BackCommand { get; }

    public bool IsPick => _isPick;
    public bool IsPlaying => _isPlaying;
    public bool IsResult => _isResult;

    public string QuestionText => _index < _questions.Count ? _questions[_index].Prompt : "";
    public string Explanation => _index < _questions.Count ? _questions[_index].Explanation : "";
    public string ProgressText => _questions.Count == 0 ? "" : $"{_index + 1} de {_questions.Count}";
    public double ProgressValue => _questions.Count == 0 ? 0 : _index * 100.0 / _questions.Count;
    public bool FeedbackVisible => _feedbackVisible;
    public bool FeedbackCorrect => _feedbackCorrect;
    public string FeedbackText => _feedbackText;
    public IBrush FeedbackBrush => _feedbackCorrect ? Palette.CardCorrect : Palette.CardWrong;
    public string ContinueLabel => _index + 1 >= _questions.Count ? "Ver resultado" : "Continuar";

    public bool Passed => _passed;
    public string ResultTitle => _passed ? "Muito bem!" : "Continue praticando!";
    public string ResultEmoji => _passed ? "\U0001F31F" : "\U0001F4DA";
    public string ResultText =>
        $"Você acertou {_correct} de {_questions.Count} ({Score}%).";
    public int Score => _questions.Count == 0 ? 0 : (int)Math.Round(_correct * 100.0 / _questions.Count);
    public string XpText => $"+{_xpEarned} XP";

    public void Refresh()
    {
        var bank = SeedGrammar.All();
        var options = new List<CategoryOption>
        {
            new()
            {
                Key = null,
                Label = "\U0001F3B2  Todos os temas",
                Detail = $"{Math.Min(QuestionCount, bank.Count)} perguntas sortidas",
                Command = new RelayCommand(() => StartQuiz(null)),
            },
        };

        foreach (var topic in GrammarTopics.All)
        {
            var count = bank.Count(q => q.Topic == topic.Key);
            if (count == 0)
                continue;
            var key = topic.Key;
            options.Add(new CategoryOption
            {
                Key = key,
                Label = $"{topic.Emoji}  {topic.Label}",
                Detail = $"{count} perguntas disponíveis",
                Command = new RelayCommand(() => StartQuiz(key)),
            });
        }

        Topics.Replace(options);
    }

    private void StartQuiz(string? topicKey)
    {
        _lastTopic = topicKey;
        var bank = SeedGrammar.All();
        var pool = topicKey is null ? bank : bank.Where(q => q.Topic == topicKey).ToList();
        if (pool.Count == 0)
            pool = bank;

        _questions = pool.OrderBy(_ => _host.Rng.Next()).Take(Math.Min(QuestionCount, pool.Count)).ToList();
        _index = 0;
        _correct = 0;
        _xpEarned = 0;
        _passed = false;
        _isPick = false;
        _isResult = false;
        _isPlaying = _questions.Count > 0;
        ShowCurrent();
        RaiseState();
    }

    private void ShowCurrent()
    {
        Options.Clear();
        _feedbackVisible = false;
        _feedbackCorrect = false;
        _feedbackText = "";
        _answered = false;

        if (_index >= _questions.Count)
            return;

        foreach (var option in _questions[_index].Options)
            Options.Add(new ChoiceOption(option, PickOption));
    }

    private void PickOption(ChoiceOption option)
    {
        if (_answered || _index >= _questions.Count)
            return;

        _answered = true;
        var question = _questions[_index];
        var correctText = question.Options[question.AnswerIndex];
        var isCorrect = option.Text == correctText;

        foreach (var candidate in Options)
            candidate.Background = candidate.Text == correctText ? Palette.CardCorrect : Palette.Card;
        if (!isCorrect)
            option.Background = Palette.CardWrong;

        if (isCorrect)
            _correct++;

        _feedbackCorrect = isCorrect;
        _feedbackText = isCorrect
            ? $"\u2705 {question.Explanation}"
            : $"\u274C A resposta certa é “{correctText}”. {question.Explanation}";
        _feedbackVisible = true;
        RaiseState();
    }

    private void Continue()
    {
        if (!_feedbackVisible)
            return;

        _index++;
        if (_index >= _questions.Count)
        {
            Finish();
            return;
        }

        ShowCurrent();
        RaiseState();
    }

    private void Finish()
    {
        var score = Score;
        _passed = score >= 70;
        _xpEarned = _correct * XpPerCorrect + (_passed ? XpPassBonus : 0);

        var profile = _host.Data.Profile;
        profile.Xp += _xpEarned;
        profile.QuizzesDone++;
        Gamification.RegisterStudyActivity(profile, DateTime.Now);
        _host.Save();

        _isPlaying = false;
        _isResult = true;
        RaiseState();
    }

    private void RaiseState()
    {
        foreach (var name in new[]
        {
            nameof(IsPick), nameof(IsPlaying), nameof(IsResult), nameof(QuestionText), nameof(ProgressText),
            nameof(ProgressValue), nameof(FeedbackVisible), nameof(FeedbackCorrect), nameof(FeedbackText),
            nameof(FeedbackBrush), nameof(ContinueLabel), nameof(Passed), nameof(ResultTitle), nameof(ResultText), nameof(ResultEmoji),
            nameof(Score), nameof(XpText),
        })
            Raise(name);
    }
}
