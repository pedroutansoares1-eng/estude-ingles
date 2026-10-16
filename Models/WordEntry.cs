namespace EnglishStudy.Models;

public sealed class WordEntry
{
    public string Id { get; set; } = "";
    public string Pt { get; set; } = "";
    public string En { get; set; } = "";
    public string? ExampleEn { get; set; }
    public string? ExamplePt { get; set; }
    public string Category { get; set; } = "conversation";
    public bool IsUser { get; set; }
}

public sealed record WordCategory(string Key, string Label, string Emoji);

public static class Categories
{
    public static readonly IReadOnlyList<WordCategory> All = new List<WordCategory>
    {
        new("greetings", "Cumprimentos", "\U0001F44B"),
        new("routine", "Rotina do Dia a Dia", "\U0001F4C5"),
        new("food", "Comidas e Bebidas", "\U0001F37D\uFE0F"),
        new("home", "Casa e Família", "\U0001F3E0"),
        new("travel", "Viagem", "\u2708\uFE0F"),
        new("work", "Trabalho e Estudo", "\U0001F4BC"),
        new("conversation", "Frases Úteis", "\U0001F4AC"),
    };

    public static WordCategory Get(string key) =>
        All.FirstOrDefault(c => c.Key == key) ?? All[^1];

    public static string Label(string key) => Get(key).Label;
    public static string Emoji(string key) => Get(key).Emoji;
}
