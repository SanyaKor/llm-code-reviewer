using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit.TextMate;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Grammars;

namespace LLMCodeReviewer
{
    public partial class CodeEditor : Window
    {
        private bool _wrap;

        private TextMate.Installation? _textMate;
        private List<Script> _scripts = new();
        
        private RegistryOptions _registry;
        private TextMate.Installation _tm;
        private string? _currentScope;
        
        private Script _currScript = new();
        private bool _contentChanged;
        
        private const double MinFont = 10.0;
        private const double MaxFont = 28.0;
        private const double StepFont = 1.0;
        
        public CodeEditor()
        {
            InitializeComponent();
            LoadEditorConfig();
            LoadScriptsFromDisk();

            Editor.TextChanged += (_, __) => _contentChanged = true;
            this.KeyDown += (_, e) =>
            {
                if (!e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
                    return;

                switch (e.Key)
                {
                    case Avalonia.Input.Key.OemPlus:
                    case Avalonia.Input.Key.Add:
                        ZoomIn();  e.Handled = true; break;

                    case Avalonia.Input.Key.OemMinus:
                    case Avalonia.Input.Key.Subtract:
                        ZoomOut(); e.Handled = true; break;

                    case Avalonia.Input.Key.D0:
                    case Avalonia.Input.Key.NumPad0:
                        ZoomReset(); e.Handled = true; break;
                }
            };
            this.Closed += OnWindowClosed;
        }
        
        private void LoadEditorConfig()
        {
            var editor = this.FindControl<TextEditor>("Editor");
            _registry = new RegistryOptions(ThemeName.DarkPlus);
            _tm = editor.InstallTextMate(_registry);
            _tm.SetGrammar(_registry.GetScopeByLanguageId(_registry.GetLanguageByExtension(".cs").Id));
        }

        private void LoadScriptsFromDisk()
        {
            _scripts = ScriptStorage.LoadScripts();

            var comboItems = _scripts.Select(p => p.Title).ToList();

            FilesList.ItemsSource = comboItems;
            FilesList.SelectedIndex = 0;

            if (_scripts.Count > 0)
            {
                FilesList.SelectedIndex = 0;
                Editor.Text = _scripts[0].Content;
            }
            else
            {
                FilesList.SelectedIndex = -1;
                Editor.Text = "No files found";
            }
        }

        private void SaveFile()
        {
            if (_currScript is null) return;
            
            if (_contentChanged)
                _currScript.Content = Editor.Text ?? string.Empty;
            
            _contentChanged = false;
        }
        private void OnFileSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (_currScript is not null)
                SaveFile();
            
            if (FilesList.SelectedItem is string title)
            {
                var s = _scripts.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.Ordinal));
                _currScript = s;
                ApplySyntaxByExtension(title);
                Editor.Text = s?.Content ?? "";
                Editor.TextArea.TextView.Redraw();
            }
        }

        private void OnDeleteFileClick(object? sender, RoutedEventArgs e)
        {
            if (FilesList.SelectedItem is not string title)
                return;

            var p = _scripts.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.Ordinal));
            if (p is null)
                return;
            
            var removeIndex = FilesList.SelectedIndex;
            _scripts.Remove(p);

            ScriptStorage.SaveScripts(_scripts);

            RebindList();
            
            if (_scripts.Count == 0)
            {
                FilesList.SelectedIndex = -1;
                Editor.Text = "No scripts found.";
                return;
            }

            var newIndex = removeIndex - 1;
            if (newIndex < 0) newIndex = 0;

            FilesList.SelectedIndex = newIndex;

            if (FilesList.SelectedItem is string nextTitle)
            {
                var nextScript = _scripts.FirstOrDefault(x => x.Title == nextTitle);
                if (nextScript is not null)
                {
                    Editor.Text = nextScript.Content;
                    ApplySyntaxByExtension(nextScript.Title);
                }
                else
                {
                    Editor.Text = "Select a file from the list";
                }
            }
            else
            {
                Editor.Text = "Select a file from the list";
            }
        }


        private void RebindList()
        {
            FilesList.ItemsSource = null;
            FilesList.Items.Clear();
            LoadScriptsFromDisk();
        }

      

        private async void OnAddFileClick(object? sender, RoutedEventArgs e)
        {
            string newTitle = GenerateUniqueTitle();
            var dialog = new RenameWindow(newTitle)
            {
                Title = "New File"
            };

            var result = await dialog.ShowDialog<string?>(this);

            if (string.IsNullOrWhiteSpace(result))
                return;

            var title = result.Trim();

            var p = new Script { Title = title, Content = string.Empty };
            _scripts.Add(p);
            ScriptStorage.SaveScripts(_scripts);
            
            var titles = _scripts.Select(x => x.Title).ToList();
            
            FilesList.ItemsSource = null;
            FilesList.Items.Clear();
            FilesList.ItemsSource = titles;

            FilesList.SelectedItem = title;

            Editor.Text = p.Content;
        }

        private void ApplySyntaxByExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant() ?? ".txt";
            SetGrammarByExtension(ext);
        }

        private void SetGrammarByExtension(string ext)
        {
            var resolvedExt = ext switch
            {
                ".cs" => ".cs",
                ".py" => ".py",
                ".json" => ".json",
                ".xml" => ".xml",
                ".md" => ".md",
                ".html" => ".html",
                ".css" => ".css",
                ".js" => ".js",
                _ => ".txt"
            };
            string scope;

            var lang = _registry.GetLanguageByExtension(resolvedExt);
            if (resolvedExt == ".txt")
            {
                scope = _registry.GetScopeByLanguageId("ignore");
            }
            else
                scope = _registry.GetScopeByLanguageId(lang.Id);

            _tm.SetGrammar(scope);

            if (!string.Equals(_currentScope, scope, StringComparison.Ordinal))
            {
                _tm.SetGrammar(scope);
                _currentScope = scope;
            }
        }

        private static string InjectFileName(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            string name = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;

            name = Regex.Replace(name, @"[^a-zA-Z0-9 _]", "_");

            name = Regex.Replace(name, @"\s{2,}", " ");
            name = Regex.Replace(name, "_{2,}", "_");
            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name))
                name = "New script";

            if (string.IsNullOrWhiteSpace(ext))
                ext = ".txt";

            return name + ext;
        }

        private string GenerateUniqueTitle(string baseTitle = "New script")
        {
            string baseSanitized = InjectFileName(baseTitle);
            string ext = Path.GetExtension(baseSanitized);
            string name = Path.GetFileNameWithoutExtension(baseSanitized);

            var sameExtTitles = new HashSet<string>(
                _scripts
                    .Select(p => InjectFileName(p.Title))
                    .Where(t => string.Equals(Path.GetExtension(t), ext, StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase
            );

            string candidate = $"{name}{ext}";
            int i = 1;
            while (sameExtTitles.Contains(candidate))
            {
                candidate = $"{name} {i}{ext}";
                i++;
            }

            return candidate;
        }

       
        private void OnWindowClosed(object? sender, EventArgs e)
        {
            if (FilesList.SelectedItem is not string currentTitle)
                return;

            var current = _scripts.FirstOrDefault(x =>
                string.Equals(x.Title, currentTitle, StringComparison.Ordinal));

            if (current is null)
                return;

            current.Content = Editor.Text ?? string.Empty;

            ScriptStorage.SaveScripts(_scripts);

        }

        private void OnBotCLick(object? sender, RoutedEventArgs e)
        {
            var wnd = new AIbot();
            wnd.Show(this);
        }

        private void OnExitClick(object? sender, RoutedEventArgs e) => Close();

        private void OnToggleWrapClick(object? sender, RoutedEventArgs e)
        {
            _wrap = !_wrap;
            Editor.WordWrap = _wrap;
        }
        
        private async void OnRenameClick(object? sender, RoutedEventArgs e)
        {
            if (FilesList.SelectedItem is not string title)
                return;

            var dialog = new RenameWindow(title);
            var result = await dialog.ShowDialog<string?>(this);

            if (string.IsNullOrWhiteSpace(result))
                return;

            var item = _scripts.FirstOrDefault(x => x.Title == title);
            if (item is null)
                return;

            item.Title = result;
            FilesList.ItemsSource = null;
            FilesList.ItemsSource = _scripts.Select(s => s.Title).ToList();
            FilesList.SelectedItem = result;
        }
        
        private void ApplyFontSize(double size)
        {
            var s = Math.Clamp(size, MinFont, MaxFont);
            Editor.FontSize = s;
            Editor.TextArea.TextView.Redraw();
        }

        private void ZoomIn()  => ApplyFontSize(Editor.FontSize + StepFont);
        private void ZoomOut() => ApplyFontSize(Editor.FontSize - StepFont);
        private void ZoomReset()=> ApplyFontSize(14.0); 
        
    }

}