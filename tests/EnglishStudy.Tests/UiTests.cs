using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using EnglishStudy.Models;
using EnglishStudy.Services;
using EnglishStudy.ViewModels;
using EnglishStudy.Views;

namespace EnglishStudy.Tests;

/// <summary>
/// Renderiza cada tela fora da tela (headless + Skia) para garantir que o
/// mapeamento tela ↔ view model funciona e que nenhuma tela quebra ao abrir.
/// As imagens saem em /tmp/opencode/page-*.png para inspeção visual.
/// </summary>
public class UiRenderTests
{
    private const string ShotDir = "/tmp/opencode";

    private static (MainWindowViewModel Shell, MainWindow Window, string Path) BuildWindow()
    {
        var path = Path.Combine(Path.GetTempPath(), $"englishstudy-ui-{Guid.NewGuid():N}.json");
        var store = new JsonStore(path);
        var data = store.Load();
        var host = new AppHost(store, data);
        var shell = new MainWindowViewModel(host);
        var window = new MainWindow { DataContext = shell };
        return (shell, window, path);
    }

    [AvaloniaTheory]
    [InlineData("home")]
    [InlineData("flashcards")]
    [InlineData("lessons")]
    [InlineData("quiz")]
    [InlineData("chat")]
    [InlineData("vocab")]
    [InlineData("settings")]
    public void EveryPage_Renders_WithoutErrors(string key)
    {
        var (shell, window, path) = BuildWindow();
        try
        {
            window.Show();
            shell.Navigate(key);

            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();

            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            Assert.True(frame!.PixelSize.Width > 800, "a janela principal deveria renderizar a largura toda");
            Assert.Equal(key, shell.CurrentNav.Key);

            if (Directory.Exists(ShotDir))
                frame.Save(Path.Combine(ShotDir, $"page-{key}.png"));

            window.Close();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void MainWindow_Renders_AtMinimumSize()
    {
        var (shell, window, path) = BuildWindow();
        try
        {
            window.Width = window.MinWidth;
            window.Height = window.MinHeight;
            window.Show();
            shell.Navigate("flashcards");

            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();

            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);

            if (Directory.Exists(ShotDir))
                frame!.Save(Path.Combine(ShotDir, "page-small.png"));

            window.Close();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void Flashcards_ShowsFirstNewCard_AndRevealsAnswer()
    {
        var path = Path.Combine(Path.GetTempPath(), $"englishstudy-ui-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonStore(path);
            var host = new AppHost(store, store.Load());
            var vm = new FlashcardsViewModel(host);

            vm.Refresh();

            Assert.True(vm.HasSession);
            Assert.False(vm.IsAnswerShown);
            Assert.False(string.IsNullOrWhiteSpace(vm.Prompt));
            Assert.False(string.IsNullOrWhiteSpace(vm.Answer));
            Assert.Equal("Palavra nova", vm.CardType);

            vm.RevealCommand.Execute(null);
            Assert.True(vm.IsAnswerShown);

            var queueBefore = vm.QueueText;
            vm.RateCommand.Execute("Good");
            Assert.Equal(1, host.Data.Profile.TotalReviews);
            Assert.True(host.Data.Profile.Xp > 0);
            Assert.Equal(1, host.Data.Profile.StreakDays);
            Assert.NotEqual(queueBefore, vm.QueueText);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void Lesson_CanBePlayedToTheEnd()
    {
        var path = Path.Combine(Path.GetTempPath(), $"englishstudy-ui-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonStore(path);
            var host = new AppHost(store, store.Load());
            var vm = new LessonsViewModel(host);
            vm.Refresh();

            Assert.True(vm.IsPick);
            Assert.NotEmpty(vm.Categories);

            // Começa a primeira lição disponível.
            vm.Categories[0].Command!.Execute(null);
            Assert.True(vm.IsPlaying);

            var guard = 0;
            while (vm.IsPlaying && guard++ < 60)
            {
                if (vm.IsChoice)
                {
                    // Percorre o fluxo respondendo sempre a primeira alternativa
                    // (o objetivo aqui é exercitar a tela inteira, não acertar tudo).
                    Assert.NotEmpty(vm.Options);
                    vm.Options[0].Command.Execute(null);
                }
                else if (vm.IsScramble)
                {
                    while (vm.BankTokens.Count > 0)
                        vm.BankTokens[0].Command.Execute(null);
                    vm.CheckCommand.Execute(null);
                }

                Assert.True(vm.FeedbackVisible);
                vm.ContinueCommand.Execute(null);
            }

            Assert.True(vm.IsResult);
            Assert.False(vm.IsPlaying);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void Quiz_CanBePlayedToTheEnd()
    {
        var path = Path.Combine(Path.GetTempPath(), $"englishstudy-ui-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonStore(path);
            var host = new AppHost(store, store.Load());
            var vm = new QuizViewModel(host);
            vm.Refresh();

            Assert.True(vm.IsPick);
            vm.Topics[0].Command!.Execute(null);
            Assert.True(vm.IsPlaying);

            var guard = 0;
            while (vm.IsPlaying && guard++ < 40)
            {
                Assert.NotEmpty(vm.Options);
                vm.Options[0].Command.Execute(null);
                vm.ContinueCommand.Execute(null);
            }

            Assert.True(vm.IsResult);
            Assert.Equal(1, host.Data.Profile.QuizzesDone);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void Vocabulary_AddsWord_ThatBecomesACard()
    {
        var path = Path.Combine(Path.GetTempPath(), $"englishstudy-ui-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonStore(path);
            var host = new AppHost(store, store.Load());
            var vm = new VocabularyViewModel(host);
            vm.Refresh();

            var before = host.Data.Words.Count;
            vm.NewPt = "borboleta";
            vm.NewEn = "butterfly";
            vm.AddCommand.Execute(null);

            Assert.Equal(before + 1, host.Data.Words.Count);
            var added = host.Data.Words.Single(w => w.En == "butterfly");
            Assert.True(added.IsUser);
            Assert.Contains(host.Data.Cards, c => c.WordId == added.Id);

            // A validação protege contra campos vazios.
            vm.NewPt = "";
            vm.NewEn = "";
            vm.AddCommand.Execute(null);
            Assert.True(vm.AddError);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void Chat_CreateAndDeleteSession_KeepsGreeting()
    {
        var path = Path.Combine(Path.GetTempPath(), $"englishstudy-ui-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonStore(path);
            var host = new AppHost(store, store.Load());
            var vm = new ChatViewModel(host);
            vm.Refresh();

            Assert.Single(vm.Sessions);
            Assert.NotEmpty(vm.Messages);
            Assert.StartsWith("Hi!", vm.Messages[0].Text);

            vm.NewChatCommand.Execute(null);
            Assert.Equal(2, vm.Sessions.Count);

            vm.DeleteChatCommand.Execute(null);
            Assert.Single(vm.Sessions);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void Settings_Save_WritesProfileAndSettings()
    {
        var path = Path.Combine(Path.GetTempPath(), $"englishstudy-ui-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonStore(path);
            var host = new AppHost(store, store.Load());
            var vm = new SettingsViewModel(host);
            vm.Refresh();

            vm.Name = "Pedro";
            vm.NewCardsText = "5";
            vm.SaveCommand.Execute(null);

            Assert.Equal("Pedro", host.Data.Profile.Name);
            Assert.Equal(5, host.Data.Settings.NewCardsPerDay);

            // Limite fora da faixa é ajustado.
            vm.NewCardsText = "500";
            vm.SaveCommand.Execute(null);
            Assert.Equal(100, host.Data.Settings.NewCardsPerDay);

            // Reset zera o progresso mas mantém o nome e as palavras.
            host.Data.Profile.Xp = 500;
            vm.ResetCommand.Execute(null); // pede confirmação
            vm.ResetCommand.Execute(null); // confirma
            Assert.Equal(0, host.Data.Profile.Xp);
            Assert.Equal("Pedro", host.Data.Profile.Name);
            Assert.Equal(host.Data.Words.Count, host.Data.Cards.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
