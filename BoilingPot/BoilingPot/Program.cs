using Avalonia;
using Avalonia.ReactiveUI; 
using System;
using System.Diagnostics;
using BoilingPot.Services;
using BoilingPot.ViewModels;
using BoilingPot.ViewModels.Components;
using BoilingPot.ViewModels.SettingsViewModels;
using BoilingPot.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace BoilingPot;
internal class Program
{
private static void ConfigureDependencies(IServiceCollection services)
{
    Debug.WriteLine("[Program.ConfigureDependencies] Registering application services.");
    // Register ViewModels
    services.AddSingleton<IThemeLoaderService, ThemeLoaderService>();
    
    // --- Регистрация ViewModel Компонентов ---
    // Debug.WriteLine("[App] ConfigureDependencies: Регистрация PotViewModelBase, MainPotViewModel, AltPotViewModel.");
    services.AddSingleton<PotViewModelBase>(); // MainPotViewModel - реализация по умолчанию
// Если хочешь регистрировать по интерфейсу IPotViewModel как синглтон
    services.AddSingleton<IPotViewModel>(sp => sp.GetRequiredService<PotViewModelBase>()); 
    services.AddTransient<StoveViewModelBase>();

    
    // // ViewModel секций настроек (Transient)
    // Debug.WriteLine("[App] ConfigureDependencies: Регистрация ViewModel секций настроек.");
    services.AddTransient<GeneralSettingsViewModel>();
    services.AddTransient<ThemeSettingsViewModel>();
    services.AddTransient<AboutViewModel>();
    services.AddSingleton<ModelSettingsViewModel>(); // <--- Его конструктор имеет зависимости
    //
    // // Основные ViewModel (Singleton)
    // Debug.WriteLine("[App] ConfigureDependencies: Регистрация SettingsViewModel (Singleton).");
    services.AddTransient<SettingsViewModel>(); // <--- Его конструктор имеет зависимости
    //
    // Debug.WriteLine("[App] ConfigureDependencies: Регистрация CommonViewModel, MolecularViewModel, HomeViewModel.");
    services.AddTransient<CommonViewModel>();
    services.AddTransient<MolecularViewModel>();
    services.AddTransient<HomeViewModel>();

    Debug.WriteLine("[App] ConfigureDependencies: Регистрация MainViewModel (Singleton).");
    services.AddSingleton<MainViewModel>(); // <--- Его конструктор имеет зависимости
    
    // Окна (Transient)
    Debug.WriteLine("[App] ConfigureDependencies: Регистрация MainWindow.");
    services.AddTransient<MainWindow>();
    
    services.UseMicrosoftDependencyResolver();
    Debug.WriteLine("[Program.ConfigureDependencies] Application services registered.");
}

[STAThread]
public static void Main(string[] args)
{
    try
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) => ConfigureDependencies(services))
            .Build();

        BuildAvaloniaApp() 
            .StartWithClassicDesktopLifetime(args); 
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"!!! CRITICAL ERROR IN MAIN: {ex}");
    }
}

private static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>() 
        .UsePlatformDetect()
        .LogToTrace()
        .UseReactiveUI(); 
}
