using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace LLMCodeReviewer
{
    public partial class AIbot : Window
    {
        private LLM _llm;
        private Border? _typingIndicator;
        private bool _isTyping = false;
        private Border _replyBubble;
        private TextBox _replyText;
        public AIbot()
        {
            InitializeComponent();
            _llm = new LLM("gpt-5");
        }
        
        private async void OnSendPromptClick(object? sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(InputBox.Text))
                return;

            string messageText = InputBox.Text;

            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#2A2B2E")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10),
                Margin = new Thickness(100, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Right,
                Child = new TextBlock
                {
                    Text = messageText,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                }
            };
            
            
            
            MessagesPanel.Children.Add(bubble);
            ShowTypingIndicator();
            InputBox.Clear();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (MessagesPanel.Parent is ScrollViewer scroll)
                {
                    scroll.ScrollToEnd();
                }
                InputBox.Focus();
            }, DispatcherPriority.Render);

            var response = "";
            
            switch (_llm.ErrorCode)
            {
                case LlmInitError.NoApiKey:
                    response = "Error - Please set your OpenAI key first. Note: export API key in env.";
                    break;
                case LlmInitError.NoConnection:
                    response = "Error - OpenAI API is unreachable.";
                    break;
                case LlmInitError.NetworkError:
                    response = "Error - Network problem. Check your connection.";
                    break;
                default:
                    response = await _llm.AskAsync(messageText);
                    break;
            }
            
            HideTypingIndicator();
            
            var replyBubble = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#3C3F41")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 4, 80, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBox
                {
                    Text = response,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Color.Parse("#A9B7C6")),
                    Background = new SolidColorBrush(Color.Parse("#3C3F41")),
                    BorderThickness = new Thickness(0),
                    IsReadOnly = true,
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    IsTabStop = false,
                    Cursor = new Cursor(StandardCursorType.Ibeam) 
                }
            };

            MessagesPanel.Children.Add(replyBubble);
            
            Dispatcher.UIThread.Post(() =>
            {
                if (MessagesPanel.Parent is ScrollViewer scroll)
                {
                    scroll.ScrollToEnd();
                }
            }, DispatcherPriority.Background);
        }
        
        private async void ShowTypingIndicator()
        {
            if (_isTyping) return;
            _isTyping = true;

            var label = new TextBlock
            {
                Text = "Waiting for response",
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
                        label.Text = "Waiting for AI response" + new string('.', (i % 3) + 1));
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