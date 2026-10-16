using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace EnglishStudy.ViewModels;

/// <summary>Cores usadas pelos view models para dar feedback visual.</summary>
public static class Palette
{
    public static readonly IBrush Card = Brush("#28374D");
    public static readonly IBrush CardCorrect = Brush("#14532D");
    public static readonly IBrush CardWrong = Brush("#7F1D1D");
    public static readonly IBrush Primary = Brush("#22C55E");
    public static readonly IBrush Accent = Brush("#3B82F6");
    public static readonly IBrush Danger = Brush("#EF4444");
    public static readonly IBrush Warn = Brush("#F59E0B");
    public static readonly IBrush Dim = Brush("#94A3B8");
    public static readonly IBrush Text = Brush("#E2E8F0");
    public static readonly IBrush White = Brush("#FFFFFF");

    private static IBrush Brush(string hex) =>
        new SolidColorBrush(Color.Parse(hex));
}

/// <summary>Item de menu lateral.</summary>
public sealed class NavItem
{
    public string Key { get; }
    public string Display { get; }

    public NavItem(string key, string emoji, string label)
    {
        Key = key;
        Display = $"{emoji}   {label}";
    }
}

/// <summary>Uma alternativa clicável (quiz/lição) com feedback de cor.</summary>
public sealed class ChoiceOption : ObservableBase
{
    private IBrush _background = Palette.Card;

    public string Text { get; }
    public ICommand Command { get; }

    public IBrush Background
    {
        get => _background;
        set => Set(ref _background, value);
    }

    public ChoiceOption(string text, Action<ChoiceOption> onPick)
    {
        Text = text;
        Command = new RelayCommand(_ => onPick(this));
    }
}

/// <summary>Palavra clicável do exercício de montar frase.</summary>
public sealed class WordToken : ObservableBase
{
    public string Text { get; }
    public ICommand Command { get; }

    public WordToken(string text, Action<WordToken> onClick)
    {
        Text = text;
        Command = new RelayCommand(_ => onClick(this));
    }
}

/// <summary>Bolha de mensagem no chat.</summary>
public sealed class ChatBubble
{
    public string Text { get; init; } = "";
    public HorizontalAlignment Align { get; init; } = HorizontalAlignment.Left;
    public IBrush Background { get; init; } = Palette.Card;
    public IBrush Foreground { get; init; } = Palette.Text;
}

/// <summary>Opção de categoria usada nas telas de lição e vocabulário.</summary>
public sealed class CategoryOption
{
    public string? Key { get; init; }
    public string Label { get; init; } = "";
    public string Detail { get; init; } = "";
    public ICommand? Command { get; init; }
}

/// <summary>Linha da lista de vocabulário.</summary>
public sealed class WordRow
{
    public string Pt { get; init; } = "";
    public string En { get; init; } = "";
    public string CategoryLabel { get; init; } = "";
    public string? ExampleEn { get; init; }
    public string? ExamplePt { get; init; }
    public bool IsUser { get; init; }
    public ICommand? DeleteCommand { get; init; }
    public bool HasExample => !string.IsNullOrWhiteSpace(ExampleEn);
}

/// <summary>Coleção auxiliar para manter a lista sempre ordenada.</summary>
public static class ObservableExtensions
{
    public static void Replace<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
            collection.Add(item);
    }
}
