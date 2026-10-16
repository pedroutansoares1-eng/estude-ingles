using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class MainWindowViewModel : ObservableBase
{
    private readonly Dictionary<string, ObservableBase> _pages = new();
    private string _currentKey = "home";
    private NavItem _currentNav;

    public IReadOnlyList<NavItem> NavItems { get; }

    public MainWindowViewModel(AppHost host)
    {
        _pages["home"] = new DashboardViewModel(host);
        _pages["flashcards"] = new FlashcardsViewModel(host);
        _pages["lessons"] = new LessonsViewModel(host);
        _pages["quiz"] = new QuizViewModel(host);
        _pages["chat"] = new ChatViewModel(host);
        _pages["vocab"] = new VocabularyViewModel(host);
        _pages["settings"] = new SettingsViewModel(host);

        NavItems = new List<NavItem>
        {
            new("home", "\U0001F3E0", "Início"),
            new("flashcards", "\U0001F0CF", "Flashcards"),
            new("lessons", "\U0001F4DA", "Lições"),
            new("quiz", "\u2753", "Gramática"),
            new("chat", "\U0001F4AC", "Conversar"),
            new("vocab", "\U0001F4D6", "Meu Vocabulário"),
            new("settings", "\u2699\uFE0F", "Configurações"),
        };
        _currentNav = NavItems[0];

        host.Navigate = Navigate;
        RefreshCurrent();
    }

    public ObservableBase CurrentPage => _pages[_currentKey];

    public NavItem CurrentNav
    {
        get => _currentNav;
        set
        {
            if (value is not null)
                Navigate(value.Key);
        }
    }

    public void Navigate(string key)
    {
        if (!_pages.ContainsKey(key))
            return;

        var changed = _currentKey != key;
        _currentKey = key;

        var item = NavItems.First(n => n.Key == key);
        if (!ReferenceEquals(_currentNav, item))
        {
            _currentNav = item;
            Raise(nameof(CurrentNav));
        }

        if (changed)
            Raise(nameof(CurrentPage));

        RefreshCurrent();
    }

    private void RefreshCurrent()
    {
        if (_pages[_currentKey] is IRefreshable refreshable)
            refreshable.Refresh();
    }
}
