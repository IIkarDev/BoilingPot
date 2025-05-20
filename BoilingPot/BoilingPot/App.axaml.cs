// App.axaml.cs

// --- Подключение необходимых пространств имен ---

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BoilingPot.ViewModels;
using BoilingPot.Views;
using Splat;
// Базовое пространство имен Avalonia
// Для IClassicDesktopStyleApplicationLifetime
// Для AvaloniaXamlLoader
// Пространство имен для MainViewModel
// Пространство имен для MainWindow
// Для InvalidOperationException
// Для Debug.WriteLine

// Для Locator.Current и GetService (используется ReactiveUI для DI)

// --- Основное пространство имен вашего приложения ---
namespace BoilingPot; // Используйте ваше пространство имен, если отличается

// Класс App - это главный класс вашего Avalonia-приложения.
// Он наследуется от Avalonia.Application.
// Модификатор 'partial' означает, что часть этого класса генерируется из App.axaml.
public class App : Application
{
    // Метод Initialize() вызывается Avalonia при инициализации XAML-части приложения.
    // Он должен быть здесь для загрузки App.axaml.
    public override void Initialize()
    {
        Debug.WriteLine($"[{nameof(App)}.Initialize] Начало загрузки XAML для App.");
        AvaloniaXamlLoader.Load(this); // Загружает XAML-разметку из App.axaml
        Debug.WriteLine($"[{nameof(App)}.Initialize] XAML для App загружен.");
    }

    // Метод OnFrameworkInitializationCompleted() вызывается ПОСЛЕ того, как
    // фреймворк Avalonia полностью инициализирован, но ДО начала обработки
    // сообщений операционной системы (т.е. до фактического показа окна).
    // Это стандартное место для настройки главного окна и его DataContext.
    public override void OnFrameworkInitializationCompleted()
    {
        Debug.WriteLine($"[{nameof(App)}.OnFrameworkInitializationCompleted] Начало.");

        // Проверяем, запущено ли приложение как десктопное (Windows, macOS, Linux).
        // Для мобильных или веб-приложений (SingleView) жизненный цикл будет другим.
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] Жизненный цикл: IClassicDesktopStyleApplicationLifetime.");

            // --- Получение Главного Окна и ViewModel из DI контейнера через Splat ---
            // Locator.Current.GetService<T>() - это способ, которым ReactiveUI (через Splat)
            // получает экземпляры из DI контейнера, который мы настроили
            // в Program.cs (services.UseMicrosoftDependencyResolver()).

            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] Попытка получить MainWindow из DI (Splat).");
            var mainWindow = Locator.Current.GetService<MainWindow>();

            // Критически важно проверить, что MainWindow был успешно разрешен.
            if (mainWindow == null)
            {
                var errorMsg =
                    "!!! КРИТИЧЕСКАЯ ОШИБКА: MainWindow не удалось разрешить из Locator.Current! Проверьте регистрацию в DI.";
                Debug.WriteLine(errorMsg);
                // Выбрасываем исключение, так как без главного окна приложение не сможет работать.
                throw new InvalidOperationException(errorMsg);
            }

            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] MainWindow успешно получен (тип: {mainWindow.GetType().FullName}).");

            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] Попытка получить MainViewModel из DI (Splat).");
            var mainViewModel = Locator.Current.GetService<MainViewModel>();

            // Также критически важно проверить, что MainViewModel был успешно разрешен.
            if (mainViewModel == null)
            {
                var errorMsg =
                    "!!! КРИТИЧЕСКАЯ ОШИБКА: MainViewModel не удалось разрешить из Locator.Current! Проверьте регистрацию в DI.";
                Debug.WriteLine(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] MainViewModel успешно получен (тип: {mainViewModel.GetType().FullName}).");

            // --- Связывание Окна и ViewModel ---
            // Устанавливаем полученный MainViewModel в качестве DataContext для MainWindow.
            // Теперь все привязки {Binding ...} в MainWindow.axaml будут искать
            // свойства и команды в этом экземпляре MainViewModel.
            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] Установка DataContext для MainWindow.");
            mainWindow.DataContext = mainViewModel;

            // Назначаем созданное и настроенное окно главным окном приложения.
            // После этого оно будет показано пользователю.
            desktop.MainWindow = mainWindow;
            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] MainWindow назначено и готово к показу.");
        }
        else
        {
            // Лог на случай, если жизненный цикл не десктопный (для отладки)
            Debug.WriteLine(
                $"[{nameof(App)}.OnFrameworkInitializationCompleted] Жизненный цикл НЕ IClassicDesktopStyleApplicationLifetime (тип: {ApplicationLifetime?.GetType().FullName}). Главное окно не настроено для этого типа.");
        }

        // Вызываем базовую реализацию метода. Это важно для завершения
        // инициализации Avalonia.
        base.OnFrameworkInitializationCompleted();
        Debug.WriteLine($"[{nameof(App)}.OnFrameworkInitializationCompleted] Завершено успешно.");
    }
}