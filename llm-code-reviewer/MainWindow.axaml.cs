using Avalonia.Controls;
using Avalonia.Interactivity;
using System;             

using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;

using System.Threading.Tasks;
using Avalonia.Threading;


namespace LLMCodeReviewer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private Button _saveButton;
        public LLM llm;
        
        private Border? _typingIndicator;
        private bool _isTyping = false;
        
        public MainWindow()
        {
            InitializeComponent();
            llm = new LLM("gpt-5");
            _vm = MainViewModel.Load();
            DataContext = _vm;
        }
        
        private async void OnInputKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox input && !string.IsNullOrWhiteSpace(input.Text))
            {
                var bubble = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#2A2B2E")),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(10),
                    Margin = new Thickness(100, 4, 0, 4),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Child = new TextBlock
                    {
                        Text = input.Text,
                        FontSize = 16,
                        Foreground = new SolidColorBrush(Color.Parse("#A9B7C6")),
                        TextWrapping = TextWrapping.Wrap,
                    }
                };

                MessagesPanel.Children.Add(bubble);
                ShowTypingIndicator();
                input.Clear();
                
                var response = await llm.AskAsync("Whats the weather rn in berlin?");
                HideTypingIndicator();
                __AddMessage(response, fromUser: false);
            }
        }
        private void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            _vm.Save();
        }
        
        private void OnRestoreClick(object? sender, RoutedEventArgs e)
        {
            _vm.Restore();
        }


        protected override void OnClosed(EventArgs e)
        {
            _vm.Save();
            base.OnClosed(e);
        }
        
        
        private void __AddMessage(string text, bool fromUser)
        {
            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.Parse(fromUser ? "#2E3B4E" : "#3C3F41")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10),
                Margin = fromUser ? new Thickness(80, 4, 0, 4) : new Thickness(0, 4, 80, 4),
                HorizontalAlignment = fromUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            MessagesPanel.Children.Add(bubble);

            Dispatcher.UIThread.Post(() => bubble.BringIntoView(), DispatcherPriority.Background);
        }
        
        private async void ShowTypingIndicator()
        {
            if (_isTyping) return;
            _isTyping = true;

            var label = new TextBlock
            {
                Text = "🤖 ИИ печатает",
                Foreground = new SolidColorBrush(Color.Parse("#8F9BA8")),
                FontStyle = FontStyle.Italic
            };

            _typingIndicator = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#3C3F41")),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 4, 80, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = label
            };
            MessagesPanel.Children.Add(_typingIndicator);

            _ = Task.Run(async () =>
            {
                int i = 0;
                while (_isTyping)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                        label.Text = "Waiting for response" + new string('.', (i % 3) + 1));
                    i++;
                    await Task.Delay(400);
                }
            });
        }
        
        private void HideTypingIndicator()
        {
            if (!_isTyping) return;
            _isTyping = false;

            if (_typingIndicator != null)
            {
                MessagesPanel.Children.Remove(_typingIndicator);
                _typingIndicator = null;
            }
        }
        
    }
}