// App.axaml.cs
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BoilingPot.ViewModels; // Основные VM
using BoilingPot.ViewModels.Components; // VM компонентов (Pot)
using BoilingPot.ViewModels.SettingsViewModels; // VM настроек
using BoilingPot.Services;  // Сервисы
using BoilingPot.Views;     // Основные View
using BoilingPot.Views.SettingsViews; // View настроек
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using MainWindowViewModel = BoilingPot.ViewModels.MainWindowViewModel;

namespace BoilingPot
{
    public partial class App : Application
    {
        public IHost? AppHost { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // --- Сервисы ---
                    services.AddSingleton<IPluginLoaderService, PluginLoaderService>();

                    // --- ViewModel Компонентов ---
                    // Регистрируем КОНКРЕТНЫЕ реализации PotViewModel как Transient,
                    // но основной PotViewModelInstance в ModelSettingsViewModel будет один.
                    // Это нужно, чтобы DI мог их создать при необходимости.
                    services.AddTransient<MainPotViewModel>();
                    services.AddTransient<AltPotViewModel>();
                    services.AddTransient<PotViewModelBase>();
                    // Регистрируем и интерфейс, чтобы можно было запросить любую реализацию по интерфейсу,
                    // но это не будет использоваться напрямую в нашем случае.
                    // services.AddTransient<IPotViewModel, MainPotViewModel>(); // Пример, если нужно

                    // Добавить регистрации для StoveViewModel, BubbleViewModel...

                    // --- ViewModel Секций Настроек ---
                    services.AddTransient<GeneralSettingsViewModel>();
                    services.AddTransient<ThemeSettingsViewModel>();
                    // ModelSettingsViewModel теперь имеет зависимости! DI их внедрит.
                    services.AddTransient<ModelSettingsViewModel>(); // <-- Регистрируем его

                    // --- Основные ViewModel ---
                    services.AddSingleton<SettingsViewModel>(); // ViewModel окна/панели настроек
                    services.AddTransient<CommonViewModel>();  // ViewModel для основного вида симуляции
                    services.AddTransient<MolecularViewModel>(); // ViewModel для молекулярного вида
                    services.AddTransient<HomeViewModel>();    // ViewModel для стартового экрана
                    services.AddSingleton<MainWindowViewModel>(); // Главный VM

                    // --- Окна ---
                    services.AddTransient<MainWindow>();
                })
                .Build();

            var serviceProvider = AppHost.Services;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
                var mainViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();
                mainWindow.DataContext = mainViewModel; // Устанавливаем DataContext окна
                desktop.MainWindow = mainWindow;
                System.Diagnostics.Debug.WriteLine(">>> DI Host и MainWindow настроены!");
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}