using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LLMCodeReviewer
{
    public static class PromptStorage
    {
        private static readonly string FilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LLMCodeReviewer",
                "prompts.json");

        public static List<Prompt> LoadPrompts()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    var defaultPrompts = new List<Prompt>
                    {
                        new() { Title = "Prompt 1", Content = "Explain this code change" },
                        new() { Title = "Prompt 2", Content = "Summarize the differences" },
                        new() { Title = "Prompt 3", Content = "Suggest improvements" },
                        new() { Title = "Prompt 4", Content = "Suggest improvements" },
                        new() { Title = "Prompt 5", Content = "Suggest improvements" },
                    };
                    SavePrompts(defaultPrompts);
                    return defaultPrompts;
                }

                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<Prompt>>(json) ?? new List<Prompt>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LoadPrompts: {ex.Message}");
                return new List<Prompt>();
            }
        }

        public static void SavePrompts(List<Prompt> prompts)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir!);

                var json = JsonSerializer.Serialize(prompts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to save prompts: {ex.Message}");
            }
        }
        public static void RemovePrompt(string title)
        {
            var prompts = LoadPrompts();
            prompts.RemoveAll(p => p.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            SavePrompts(prompts);
        }
    }
    public class Prompt
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
    }
}