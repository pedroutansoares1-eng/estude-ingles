namespace EnglishStudy.Models;

public sealed class GrammarQuestion
{
    public int Id { get; set; }
    public string Topic { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string[] Options { get; set; } = Array.Empty<string>();
    public int AnswerIndex { get; set; }
    public string Explanation { get; set; } = "";
}

public sealed record GrammarTopic(string Key, string Label, string Emoji);

public static class GrammarTopics
{
    public static readonly IReadOnlyList<GrammarTopic> All = new List<GrammarTopic>
    {
        new("present", "Presente Simples", "\u23F0"),
        new("past", "Passado Simples", "\u23EA"),
        new("future", "Futuro", "\u23E9"),
        new("irregular", "Verbos Irregulares", "\U0001F300"),
        new("prepositions", "Preposições", "\U0001F4CD"),
        new("articles", "Artigos", "\U0001F524"),
        new("comparatives", "Comparativos", "\u2696\uFE0F"),
        new("questions", "Perguntas", "\u2753"),
    };

    public static GrammarTopic Get(string key) =>
        All.FirstOrDefault(t => t.Key == key) ?? All[^1];

    public static string Label(string key) => Get(key).Label;
    public static string Emoji(string key) => Get(key).Emoji;
}
