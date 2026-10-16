using System.Collections.ObjectModel;
using System.Windows.Input;
using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class VocabularyViewModel : ObservableBase, IRefreshable
{
    private readonly AppHost _host;

    private CategoryOption? _selectedFilter;
    private CategoryOption? _selectedNewCategory;
    private bool _onlyMine;
    private string _newPt = "";
    private string _newEn = "";
    private string _newExampleEn = "";
    private string _newExamplePt = "";
    private string _addStatus = "";
    private bool _addError;

    public VocabularyViewModel(AppHost host)
    {
        _host = host;
        AddCommand = new RelayCommand(AddWord);
        ClearFilterCommand = new RelayCommand(() =>
        {
            _onlyMine = false;
            Raise(nameof(OnlyMine));
            SelectedFilter = Filters.FirstOrDefault();
        });
    }

    public ObservableCollection<WordRow> Rows { get; } = new();
    public ObservableCollection<CategoryOption> Filters { get; } = new();
    public ObservableCollection<CategoryOption> CategoryChoices { get; } = new();

    public ICommand AddCommand { get; }
    public ICommand ClearFilterCommand { get; }

    public string NewPt { get => _newPt; set => Set(ref _newPt, value); }
    public string NewEn { get => _newEn; set => Set(ref _newEn, value); }
    public string NewExampleEn { get => _newExampleEn; set => Set(ref _newExampleEn, value); }
    public string NewExamplePt { get => _newExamplePt; set => Set(ref _newExamplePt, value); }
    public string AddStatus { get => _addStatus; private set => Set(ref _addStatus, value); }
    public bool AddError { get => _addError; private set => Set(ref _addError, value); }

    public bool OnlyMine
    {
        get => _onlyMine;
        set
        {
            if (Set(ref _onlyMine, value))
                RebuildRows();
        }
    }

    public CategoryOption? SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (Set(ref _selectedFilter, value))
                RebuildRows();
        }
    }

    public CategoryOption? SelectedNewCategory
    {
        get => _selectedNewCategory;
        set => Set(ref _selectedNewCategory, value);
    }

    public bool IsEmptyList => Rows.Count == 0;
    public string RowsCountText => Rows.Count == 1 ? "1 palavra" : $"{Rows.Count} palavras";

    public void Refresh()
    {
        if (Filters.Count == 0)
        {
            Filters.Add(new CategoryOption { Key = null, Label = "\U0001F4D6  Todas as categorias" });
            foreach (var category in EnglishStudy.Models.Categories.All)
                Filters.Add(new CategoryOption { Key = category.Key, Label = $"{category.Emoji}  {category.Label}" });
            _selectedFilter = Filters[0];
            Raise(nameof(SelectedFilter));
        }

        if (CategoryChoices.Count == 0)
        {
            foreach (var category in EnglishStudy.Models.Categories.All)
                CategoryChoices.Add(new CategoryOption { Key = category.Key, Label = $"{category.Emoji}  {category.Label}" });
            _selectedNewCategory = CategoryChoices[0];
            Raise(nameof(SelectedNewCategory));
        }

        RebuildRows();
    }

    private void RebuildRows()
    {
        var key = _selectedFilter?.Key;
        var query = _host.Data.Words.AsEnumerable();
        if (key is not null)
            query = query.Where(w => w.Category == key);
        if (_onlyMine)
            query = query.Where(w => w.IsUser);

        query = query.OrderBy(w => w.Category).ThenBy(w => w.Pt);
        Rows.Replace(query.Select(BuildRow));

        Raise(nameof(IsEmptyList));
        Raise(nameof(RowsCountText));
    }

    private WordRow BuildRow(WordEntry word) => new()
    {
        Pt = word.Pt,
        En = word.En,
        CategoryLabel = $"{EnglishStudy.Models.Categories.Emoji(word.Category)} {EnglishStudy.Models.Categories.Label(word.Category)}",
        ExampleEn = word.ExampleEn,
        ExamplePt = word.ExamplePt,
        IsUser = word.IsUser,
        DeleteCommand = word.IsUser ? new RelayCommand(() => DeleteWord(word.Id)) : null,
    };

    private void AddWord()
    {
        var pt = NewPt.Trim();
        var en = NewEn.Trim();
        if (pt.Length == 0 || en.Length == 0)
        {
            AddStatus = "⚠️ Preencha pelo menos o português e o inglês.";
            AddError = true;
            return;
        }

        var word = new WordEntry
        {
            Id = "u" + Guid.NewGuid().ToString("N")[..8],
            Pt = pt,
            En = en,
            ExampleEn = string.IsNullOrWhiteSpace(NewExampleEn) ? null : NewExampleEn.Trim(),
            ExamplePt = string.IsNullOrWhiteSpace(NewExamplePt) ? null : NewExamplePt.Trim(),
            Category = SelectedNewCategory?.Key ?? "conversation",
            IsUser = true,
        };

        _host.Data.Words.Add(word);
        _host.Data.Cards.Add(SrsEngine.NewCard(word.Id));
        _host.Save();

        NewPt = "";
        NewEn = "";
        NewExampleEn = "";
        NewExamplePt = "";
        AddError = false;
        AddStatus = $"✅ “{word.En}” foi adicionada e já aparece nos flashcards!";

        RebuildRows();
    }

    private void DeleteWord(string id)
    {
        _host.Data.Words.RemoveAll(w => w.Id == id);
        _host.Data.Cards.RemoveAll(c => c.WordId == id);
        _host.Save();
        AddError = false;
        AddStatus = "🗑️ Palavra removida.";
        RebuildRows();
    }
}
