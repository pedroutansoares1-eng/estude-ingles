using System.Collections.ObjectModel;
using System.Windows.Input;
using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class ChatViewModel : ObservableBase, IRefreshable
{
    private const string Greeting =
        "Hi! I'm Mia, your English tutor! \U0001F60A\n\n" +
        "(Oi! Eu sou a Mia, sua professora de inglês. Escreva em inglês — e, se precisar, pode pedir explicações em português.)";

    private readonly AppHost _host;

    private ChatSession? _selected;
    private string _input = "";
    private bool _isBusy;
    private string _status = "";

    public ChatViewModel(AppHost host)
    {
        _host = host;
        SendCommand = new RelayCommand(_ => _ = SendAsync(), _ => CanSend);
        NewChatCommand = new RelayCommand(NewChat);
        DeleteChatCommand = new RelayCommand(DeleteChat);

        Starters = new List<CategoryOption>
        {
            new() { Label = "\U0001F44B  Apresentar-se", Detail = "", Command = new RelayCommand(() => SendStarter("Hello! My name is ...")) },
            new() { Label = "\U0001F355  Falar sobre comida", Detail = "", Command = new RelayCommand(() => SendStarter("Let's talk about food. What is your favorite dish?")) },
            new() { Label = "\U0001F4DA  Explicar gramática", Detail = "", Command = new RelayCommand(() => SendStarter("Pode explicar em português quando usar o past simple?")) },
        };
    }

    public ObservableCollection<ChatBubble> Messages { get; } = new();
    public ObservableCollection<ChatSession> Sessions { get; } = new();
    public IReadOnlyList<CategoryOption> Starters { get; }

    public ICommand SendCommand { get; }
    public ICommand NewChatCommand { get; }
    public ICommand DeleteChatCommand { get; }

    public event Action? MessagesChanged;

    public string Endpoint => _host.Data.Settings.AiEndpoint;

    public string Input
    {
        get => _input;
        set
        {
            if (Set(ref _input, value))
            {
                Raise(nameof(CanSend));
                (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ChatSession? SelectedSession
    {
        get => _selected;
        set
        {
            if (!Set(ref _selected, value))
                return;
            LoadMessages();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (Set(ref _isBusy, value))
            {
                Raise(nameof(CanSend));
                (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            if (Set(ref _status, value))
                Raise(nameof(HasStatus));
        }
    }

    public bool HasStatus => !string.IsNullOrWhiteSpace(Status);
    public bool CanSend => !IsBusy && !string.IsNullOrWhiteSpace(Input);
    public string SessionHint => Sessions.Count == 0 ? "Nenhuma conversa ainda." : "";

    public void Refresh()
    {
        if (_host.Data.Chats.Count == 0)
            _host.Data.Chats.Add(CreateSession());

        Sessions.Replace(_host.Data.Chats.OrderByDescending(c => c.StartedAt));

        if (_selected is null || !_host.Data.Chats.Contains(_selected))
            SelectedSession = Sessions.FirstOrDefault();
        else
            LoadMessages();
    }

    private ChatSession CreateSession()
    {
        var session = new ChatSession
        {
            Title = "Conversa nova",
            StartedAt = DateTime.Now,
        };
        session.Messages.Add(new ChatMessage { Role = "assistant", Text = Greeting });
        return session;
    }

    private void NewChat()
    {
        var session = CreateSession();
        _host.Data.Chats.Add(session);
        _host.Save();
        Sessions.Replace(_host.Data.Chats.OrderByDescending(c => c.StartedAt));
        SelectedSession = session;
    }

    private void DeleteChat()
    {
        if (_selected is null)
            return;

        _host.Data.Chats.Remove(_selected);
        _host.Save();
        _selected = null;

        if (_host.Data.Chats.Count == 0)
            _host.Data.Chats.Add(CreateSession());

        Sessions.Replace(_host.Data.Chats.OrderByDescending(c => c.StartedAt));
        SelectedSession = Sessions.FirstOrDefault();
    }

    private void LoadMessages()
    {
        if (_selected is null)
        {
            Messages.Clear();
        }
        else
        {
            Messages.Replace(_selected.Messages.Select(ToBubble));
        }
        Raise(nameof(SessionHint));
        MessagesChanged?.Invoke();
    }

    private static ChatBubble ToBubble(ChatMessage message) => message.Role == "user"
        ? new ChatBubble { Text = message.Text, Align = Avalonia.Layout.HorizontalAlignment.Right, Background = Palette.Accent, Foreground = Palette.White }
        : new ChatBubble { Text = message.Text, Align = Avalonia.Layout.HorizontalAlignment.Left, Background = Palette.Card, Foreground = Palette.Text };

    private void SendStarter(string text)
    {
        Input = text;
        _ = SendAsync();
    }

    private async Task SendAsync()
    {
        var text = Input.Trim();
        if (text.Length == 0 || IsBusy)
            return;

        if (_selected is null)
            NewChat();

        var session = _selected;
        if (session is null)
            return;

        Input = "";
        AddMessage(session, "user", text);

        if (session.Title == "Conversa nova")
        {
            session.Title = text.Length <= 32 ? text : text[..32] + "…";
            Sessions.Replace(_host.Data.Chats.OrderByDescending(c => c.StartedAt));
            SelectedSession = session;
        }

        IsBusy = true;
        Status = "Mia está digitando…";

        try
        {
            var reply = await _host.Ai.SendAsync(_host.Data.Settings, session.Messages);
            AddMessage(session, "assistant", reply);
            Status = "";
        }
        catch (Exception ex)
        {
            AddMessage(session, "assistant",
                "\u26A0\uFE0F Não consegui falar com a IA agora.\n\n" +
                "Verifique se o LM Studio está aberto com o servidor ligado em:\n" +
                $"{Endpoint}\n\nDetalhe técnico: {ex.Message}");
            Status = "Sem conexão com a IA. Confira o endereço nas Configurações.";
        }
        finally
        {
            IsBusy = false;
            _host.Save();
        }
    }

    private void AddMessage(ChatSession session, string role, string text)
    {
        var message = new ChatMessage { Role = role, Text = text };
        session.Messages.Add(message);
        Messages.Add(ToBubble(message));
        MessagesChanged?.Invoke();
    }
}
