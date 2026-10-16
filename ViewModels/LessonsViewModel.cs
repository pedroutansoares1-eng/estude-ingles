using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class LessonsViewModel : ObservableBase, IRefreshable
{
    private const int QuestionCount = 6;
    private const int StartingLives = 3;
    private const int XpPerCorrect = 5;
    private const int XpPassBonus = 10;

    private readonly AppHost _host;

    private List<ExerciseBase> _exercises = new();
    private int _index;
    private int _lives = StartingLives;
    private int _correct;
    private string? _lastCategory;
    private bool _isPick = true;
    private bool _isPlaying;
    private bool _isResult;

    private ChoiceExercise? _choice;
    private ScrambleExercise? _scramble;
    private bool _feedbackVisible;
    private bool _feedbackCorrect;
    private string _feedbackText = "";
    private bool _scrambleChecked;
    private bool _passed;
    private string _resultTitle = "";
    private string _resultText = "";
    private string _resultEmoji = "";
    private int _xpEarned;

    public LessonsViewModel(AppHost host)
    {
        _host = host;
        ContinueCommand = new RelayCommand(Continue);
        CheckCommand = new RelayCommand(CheckScramble);
        RetryCommand = new RelayCommand(() => StartLesson(_lastCategory));
        BackCommand = new RelayCommand(() =>
        {
            _isResult = false;
            _isPlaying = false;
            _isPick = true;
            Refresh();
            RaiseState();
        });
    }

    public ObservableCollection<CategoryOption> Categories { get; } = new();
    public ObservableCollection<ChoiceOption> Options { get; } = new();
    public ObservableCollection<WordToken> AnswerTokens { get; } = new();
    public ObservableCollection<WordToken> BankTokens { get; } = new();

    public ICommand ContinueCommand { get; }
    public ICommand CheckCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand BackCommand { get; }

    public bool IsPick => _isPick;
    public bool IsPlaying => _isPlaying;
    public bool IsResult => _isResult;

    public bool IsChoice => _choice is not null;
    public bool IsScramble => _scramble is not null;
    public string Instruction => _index < _exercises.Count ? _exercises[_index].Instruction : "";
    public string PromptText => _choice?.Prompt ?? _scramble?.Prompt ?? "";
    public string ProgressText => _exercises.Count == 0 ? "" : $"{_index + 1} de {_exercises.Count}";
    public double ProgressValue => _exercises.Count == 0 ? 0 : _index * 100.0 / _exercises.Count;
    public string LivesText => string.Concat(Enumerable.Repeat("\u2764\uFE0F", Math.Max(0, _lives)));

    public bool FeedbackVisible => _feedbackVisible;
    public bool FeedbackCorrect => _feedbackCorrect;
    public string FeedbackText => _feedbackText;
    public IBrush FeedbackBrush => _feedbackCorrect ? Palette.CardCorrect : Palette.CardWrong;
    public bool ScrambleChecked => _scrambleChecked;
    public bool CanCheck => AnswerTokens.Count > 0 && !_scrambleChecked;
    public string ContinueLabel =>
        _lives <= 0 || _index + 1 >= _exercises.Count ? "Ver resultado" : "Continuar";

    public bool Passed => _passed;
    public string ResultTitle => _resultTitle;
    public string ResultText => _resultText;
    public string ResultEmoji => _resultEmoji;
    public string XpText => $"+{_xpEarned} XP";

    public void Refresh()
    {
        var words = _host.Data.Words;
        Categories.Replace(BuildCategoryOptions(words));
    }

    private List<CategoryOption> BuildCategoryOptions(IReadOnlyList<WordEntry> words)
    {
        var options = new List<CategoryOption>
        {
            new()
            {
                Key = null,
                Label = "\U0001F500  Todas as categorias",
                Detail = $"{Math.Min(QuestionCount, words.Count)} exercícios misturando tudo",
                Command = new RelayCommand(() => StartLesson(null)),
            },
        };

        foreach (var category in EnglishStudy.Models.Categories.All)
        {
            var count = words.Count(w => w.Category == category.Key);
            if (count == 0)
                continue;

            var key = category.Key;
            options.Add(new CategoryOption
            {
                Key = key,
                Label = $"{category.Emoji}  {category.Label}",
                Detail = $"{Math.Min(QuestionCount, count)} exercícios · {count} palavras",
                Command = new RelayCommand(() => StartLesson(key)),
            });
        }

        return options;
    }

    private void StartLesson(string? categoryKey)
    {
        _lastCategory = categoryKey;
        _exercises = _host.Lessons.BuildLesson(_host.Data.Words, categoryKey, QuestionCount);
        _index = 0;
        _lives = StartingLives;
        _correct = 0;
        _xpEarned = 0;
        _passed = false;
        _isPick = false;
        _isResult = false;
        _isPlaying = _exercises.Count > 0;
        ShowCurrent();
        RaiseState();
    }

    private void ShowCurrent()
    {
        Options.Clear();
        AnswerTokens.Clear();
        BankTokens.Clear();
        _feedbackVisible = false;
        _feedbackCorrect = false;
        _feedbackText = "";
        _scrambleChecked = false;

        _choice = null;
        _scramble = null;

        if (_index >= _exercises.Count)
            return;

        switch (_exercises[_index])
        {
            case ChoiceExercise choice:
                _choice = choice;
                foreach (var option in choice.Options)
                    Options.Add(new ChoiceOption(option, PickOption));
                break;

            case ScrambleExercise scramble:
                _scramble = scramble;
                foreach (var token in scramble.Tokens)
                    BankTokens.Add(new WordToken(token, TokenToAnswer));
                break;
        }
    }

    private void PickOption(ChoiceOption option)
    {
        if (_feedbackVisible || _choice is null)
            return;

        var isCorrect = option.Text == _choice.AnswerText;
        foreach (var candidate in Options)
            candidate.Background = candidate.Text == _choice.AnswerText ? Palette.CardCorrect : Palette.Card;
        if (!isCorrect)
            option.Background = Palette.CardWrong;

        SetFeedback(
            isCorrect,
            isCorrect
                ? $"Boa! {_choice.Feedback}"
                : $"Quase! A resposta certa é “{_choice.AnswerText}”. {_choice.Feedback}");
    }

    private void TokenToAnswer(WordToken token)
    {
        if (_scrambleChecked || !BankTokens.Remove(token))
            return;
        AnswerTokens.Add(token);
        RaiseState();
    }

    private void TokenToBank(WordToken token)
    {
        if (_scrambleChecked || !AnswerTokens.Remove(token))
            return;
        BankTokens.Add(token);
        RaiseState();
    }

    private void CheckScramble()
    {
        if (_scramble is null || _scrambleChecked || AnswerTokens.Count == 0)
            return;

        var attempt = string.Join(' ', AnswerTokens.Select(t => t.Text));
        var expected = string.Join(' ', _scramble.Answer);
        var isCorrect = string.Equals(attempt.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);

        _scrambleChecked = true;
        SetFeedback(
            isCorrect,
            isCorrect
                ? $"Perfeito! {_scramble.Feedback}"
                : $"Ficou faltando algo. O certo é: “{_scramble.Feedback}”");
    }

    private void SetFeedback(bool correct, string text)
    {
        _feedbackCorrect = correct;
        _feedbackText = text;
        _feedbackVisible = true;
        if (correct)
            _correct++;
        else
            _lives--;
        RaiseState();
    }

    private void Continue()
    {
        if (!_feedbackVisible)
            return;

        if (_lives <= 0)
        {
            Finish(false);
            return;
        }

        _index++;
        if (_index >= _exercises.Count)
        {
            Finish(_correct >= 4);
            return;
        }

        ShowCurrent();
        RaiseState();
    }

    private void Finish(bool passed)
    {
        _passed = passed;
        _xpEarned = _correct * XpPerCorrect + (passed ? XpPassBonus : 0);

        var profile = _host.Data.Profile;
        profile.Xp += _xpEarned;
        if (passed)
            profile.LessonsDone++;
        Gamification.RegisterStudyActivity(profile, DateTime.Now);
        _host.Save();

        _isPlaying = false;
        _isResult = true;
        _resultEmoji = passed ? "\U0001F389" : "\U0001F4AA";
        _resultTitle = passed ? "Lição concluída!" : "Quase lá!";
        _resultText = passed
            ? $"Você acertou {_correct} de {_exercises.Count}. Continue assim!"
            : $"Você acertou {_correct} de {_exercises.Count} e perdeu as vidas. Tente de novo!";
        RaiseState();
    }

    private void RaiseState()
    {
        foreach (var name in new[]
        {
            nameof(IsPick), nameof(IsPlaying), nameof(IsResult), nameof(IsChoice), nameof(IsScramble),
            nameof(Instruction), nameof(PromptText), nameof(ProgressText), nameof(ProgressValue),
            nameof(LivesText), nameof(FeedbackVisible), nameof(FeedbackCorrect), nameof(FeedbackText),
            nameof(FeedbackBrush), nameof(ScrambleChecked), nameof(CanCheck), nameof(ContinueLabel), nameof(Passed),
            nameof(ResultTitle), nameof(ResultText), nameof(ResultEmoji), nameof(XpText),
        })
            Raise(name);
    }
}
