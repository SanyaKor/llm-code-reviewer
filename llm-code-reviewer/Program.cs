using Avalonia;
using System;
using DotNetEnv;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;


namespace LLMCodeReviewer
{

    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            LoadEnvFromSolutionRoot();
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        private static void LoadEnvFromSolutionRoot()
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, ".env")))
                dir = Directory.GetParent(dir)?.FullName;

            if (dir != null)
                Env.Load(Path.Combine(dir, ".env"));
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();



    }
    
}