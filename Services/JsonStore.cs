using System.Text.Json;
using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>Persistência local em JSON (salva em ~/.local/share/EnglishStudy/data.json).</summary>
public sealed class JsonStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string DataPath { get; }

    public JsonStore(string? path = null)
    {
        if (path is null)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var baseDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (string.IsNullOrWhiteSpace(baseDir))
                baseDir = Path.Combine(home, ".local", "share");
            var dir = Path.Combine(baseDir, "EnglishStudy");
            Directory.CreateDirectory(dir);
            path = Path.Combine(dir, "data.json");
        }
        DataPath = path;
    }

    public AppData Load()
    {
        AppData data;
        var existed = File.Exists(DataPath);
        if (existed)
        {
            try
            {
                data = JsonSerializer.Deserialize<AppData>(File.ReadAllText(DataPath), Options) ?? new AppData();
            }
            catch
            {
                // Arquivo corrompido: começa de novo em vez de travar o app.
                data = new AppData();
            }
        }
        else
        {
            data = new AppData();
        }

        Normalize(data);

        // Primeira execução: grava o conteúdo embutido para o usuário já ter um arquivo de dados.
        if (!existed)
            Save(data);

        return data;
    }

    public void Save(AppData data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, Options);
            var tmp = DataPath + ".tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, DataPath, overwrite: true);
        }
        catch
        {
            // Não deixa uma falha de gravação derrubar o app.
        }
    }

    /// <summary>Garante conteúdo embutido e consistência entre palavras e cartões.</summary>
    private static void Normalize(AppData data)
    {
        if (data.SeedVersion < SeedWords.Version)
        {
            var ids = data.Words.Select(w => w.Id).ToHashSet();
            foreach (var word in SeedWords.All())
                if (ids.Add(word.Id))
                    data.Words.Add(word);
            data.SeedVersion = SeedWords.Version;
        }

        var wordIds = data.Words.Select(w => w.Id).ToHashSet();
        var cardIds = data.Cards.Select(c => c.WordId).ToHashSet();
        data.Cards.RemoveAll(c => !wordIds.Contains(c.WordId));
        foreach (var word in data.Words)
            if (cardIds.Add(word.Id))
                data.Cards.Add(SrsEngine.NewCard(word.Id));

        if (string.IsNullOrWhiteSpace(data.Settings.AiEndpoint))
            data.Settings.AiEndpoint = "http://localhost:1234/v1";
    }
}
