using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>Cliente para qualquer servidor compatível com a API da OpenAI (ex.: LM Studio local).</summary>
public sealed class AiChatClient
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(120) };

    public const string SystemPrompt = """
        Você é a Mia, uma professora de inglês simpática que conversa com um estudante brasileiro.
        Regras:
        1. Responda SEMPRE em inglês simples (nível A2), com frases curtas e naturais.
        2. Termine com uma pergunta curta para manter a conversa.
        3. Se o estudante errar o inglês, corrija gentilmente em uma linha separada no formato: [Correção: "frase correta"]
        4. Se ele pedir explicação ou escrever em português, explique em português.
        5. Seja encorajador e use no máximo um emoji.
        """;

    public async Task<List<string>> ListModelsAsync(string endpoint, string? apiKey)
    {
        var url = endpoint.TrimEnd('/') + "/models";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await Http.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

        var models = new List<string>();
        if (doc.RootElement.TryGetProperty("data", out var data))
            foreach (var item in data.EnumerateArray())
                if (item.TryGetProperty("id", out var id) && id.GetString() is { Length: > 0 } name)
                    models.Add(name);
        return models;
    }

    public async Task<string> SendAsync(AppSettings settings, IReadOnlyList<ChatMessage> history)
    {
        var url = settings.AiEndpoint.TrimEnd('/') + "/chat/completions";

        var messages = new List<Dictionary<string, object>>
        {
            new() { ["role"] = "system", ["content"] = SystemPrompt },
        };
        foreach (var message in history.TakeLast(12))
            messages.Add(new Dictionary<string, object>
            {
                ["role"] = message.Role == "user" ? "user" : "assistant",
                ["content"] = message.Text,
            });

        var body = new Dictionary<string, object>
        {
            ["model"] = string.IsNullOrWhiteSpace(settings.AiModel) ? "local-model" : settings.AiModel,
            ["messages"] = messages,
            ["temperature"] = 0.7,
            ["max_tokens"] = 400,
            ["stream"] = false,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
        if (!string.IsNullOrWhiteSpace(settings.AiApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.AiApiKey);

        using var response = await Http.SendAsync(request).ConfigureAwait(false);
        var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"O servidor respondeu {(int)response.StatusCode}: {Shorten(payload)}");

        using var doc = JsonDocument.Parse(payload);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return string.IsNullOrWhiteSpace(content) ? "(sem resposta)" : content.Trim();
    }

    private static string Shorten(string text) =>
        text.Length <= 200 ? text : text[..200] + "…";
}
