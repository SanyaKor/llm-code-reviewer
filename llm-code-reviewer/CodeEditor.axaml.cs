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
        private List<Script> _initialScripts;

        
        private RegistryOptions _registry;
        private TextMate.Installation _tm;
        private string? _currentScope;
        
        private Script _currScript = new();
        private bool _contentChanged;
        
        private const double _minFont = 10.0;
        private const double _maxFont = 28.0;
        private const double _stepFont = 1.0;
        
        private AIbot? _botWindow;
        
        public CodeEditor()
        {
            InitializeComponent();
            LoadEditorConfig();
            LoadScriptsFromDisk();

            _initialScripts = _scripts.Select(s => new Script(s)).ToList();
            
            
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
        

        private string AnalyzeChanges_1()
        {
            string prompt = """
                            Analyze the following diffs from source files.

                            Each diff block follows this structure:

                            ──────────────────────────────
                            updated: <filename>
                            - {line number}: <old line>
                            + {line number}: <new line>
                            ──────────────────────────────
                            deleted: <filename>
                            - <old line>
                            ──────────────────────────────
                            added: <filename>
                            + <new line>
                            ──────────────────────────────
                            renamed: <old_filename> -> <new_filename>
                            ──────────────────────────────

                            Rules:
                            1. “updated” — file existed before and was modified.
                            2. “deleted” — file was removed completely.
                            3. “added” — new file was added.
                            4. Lines starting with “-” show removed or replaced code (old version).
                            5. Lines starting with “+” show newly added or changed code (new version).
                            6. Line numbers indicate original or new line positions in the file.

                            Your task:
                            - For each file, describe what changed and why.
                            - Identify specific logic or syntax modifications.
                            - Summarize the intent of the edit (bug fix, refactor, feature, etc.).
                            - Keep your explanation structured per file.
                            """;
            
            foreach (var oldScript in _initialScripts)
            {
                var updated = _scripts.FirstOrDefault(s => s.Id == oldScript.Id);
                if (updated is null)
                {
                    prompt += $"deleted: {oldScript.Title}\n";
                    string[] oldLines = oldScript.Content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    
                    foreach (var s in oldLines)
                        if (!string.IsNullOrWhiteSpace(s))
                            prompt += $"- {s}\n";

                    continue;
                }

                if (!string.Equals(oldScript.Content, updated.Content, StringComparison.Ordinal))
                {
                    if (!string.Equals(oldScript.Title, updated.Title, StringComparison.Ordinal))
                        prompt += $"renamed: {oldScript.Title} -> {updated.Title}\n";

                    prompt += $"updated: {updated.Title}\n";
                    
                    string[] oldLines = oldScript.Content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    string[] newLines = updated.Content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

                    int max = Math.Max(oldLines.Length, newLines.Length);

                    for (int i = 0; i < max; i++)
                    {
                        string? oldLine = i < oldLines.Length ? oldLines[i] : null;
                        string? newLine = i < newLines.Length ? newLines[i] : null;

                        if (!string.Equals(oldLine, newLine, StringComparison.Ordinal))
                        {
                            if (!string.IsNullOrWhiteSpace(oldLine))
                                prompt += $"- {i}:{oldLine}\n";
                            if (!string.IsNullOrWhiteSpace(newLine))
                                prompt += $"+ {i}:{newLine}\n";
                        }
                    }
                }
            }
            
            foreach (var updated in _scripts)
            {
                if (_initialScripts.All(s => s.Id != updated.Id))
                {
                    prompt += $"added: {updated.Title}\n";
                    string[] updatedLines = updated.Content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

                    foreach (var s in updatedLines)
                        if (!string.IsNullOrWhiteSpace(s))
                            prompt += $"+ {s}\n";

                }
            }
            
            return prompt;
        }
        
        
        private string AnalyzeChanges_2()
        {
            string prompt = """
                            Analyze the following diffs from files.

                            Each diff follows this structure:

                            ──────────────────────────────
                            updated: <filename>
                            - old_script:
                            <old content>
                            + new_script:
                            <new content>
                            ──────────────────────────────
                            deleted: <filename>
                            - old_script:
                            <old content>
                            ──────────────────────────────
                            added: <filename>
                            + new_script:
                            <new content>
                            ──────────────────────────────
                            renamed: <old_filename> -> <new_filename>
                            ──────────────────────────────

                            Rules:
                            1. “updated” means the file existed before and was modified.
                            2. “deleted” means the file was removed.
                            3. “added” means a new file was added.
                            4. Lines starting with “-” indicate removed or old code.
                            5. Lines starting with “+” indicate new or added code.

                            Your task:
                            - For each file section, describe what changed and why.
                            - Summarize the purpose or intention behind modifications.
                            - Identify potential issues, mistakes, or improvements.
                            - Keep your response structured by file name.;
                            """;
            
            foreach (var oldScript in _initialScripts)
            {
                var updated = _scripts.FirstOrDefault(s => s.Id == oldScript.Id);
                if (updated is null)
                {
                    prompt += $"deleted: {oldScript.Title}\n";
                    prompt += $"- old_script:\n{oldScript.Content}\n";
                    continue;
                }

                if (!string.Equals(oldScript.Content, updated.Content, StringComparison.Ordinal))
                {
                    if (!string.Equals(oldScript.Title, updated.Title, StringComparison.Ordinal))
                        prompt += $"renamed: {oldScript.Title} -> {updated.Title}\n";

                    prompt += $"updated: {updated.Title}\n";
                    prompt += $"- old_script:\n{oldScript.Content}\n";
                    prompt += $"+ new_script:\n{updated.Content}\n";
                }
            }
            
            foreach (var updated in _scripts)
            {
                if (_initialScripts.All(s => s.Id != updated.Id))
                {
                    prompt += $"added: {updated.Title}\n";
                    prompt += $"+ new_script:\n{updated.Content}\n";
                }
            }
            
            return prompt;
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

        private async void OnBotCLick(object? sender, RoutedEventArgs e)
        {
            SaveFile();
            string prompt = AnalyzeChanges_2();
            
            
            if (_botWindow is { IsVisible: true })
            {
                _botWindow.Activate();
                return;
            }

            _botWindow = new AIbot(prompt)
            {
                DataContext = this,
                Topmost = false,
                ShowInTaskbar = true
            };
            _botWindow.Closed += (_, _) => _botWindow = null;

            _botWindow.Show();
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
            var s = Math.Clamp(size, _minFont, _maxFont);
            Editor.FontSize = s;
            Editor.TextArea.TextView.Redraw();
        }
        private void ZoomIn()  => ApplyFontSize(Editor.FontSize + _stepFont);
        private void ZoomOut() => ApplyFontSize(Editor.FontSize - _stepFont);
        private void ZoomReset()=> ApplyFontSize(14.0); 
        
    }

}