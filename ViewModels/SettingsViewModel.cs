using System.Windows.Input;
using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.ViewModels;

public sealed class SettingsViewModel : ObservableBase, IRefreshable
{
    private readonly AppHost _host;

    private string _name = "";
    private string _newCardsText = "";
    private string _endpoint = "";
    private string _model = "";
    private string _apiKey = "";
    private string _saveStatus = "";
    private bool _saveError;
    private string _testStatus = "";
    private bool _isTesting;
    private bool _confirmReset;
    private string _resetButtonText = "\U0001F5D1\uFE0F  Apagar progresso";
    private string _resetStatus = "";

    public SettingsViewModel(AppHost host)
    {
        _host = host;
        SaveCommand = new RelayCommand(Save);
        TestAiCommand = new RelayCommand(_ => _ = TestAiAsync());
        ResetCommand = new RelayCommand(ResetProgress);
    }

    public ICommand SaveCommand { get; }
    public ICommand TestAiCommand { get; }
    public ICommand ResetCommand { get; }

    public string Name { get => _name; set => Set(ref _name, value); }
    public string NewCardsText { get => _newCardsText; set => Set(ref _newCardsText, value); }
    public string Endpoint { get => _endpoint; set => Set(ref _endpoint, value); }
    public string Model { get => _model; set => Set(ref _model, value); }
    public string ApiKey { get => _apiKey; set => Set(ref _apiKey, value); }
    public string SaveStatus { get => _saveStatus; private set => Set(ref _saveStatus, value); }
    public bool SaveError { get => _saveError; private set => Set(ref _saveError, value); }
    public string TestStatus { get => _testStatus; private set => Set(ref _testStatus, value); }
    public bool IsTesting { get => _isTesting; private set => Set(ref _isTesting, value); }
    public string ResetButtonText { get => _resetButtonText; private set => Set(ref _resetButtonText, value); }
    public string ResetStatus { get => _resetStatus; private set => Set(ref _resetStatus, value); }

    public string DataPath => _host.Store.DataPath;

    public string SpeechNote => _host.Speech.IsAvailable
        ? "\u2705 Pronúncia ativada: as palavras podem ser ouvidas nos flashcards."
        : "\U0001F507 Pronúncia desativada. Para ouvir as palavras, instale o espeak-ng (sudo apt install espeak-ng) e reabra o app.";

    public string AiNote =>
        "O app conversa com qualquer servidor compatível com a API da OpenAI. " +
        "Com o LM Studio aberto, ligue o servidor local (aba Developer → Start Server). " +
        "O endereço padrão é http://localhost:1234/v1.";

    public void Refresh()
    {
        var data = _host.Data;
        Name = data.Profile.Name;
        NewCardsText = data.Settings.NewCardsPerDay.ToString();
        Endpoint = data.Settings.AiEndpoint;
        Model = data.Settings.AiModel;
        ApiKey = data.Settings.AiApiKey;
        Raise(nameof(DataPath));
        Raise(nameof(SpeechNote));
    }

    private void Save()
    {
        var data = _host.Data;

        if (int.TryParse(NewCardsText.Trim(), out var limit))
            data.Settings.NewCardsPerDay = Math.Clamp(limit, 0, 100);

        data.Profile.Name = Name.Trim();
        data.Settings.AiEndpoint = string.IsNullOrWhiteSpace(Endpoint)
            ? "http://localhost:1234/v1"
            : Endpoint.Trim();
        data.Settings.AiModel = Model.Trim();
        data.Settings.AiApiKey = ApiKey.Trim();

        _host.Save();
        NewCardsText = data.Settings.NewCardsPerDay.ToString();
        Endpoint = data.Settings.AiEndpoint;
        SaveError = false;
        SaveStatus = "Configurações salvas \u2705";
    }

    private async Task TestAiAsync()
    {
        IsTesting = true;
        TestStatus = "Testando conexão…";

        try
        {
            var models = await _host.Ai.ListModelsAsync(Endpoint.Trim(), ApiKey.Trim());
            if (models.Count == 0)
            {
                TestStatus = "\u26A0\uFE0F Conectou, mas nenhum modelo está carregado no servidor.";
            }
            else
            {
                var preview = string.Join(", ", models.Take(3));
                var extra = models.Count > 3 ? $" (+{models.Count - 3})" : "";
                TestStatus = $"\u2705 Conectado! Modelos: {preview}{extra}";

                if (string.IsNullOrWhiteSpace(Model))
                {
                    Model = models[0];
                    _host.Data.Settings.AiModel = models[0];
                    _host.Save();
                }
            }
        }
        catch (Exception ex)
        {
            TestStatus = $"\u274C Não consegui conectar. Detalhe: {ex.Message}";
        }
        finally
        {
            IsTesting = false;
        }
    }

    private void ResetProgress()
    {
        if (!_confirmReset)
        {
            _confirmReset = true;
            ResetButtonText = "\u26A0\uFE0F  Clique de novo para confirmar";
            ResetStatus = "Isso apaga XP, sequência e todo o progresso dos cartões. Suas palavras ficam.";
            return;
        }

        var data = _host.Data;
        var name = data.Profile.Name;
        data.Profile = new UserProfile { Name = name };
        data.Cards.Clear();
        foreach (var word in data.Words)
            data.Cards.Add(SrsEngine.NewCard(word.Id));

        _host.Save();

        _confirmReset = false;
        ResetButtonText = "\U0001F5D1\uFE0F  Apagar progresso";
        ResetStatus = "Progresso zerado. Começando de novo!";
    }
}
