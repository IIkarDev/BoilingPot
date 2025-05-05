using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BoilingPot.ViewModels;         
using BoilingPot.Views;              
using System;
using System.Diagnostics;
using Splat;

namespace BoilingPot // Используйте ваше пространство имен
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = Locator.Current.GetService<MainWindow>();
                var mainViewModel = Locator.Current.GetService<MainViewModel>();

                if (mainWindow == null)
                {
                    var errorMsg = "!!! CRITICAL ERROR: MainWindow could not be resolved from Locator.Current!";
                    Debug.WriteLine(errorMsg);

                    throw new InvalidOperationException(errorMsg);
                }

                if (mainViewModel == null)
                {
                    var errorMsg = "!!! CRITICAL ERROR: MainViewModel could not be resolved from Locator.Current!";
                    Debug.WriteLine(errorMsg);
                    throw new InvalidOperationException(errorMsg);
                }

                mainWindow.DataContext = mainViewModel;
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
            Debug.WriteLine("[App.OnFrameworkInitializationCompleted] Completed successfully.");
        }
    }
}