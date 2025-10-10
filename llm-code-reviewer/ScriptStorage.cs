using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LLMCodeReviewer
{
    public static class ScriptStorage
    {
        private static readonly string FilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LLMCodeReviewer",
                "scripts.json");

        public static List<Script> LoadScripts()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    var defaultScripts = new List<Script>
                    {
                        new() { Title = "Default Csharp Script.cs", 
                            Content = """
                                      using System;
                                      using System.Linq;

                                      namespace Demo
                                      {
                                          public static class Program
                                          {
                                              public static void Main(string[] args)
                                              {
                                                  Console.WriteLine("Hello TextMate!");
                                                  var xs = Enumerable.Range(1, 5).ToArray();
                                                  Console.WriteLine($"Sum = {xs.Sum()}");
                                              }
                                          }
                                      }
                                      """ },
                        new() { Title = "Default Python Script.py", 
                            Content = """"
                                      class Greeter:
                                          def __init__(self, name):
                                              self.name = name
                                  
                                          def greet(self):
                                              print(f"Hello, {self.name}!")
                                  
                                      if __name__ == "__main__":
                                          g = Greeter("World")
                                          g.greet()
                                  
                                      """"
                            },
                        new() { Title = "Default Html Script.html", 
                            Content = """
                                          <!DOCTYPE html>
                                          <html lang="en">
                                          <head>
                                              <meta charset="UTF-8">
                                              <meta name="viewport" content="width=device-width, initial-scale=1.0">
                                              <title>Hello HTML</title>
                                              <style>
                                                  body {
                                                      background-color: #1e1e1e;
                                                      color: #e8e8e8;
                                                      font-family: Arial, sans-serif;
                                                      display: flex;
                                                      justify-content: center;
                                                      align-items: center;
                                                      height: 100vh;
                                                  }
                                                  h1 { color: #409EFF; }
                                              </style>
                                          </head>
                                          <body>
                                              <h1>Hello, World!</h1>
                                          </body>
                                          </html>
                                          """
                        },
                        new()
                        {
                            Title = "Default Xml script.xml",
                            Content = """
                                      <?xml version="1.0" encoding="UTF-8"?>
                                      <configuration>
                                          <appSettings>
                                              <add key="Theme" value="Dark"/>
                                              <add key="Language" value="en-US"/>
                                          </appSettings>
                                          <user>
                                              <name>John Doe</name>
                                              <role>Developer</role>
                                          </user>
                                      </configuration>
                                      """
                        },
                        new()
                        {
                            Title = "Default Json script.json",
                            Content = """
                                      {
                                          "name": "John Doe",
                                          "age": 30,
                                          "isDeveloper": true,
                                          "skills": ["C#", "Python", "HTML", "CSS"],
                                          "preferences": {
                                              "theme": "dark",
                                              "fontSize": 14
                                          }
                                      }
                                      """
                        }
                    };
                    SaveScripts(defaultScripts);
                    return defaultScripts;
                }

                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<Script>>(json) ?? new List<Script>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LoadScripts: {ex.Message}");
                return new List<Script>();
            }
        }

        public static void SaveScripts(List<Script> scripts)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir!);

                var json = JsonSerializer.Serialize(scripts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to save scripts: {ex.Message}");
            }
        }
        public static void RemoveScript(string title)
        {
            var scripts = LoadScripts();
            scripts.RemoveAll(p => p.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            SaveScripts(scripts);
        }
    }
    public class Script
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
    }
}