using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LLMCodeReviewer
{
    public partial class RenameWindow : Window
    {
        public string NewName { get; private set; } = string.Empty;
        public RenameWindow(string currentName)
        {
            InitializeComponent();
            InputBox.Text = currentName;
            InputBox.CaretIndex = currentName.Length;
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void OnOkClick(object? sender, RoutedEventArgs e)
        {
            var text = InputBox.Text?.Trim();
            if (!string.IsNullOrEmpty(text))
                NewName = text;
            Close(NewName);
        }
    }
}