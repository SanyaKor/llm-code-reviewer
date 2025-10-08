using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.Json;
using System;



namespace LLMCodeReviewer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _defaultText = "sadasdadassdfada";
        private readonly string _initialText = "Текст по умолчанию";
        public string DefaultText
        {
            get => _defaultText;
            set
            {
                if (_defaultText != value)
                {
                    _defaultText = value;
                    OnPropertyChanged();
                }
            }
        }

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AvaloniaApp",
            "settings.json");

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(FilePath, json);
        }

        public void Restore()
        {
            DefaultText = _initialText;
        }

        public static MainViewModel Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<MainViewModel>(json) ?? new MainViewModel();
                }
            }
            catch
            {
                
            }

            return new MainViewModel(); 
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}