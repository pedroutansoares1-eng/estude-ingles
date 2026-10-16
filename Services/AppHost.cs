using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>Dependências compartilhadas por todo o app.</summary>
public sealed class AppHost
{
    public AppData Data { get; }
    public JsonStore Store { get; }
    public LessonGenerator Lessons { get; }
    public SpeechService Speech { get; } = new();
    public AiChatClient Ai { get; } = new();
    public Random Rng { get; } = new();

    /// <summary>Pedido de navegação para outra tela (definido pela janela principal).</summary>
    public Action<string>? Navigate { get; set; }

    public AppHost(JsonStore store, AppData data)
    {
        Store = store;
        Data = data;
        Lessons = new LessonGenerator(Rng);
    }

    public void Save() => Store.Save(Data);
}
