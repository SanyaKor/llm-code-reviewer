using Avalonia;
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;


namespace AvaloniaApplication1
{

    class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();



    }
    
}