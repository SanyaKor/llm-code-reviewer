using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LLMCodeReviewer;

public partial class PromptConfig : Window
{
    private List<Prompt>  _prompts;
    private readonly Action _onChanged;  
    public PromptConfig(List<Prompt>  prompts, Action onChanged)
    {
        _prompts = prompts;
        _onChanged = onChanged;
        InitializeComponent();
        LoadPromptsFromFile();
    }
    
    private void LoadPromptsFromFile()
    {
        List<string> _promptItems = _prompts.Select(p => p.Title).ToList();
        PromptList.ItemsSource = _promptItems;
        if (_prompts.Count > 0)
        {
            PromptList.SelectedIndex = 0;
            PromptText.Text = _prompts[0].Content;
        }
        else
        {
            PromptList.SelectedIndex = -1;
            PromptText.Text = "No prompts found";
        }
    }
    
    private void OnPromptSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (PromptList.SelectedItem is string title)
        {
            var p = _prompts.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.Ordinal));
            PromptText.Text = p?.Content ?? "No content.";
        }
        else
        {
            PromptText.Text = "Seleсt from the list";
        }
    }
    
    private void OnDeletePromptClick(object? sender, RoutedEventArgs e)
    {   
        if (PromptList.SelectedItem is not string title)
            return;

        var p = _prompts.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.Ordinal));
        if (p is null)
            return;

        var removeIndex = PromptList.SelectedIndex;
        _prompts.Remove(p);

        PromptStorage.SavePrompts(_prompts);

        RebindList();

        if (_prompts.Count == 0)
        {
            PromptList.SelectedIndex = -1;
            PromptText.Text = "No prompts.";
            return;
        }

        var newIndex = Math.Clamp(removeIndex, 0, _prompts.Count - 1);
        PromptList.SelectedIndex = newIndex;

        if (PromptList.SelectedItem is Prompt next)
            PromptText.Text = next.Content;
        else
            PromptText.Text = "Select a prompt from the list";
        
        _onChanged.Invoke();
    }
    
    private void RebindList()
    {
        PromptList.ItemsSource = null;
        PromptList.Items.Clear();
        LoadPromptsFromFile();
    }
    
    
    
    
    
    
    
}