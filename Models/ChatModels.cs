namespace EnglishStudy.Models;

public sealed class ChatMessage
{
    public string Role { get; set; } = "assistant"; // user | assistant
    public string Text { get; set; } = "";
    public DateTime At { get; set; } = DateTime.Now;
}

public sealed class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "";
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public List<ChatMessage> Messages { get; set; } = new();
}

public sealed class AppData
{
    public int SeedVersion { get; set; }
    public UserProfile Profile { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
    public List<WordEntry> Words { get; set; } = new();
    public List<SrsCard> Cards { get; set; } = new();
    public List<ChatSession> Chats { get; set; } = new();
}
