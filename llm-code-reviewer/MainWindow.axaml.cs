using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Button _saveButton;
        private LLM _llm;
        
        private TextBox _inputBox;
        
        private Border? _typingIndicator;
        private int _lastValidIndex = 0;

        private bool _isTyping = false;
        
        private List<Prompt> _prompts = new();
        private List<string> _comboItems = new();
        
        public MainWindow()
        {
            InitializeComponent();
            LoadPromptsFromFile();
            _llm = new LLM("gpt-5");
            _inputBox = InputBox;
            
        }
        
        
        private void LoadPromptsFromFile()
        {
            _prompts = PromptStorage.LoadPrompts();

            _comboItems = _prompts.Select(p => p.Title).ToList();
            _comboItems.Add("Edit config...");

            PromptList.ItemsSource = _comboItems;
            PromptList.SelectedIndex = 0;

            PromptList.ContainerPrepared += (_, args) =>
            {
                if (args.Container.DataContext?.ToString() == "Edit config...")
                    args.Container.Classes.Add("edit-item");
            };
        }

        private async void OnSendPromptClick(object? sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(_inputBox.Text))
                return;

            string messageText = _inputBox.Text;

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
                    Foreground = new SolidColorBrush(Color.Parse("#A9B7C6")),
                    TextWrapping = TextWrapping.Wrap,
                }
            };
            
            MessagesPanel.Children.Add(bubble);
            ShowTypingIndicator();
            _inputBox.Clear();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (MessagesPanel.Parent is ScrollViewer scroll)
                {
                    scroll.ScrollToEnd();
                }
                _inputBox.Focus();
            }, DispatcherPriority.Render);
            
            var response = await _llm.AskAsync(messageText);
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
                    Background = Brushes.Transparent,
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
        
        private void OnDropDownSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            
            if (sender is not ComboBox cb) return;
            
            if (cb.SelectedItem is not string value) return;

            if (value == "Edit config..." || value == "Edit configs…")
            {
                cb.SelectedIndex = 0;          
                OpenConfigsEditor();
            }
        }

        private void OpenConfigsEditor()
        {
            var window = new PromptConfig(_prompts, onChanged: () =>
            {
                PromptStorage.SavePrompts(_prompts);
                RefreshComboFromPrompts();
            });
            window.Show();
        }
        
        private void RefreshComboFromPrompts()
        {
            var titles = _prompts.Select(p => p.Title).ToList();
            titles.Add("Edit config...");

            PromptList.ItemsSource = null;
            PromptList.Items.Clear();
            PromptList.ItemsSource = titles;

            PromptList.SelectedIndex = titles.Count > 1 ? 0 : -1;

            PromptList.ContainerPrepared += (_, a) =>
            {
                if (a.Container.DataContext?.ToString() == "Edit config...")
                    a.Container.Classes.Add("edit-item");
            };
        }

        
        

    }
}