// Program.cs

// --- Подключение необходимых пространств имен ---

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.ReactiveUI;
using BoilingPot.Services;
using BoilingPot.ViewModels;
using BoilingPot.ViewModels.Components;
using BoilingPot.ViewModels.SettingsViewModels;
using BoilingPot.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Splat.Microsoft.Extensions.DependencyInjection;
// Базовое пространство имен Avalonia
// Для интеграции ReactiveUI с Avalonia (.UseReactiveUI())
// Для базовых типов, таких как IServiceProvider, STAThread, Exception
// Для вывода отладочной информации (Debug.WriteLine)
// Пространство имен для ваших сервисов (IThemeLoaderService, ThemeLoaderService)
// Пространство имен для основных ViewModel (MainViewModel, HomeViewModel и т.д.)
// Пространство имен для ViewModel компонентов (PotViewModelBase, StoveViewModelBase)
// Пространство имен для ViewModel секций настроек
// Пространство имен для ваших Views (MainWindow)
// Основное пространство имен для DI (IServiceCollection, AddSingleton, AddTransient)
// Для IHost и Host.CreateDefaultBuilder()

// Адаптер Splat для использования Microsoft DI с ReactiveUI

// --- Основное пространство имен вашего приложения ---
namespace BoilingPot;

// Класс Program - это традиционная точка входа для .NET приложений.
// В современных шаблонах Avalonia DI часто настраивается здесь,
// а класс App становится более "легким".
internal class Program // 'internal' означает, что класс доступен только внутри этой сборки
{
    // --- Метод Конфигурации Зависимостей ---
    // Этот статический метод отвечает за регистрацию всех сервисов и ViewModel
    // в DI контейнере.
    private static void ConfigureDependencies(IServiceCollection services)
    {
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Начинаем регистрацию сервисов приложения.");

        // --- Регистрация Сервисов ---
        // IThemeLoaderService будет создан как Singleton - один экземпляр на все приложение.
        // При запросе IThemeLoaderService будет предоставлен экземпляр ThemeLoaderService.
        Debug.WriteLine(
            $"[{nameof(Program)}.ConfigureDependencies] Регистрация IThemeLoaderService как Singleton (реализация ThemeLoaderService).");
        services.AddSingleton<IThemeLoaderService, ThemeLoaderService>();

        // --- Регистрация ViewModel Компонентов ---
        // PotViewModelBase регистрируется как Singleton. Это означает, что все части приложения,
        // которые запрашивают PotViewModelBase, будут получать ОДИН И ТОТ ЖЕ экземпляр.
        // Это важно для вашей логики, где PotPresenter должен отображать состояние
        // именно этого единственного экземпляра.
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация PotViewModelBase как Singleton.");
        services.AddSingleton<PotViewModelBase>();

        // IPotViewModel также регистрируется как Singleton и будет разрешаться
        // в тот же экземпляр, что и PotViewModelBase.
        // sp.GetRequiredService<PotViewModelBase>() гарантирует, что DI контейнер
        // найдет зарегистрированный PotViewModelBase и вернет его.
        Debug.WriteLine(
            $"[{nameof(Program)}.ConfigureDependencies] Регистрация IPotViewModel как Singleton (реализуется через PotViewModelBase).");
        services.AddSingleton<IPotViewModel>(sp => sp.GetRequiredService<PotViewModelBase>());

        // StoveViewModelBase регистрируется как Transient - НОВЫЙ экземпляр
        // будет создаваться каждый раз при запросе.
        // Если вам нужен один экземпляр плиты, измените на AddSingleton.
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация StoveViewModelBase как Transient.");
        services.AddTransient<StoveViewModelBase>();
        // TODO: Аналогично зарегистрировать BubbleViewModelBase (если он существует)
        
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация StoveViewModelBase как Transient.");
        services.AddTransient<BubbleViewModelBase>();

        // --- Регистрация ViewModel Секций Настроек ---
        // ViewModel для каждой секции настроек регистрируются как Transient.
        // Это означает, что при каждом открытии окна/панели настроек и переходе
        // на секцию, будет создан новый экземпляр ViewModel этой секции.
        // Если состояние секции должно сохраняться между открытиями, рассмотрите Singleton.
        Debug.WriteLine(
            $"[{nameof(Program)}.ConfigureDependencies] Регистрация GeneralSettingsViewModel как Transient.");
        services.AddTransient<GeneralSettingsViewModel>();
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация ThemeSettingsViewModel как Transient.");
        services.AddTransient<ThemeSettingsViewModel>();
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация AboutViewModel как Transient.");
        services.AddTransient<AboutViewModel>(); // ViewModel для окна "О программе"

        // ModelSettingsViewModel регистрируется как Singleton. Это ViewModel,
        // который содержит логику для вкладки "Модели" в настройках, включая
        // PotViewModelInstance. Так как PotViewModelInstance у нас один,
        // и ModelSettingsViewModel им управляет, логично сделать его Singleton.
        // DI контейнер автоматически внедрит в его конструктор IServiceProvider и IThemeLoaderService.
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация ModelSettingsViewModel как Singleton.");
        services.AddSingleton<ModelSettingsViewModel>();

        // --- Регистрация Основных ViewModel ---
        // SettingsViewModel (для всего окна/панели настроек) регистрируется как Transient.
        // При каждом вызове команды "Показать Настройки", если SettingsView создается заново,
        // будет создан и новый SettingsViewModel. Если панель настроек одна и та же,
        // и вы просто меняете ее видимость, то лучше сделать SettingsViewModel Singleton.
        // В вашей текущей реализации MainWindow он, скорее всего, Singleton через MainViewModel.
        // Регистрация здесь как Transient для примера, если бы он запрашивался отдельно.
        // Учитывая, что он получается через MainViewModel.SettingsVM, его время жизни
        // будет таким же, как у MainViewModel (Singleton).
        Debug.WriteLine(
            $"[{nameof(Program)}.ConfigureDependencies] Регистрация SettingsViewModel (пока Transient, но будет Singleton через MainViewModel).");
        services.AddSingleton<SettingsViewModel>();

        // ViewModel для основных "экранов" приложения.
        // Transient означает, что при каждом переключении на этот экран будет
        // создаваться новый экземпляр ViewModel, и его состояние будет сбрасываться.
        // Если нужно сохранять состояние экрана, используйте Singleton.
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация CommonViewModel как Transient.");
        services.AddTransient<CommonViewModel>();
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация MolecularViewModel как Transient.");
        services.AddTransient<MolecularViewModel>();
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация HomeViewModel как Transient.");
        services.AddTransient<HomeViewModel>();

        // MainViewModel - главный управляющий ViewModel приложения.
        // Он должен быть Singleton, так как хранит общее состояние навигации.
        // DI контейнер внедрит в него IServiceProvider, IThemeLoaderService и SettingsViewModel.
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация MainViewModel как Singleton.");
        services.AddSingleton<MainViewModel>();

        // --- Регистрация Окон (Views) ---
        // Главное окно приложения регистрируется как Transient.
        // Обычно создается один экземпляр при запуске.
        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация MainWindow как Transient.");
        services.AddTransient<MainWindow>();

        // --- Настройка Splat для интеграции с Microsoft DI ---
        // Этот вызов ОБЯЗАТЕЛЕН, чтобы ReactiveUI (через Splat) мог использовать
        // наш DI контейнер Microsoft.Extensions.DependencyInjection для разрешения зависимостей
        // (например, при создании View через ViewLocator, если он использует Splat,
        // или для команд, которые могут разрешать зависимости).
        Debug.WriteLine(
            $"[{nameof(Program)}.ConfigureDependencies] Настройка Splat для использования Microsoft DI Resolver.");
        services.UseMicrosoftDependencyResolver();

        Debug.WriteLine($"[{nameof(Program)}.ConfigureDependencies] Регистрация сервисов приложения завершена.");
    }

    // --- Точка Входа в Приложение ---
    [STAThread] // Атрибут для UI-приложений, работающих в однопоточном апартаменте (нужен для Windows)
    public static void Main(string[] args)
    {
        try
        {
            Debug.WriteLine($"[{nameof(Program)}.Main] Запуск приложения...");

            // Создаем и конфигурируем хост .NET Generic Host.
            // 1. CreateDefaultBuilder(args) - создает построитель с настройками по умолчанию
            //    (логирование, конфигурация из appsettings.json, переменных окружения и т.д.).
            // 2. ConfigureServices(...) - вызываем наш метод для регистрации всех зависимостей.
            // 3. Build() - собирает хост и создает DI контейнер (IServiceProvider).
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => ConfigureDependencies(services))
                .Build();

            // Важно: Если у вас класс App (App.axaml.cs) будет сам использовать DI-контейнер
            // (например, статическое свойство App. Services), то нужно как-то передать
            // IServiceProvider из host. Services в экземпляр App.
            // Один из способов - это сделать в App. OnFrameworkInitializationCompleted,
            // если AppHost будет статическим или доступен через Application. Current.
            // В вашей текущей реализации App.axaml.cs уже есть AppHost.

            Debug.WriteLine($"[{nameof(Program)}.Main] Хост .NET Generic Host создан. Запускаем Avalonia UI...");

            // Запускаем приложение Avalonia.
            // BuildAvaloniaApp() конфигурирует Avalonia,
            // StartWithClassicDesktopLifetime(args) запускает его с жизненным циклом для десктопа.
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

            Debug.WriteLine(
                $"[{nameof(Program)}.Main] Приложение Avalonia завершило работу (или StartWithClassicDesktopLifetime блокирует до закрытия).");
        }
        catch (Exception ex)
        {
            // Ловим и выводим любые критические ошибки, возникшие при запуске.
            Debug.WriteLine($"!!! КРИТИЧЕСКАЯ ОШИБКА В MAIN: {ex}");
            // Здесь можно добавить логирование в файл или вывод сообщения пользователю.
        }
    }

    // --- Метод Конфигурации Avalonia AppBuilder ---
    // Этот метод настраивает, как будет работать Avalonia.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>() // Указываем главный класс приложения (App.axaml.cs)
            .UsePlatformDetect() // Автоматически определяет ОС для нативной интеграции
            .LogToTrace() // Включает базовое логирование Avalonia в отладчик
            .UseReactiveUI();
        // <<< Инициализируем поддержку ReactiveUI >>>
    }
}