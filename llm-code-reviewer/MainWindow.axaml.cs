using Avalonia.Controls;
using Avalonia.Interactivity;
using System;             

using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;


namespace AvaloniaApplication1
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private Button _saveButton;
        public MainWindow()
        {
            InitializeComponent();
            _vm = MainViewModel.Load();
            DataContext = _vm;
        }
        
        private void OnInputKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox input && !string.IsNullOrWhiteSpace(input.Text))
            {
                // Создаём сообщение (Border + TextBlock)
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

                input.Clear();
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
    }
}