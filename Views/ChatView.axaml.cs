using Avalonia.Controls;
using Avalonia.Threading;
using EnglishStudy.ViewModels;

namespace EnglishStudy.Views;

public partial class ChatView : UserControl
{
    private ChatViewModel? _vm;

    public ChatView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Hook();
    }

    private void Hook()
    {
        if (_vm is not null)
            _vm.MessagesChanged -= OnMessagesChanged;

        _vm = DataContext as ChatViewModel;
        if (_vm is not null)
            _vm.MessagesChanged += OnMessagesChanged;
    }

    private void OnMessagesChanged() =>
        Dispatcher.UIThread.Post(() => MessagesScroll.ScrollToEnd(), DispatcherPriority.Background);
}
