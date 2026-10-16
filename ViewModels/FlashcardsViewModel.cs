using System.Windows.Input;
using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class FlashcardsViewModel : ObservableBase, IRefreshable
{
    private const int MaxCardsPerSession = 30;

    private sealed record ReviewItem(SrsCard Card, WordEntry Word);

    private readonly AppHost _host;
    private readonly List<ReviewItem> _queue = new();

    private ReviewItem? _current;
    private bool _hasSession;
    private bool _isEmpty;
    private bool _finished;
    private bool _answerShown;
    private int _done;
    private int _good;
    private int _again;
    private int _sessionXp;

    public FlashcardsViewModel(AppHost host)
    {
        _host = host;
        RevealCommand = new RelayCommand(() => { _answerShown = true; RaiseState(); });
        RateCommand = new RelayCommand(parameter =>
        {
            if (parameter is string text && Enum.TryParse<SrsRating>(text, out var rating))
                Rate(rating);
        });
        SpeakCommand = new RelayCommand(() =>
        {
            if (_current is not null)
                _host.Speech.Speak(_answerShown ? _current.Word.En : _current.Word.En);
        });
        RestartCommand = new RelayCommand(BuildSession);
    }

    public ICommand RevealCommand { get; }
    public ICommand RateCommand { get; }
    public ICommand SpeakCommand { get; }
    public ICommand RestartCommand { get; }

    public bool HasSession => _hasSession;
    public bool IsEmpty => _isEmpty;
    public bool IsFinished => _finished;
    public bool IsAnswerShown => _answerShown;
    public bool CanSpeak => _host.Speech.IsAvailable;

    public string Prompt => _current?.Word.Pt ?? "";
    public string Answer => _current?.Word.En ?? "";
    public string ExampleEn => _current?.Word.ExampleEn ?? "";
    public string ExamplePt => _current?.Word.ExamplePt ?? "";
    public bool HasExample => !string.IsNullOrWhiteSpace(ExampleEn) && _answerShown;
    public string CategoryLabel => _current is null
        ? ""
        : $"{Categories.Emoji(_current.Word.Category)} {Categories.Label(_current.Word.Category)}";
    public string CardType => _current is null
        ? ""
        : SrsEngine.IsNew(_current.Card) ? "Palavra nova" : "Revisão";

    private int Total => _done + _queue.Count;
    public string ProgressText => $"{_done} de {Total}";
    public double ProgressValue => Total == 0 ? 0 : _done * 100.0 / Total;
    public string QueueText => _queue.Count == 1 ? "1 cartão restante" : $"{_queue.Count} cartões restantes";

    public string ReviewedText => $"{_done} revistos";
    public string GoodText => $"{_good} acertos";
    public string AgainText => $"{_again} para rever";
    public string SessionXpText => $"+{_sessionXp} XP nesta sessão";

    public void Refresh()
    {
        // Mantém a sessão em andamento; só monta uma nova quando não há nada em curso.
        if (!_hasSession || _finished || _isEmpty)
            BuildSession();
    }

    private void BuildSession()
    {
        _queue.Clear();
        var data = _host.Data;
        var today = DateTime.Today;
        var words = data.Words.ToDictionary(w => w.Id, w => w);

        var due = data.Cards
            .Where(c => !SrsEngine.IsNew(c) && SrsEngine.IsDue(c, today))
            .OrderBy(c => c.Due)
            .ToList();

        var newLimit = Math.Max(0, data.Settings.NewCardsPerDay);
        var fresh = data.Cards
            .Where(SrsEngine.IsNew)
            .Take(newLimit)
            .ToList();

        foreach (var card in due.Concat(fresh))
            if (words.TryGetValue(card.WordId, out var word))
                _queue.Add(new ReviewItem(card, word));

        // Limita tamanho da sessão para não cansar o usuário.
        if (_queue.Count > MaxCardsPerSession)
            _queue.RemoveRange(MaxCardsPerSession, _queue.Count - MaxCardsPerSession);

        _done = 0;
        _good = 0;
        _again = 0;
        _sessionXp = 0;
        _answerShown = false;
        _finished = false;
        _isEmpty = _queue.Count == 0;
        _hasSession = _queue.Count > 0;
        _current = _queue.Count > 0 ? _queue[0] : null;
        RaiseState();
    }

    private void Rate(SrsRating rating)
    {
        if (_current is null || !_answerShown)
            return;

        SrsEngine.Apply(_current.Card, rating, DateTime.Now);

        var profile = _host.Data.Profile;
        profile.TotalReviews++;
        profile.Xp += SrsEngine.XpFor(rating);
        _sessionXp += SrsEngine.XpFor(rating);

        if (rating == SrsRating.Again)
            _again++;
        else
        {
            profile.TotalCorrect++;
            _good++;
        }

        Gamification.RegisterStudyActivity(profile, DateTime.Now);

        var item = _current;
        _queue.RemoveAt(0);
        if (rating == SrsRating.Again)
            _queue.Add(item); // reaparece mais tarde na mesma sessão

        _done++;
        _answerShown = false;
        _host.Save();

        if (_queue.Count == 0)
        {
            _current = null;
            _finished = true;
            _hasSession = false;
        }
        else
        {
            _current = _queue[0];
        }

        RaiseState();
    }

    private void RaiseState()
    {
        foreach (var name in new[]
        {
            nameof(HasSession), nameof(IsEmpty), nameof(IsFinished), nameof(IsAnswerShown),
            nameof(Prompt), nameof(Answer), nameof(ExampleEn), nameof(ExamplePt), nameof(HasExample),
            nameof(CategoryLabel), nameof(CardType), nameof(ProgressText), nameof(ProgressValue),
            nameof(QueueText), nameof(ReviewedText), nameof(GoodText), nameof(AgainText),
            nameof(SessionXpText), nameof(CanSpeak),
        })
            Raise(name);
    }
}
