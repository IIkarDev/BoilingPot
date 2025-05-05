// ViewModels/SettingsViewModels/ModelSettingsViewModel.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Styling; // Для IStyle
using BoilingPot.Services;
using BoilingPot.ViewModels.Components; // Для IPotViewModel
using ReactiveUI; // Базовый класс RxUI
using ReactiveUI.Fody.Helpers; // Для [Reactive]
using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider
using System;
using System.Diagnostics; // Для Debug
using System.IO;
using System.Reactive; // Для Unit, Interaction
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks; // Для LINQ операторов Rx
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml; // Для ICommand

namespace BoilingPot.ViewModels.SettingsViewModels
{
    // ViewModel для секции "Настройки Моделей".
    // Управляет выбором внешнего вида для элементов симуляции (кастрюля, плита, пузыри).
    public class ModelSettingsViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThemeLoaderService _themeLoader; // Сервис загрузки тем

        // ViewModel для кастрюли (один экземпляр, DataContext для PotPresenter)
        public PotViewModelBase PotViewModelInstance { get; }
        
        private bool _isInitialized = false; // Флаг, чтобы инициализировать только один раз


        // --- Свойства для выбора тем (RadioButton) ---
        // Используем ключи тем
        [Reactive] public string SelectedPotThemeKey { get; set; } = "Main"; // "Main", "Alt", "Custom"
        [Reactive] public string SelectedStoveThemeKey { get; set; } = "Main"; // "Main", "Alt", "Custom"
        [Reactive] public string SelectedBubbleThemeKey { get; set; } = "Main"; // "Main", "Alt", "Custom"

        // --- Команды для загрузки кастомных тем из файла ---
        public ReactiveCommand<Unit, Unit> LoadPotThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadStoveThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadBubbleThemeCommand { get; }

        // --- Взаимодействие (Interaction) для запроса применения стиля к View ---
        // ViewModel не должен сам менять стили View. Он отправляет запрос View.
        // Входной тип - кортеж (IStyle? стиль, string имя цели), выходной - Unit.
        public Interaction<Tuple<IStyle?, string>, Unit> ApplyStyleInteraction { get; }

        // Переменные для хранения последних загруженных кастомных стилей (если нужно)
        private IStyle? _lastLoadedCustomPotStyle = null;
        private IStyle? _lastLoadedCustomStoveStyle = null;
        private IStyle? _lastLoadedCustomBubbleStyle = null;


        // --- Конструктор ---
        public ModelSettingsViewModel(IServiceProvider serviceProvider, IThemeLoaderService themeLoader)
        {
            _serviceProvider = serviceProvider;
            _themeLoader = themeLoader;
            Debug.WriteLine("[ModelSettingsVM] Конструктор RxUI: Начало");

            // Создаем Interaction для применения стиля
            ApplyStyleInteraction = new Interaction<Tuple<IStyle?, string>, Unit>();

            // Создаем ЕДИНСТВЕННЫЙ экземпляр PotViewModel для всего приложения.
            // DI подставит реализацию по умолчанию (MainPotViewModel, если так зарегистрировано)
            PotViewModelInstance = _serviceProvider.GetRequiredService<PotViewModelBase>();
            Debug.WriteLine($"[ModelSettingsVM] Конструктор RxUI: PotViewModelInstance создан (тип: {PotViewModelInstance.GetType().Name}).");

            // --- Инициализация Команд ---
            LoadPotThemeCommand = ReactiveCommand.CreateFromTask(ExecuteLoadPotThemeAsync);
            LoadStoveThemeCommand = ReactiveCommand.CreateFromTask(ExecuteLoadStoveThemeAsync); // Теперь тоже асинхронная
            LoadBubbleThemeCommand = ReactiveCommand.CreateFromTask(ExecuteLoadBubbleThemeAsync); // Теперь тоже асинхронная

            // --- Реакция на смену выбранной темы (RadioButtons) ---
            // Подписываемся на изменение ключа выбранной темы для каждого элемента
            this.WhenAnyValue(x => x.SelectedPotThemeKey)
                .Skip(1) // Пропускаем начальное значение
                .SelectMany(key => ApplySelectedThemeAsync(key, "Pot")) // Выполняем асинхронный метод применения
                .Subscribe(); // Просто подписываемся, чтобы поток выполнялся

            this.WhenAnyValue(x => x.SelectedStoveThemeKey)
                .Skip(1)
                .SelectMany(key => ApplySelectedThemeAsync(key, "Stove"))
                .Subscribe();

            this.WhenAnyValue(x => x.SelectedBubbleThemeKey)
                .Skip(1)
                .SelectMany(key => ApplySelectedThemeAsync(key, "Bubble"))
                .Subscribe();


            // --- Применяем начальные темы при запуске ---
            // Запускаем асинхронные методы без ожидания (_ = ...).
            // Логика загрузки/применения стилей внутри этих методов.
            _ = ApplySelectedThemeAsync(SelectedPotThemeKey, "Pot", isInitial: true);
            _ = ApplySelectedThemeAsync(SelectedStoveThemeKey, "Stove", isInitial: true);
            _ = ApplySelectedThemeAsync(SelectedBubbleThemeKey, "Bubble", isInitial: true);


            Debug.WriteLine("[ModelSettingsVM] Конструктор RxUI: Завершение");
        }

        public async Task InitializeAsync() // Делаем асинхронным
        {
            if (_isInitialized)
            {
                Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Уже инициализировано. Выход.");
                return; // Инициализируем только один раз
            }
            _isInitialized = true;
            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Начало инициализации.");

            // --- Применение начальных тем при инициализации ---
            // Используем await для выполнения асинхронной загрузки и применения тем.
            // Логика загрузки/применения стилей внутри этих методов.
            // Вызываем ApplySelectedThemeAsync для текущих значений ключей тем.
            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Применяем начальную тему для Pot (ключ: {SelectedPotThemeKey}).");
            await ApplySelectedThemeAsync(SelectedPotThemeKey, "Pot", isInitial: true).ToTask(); // Преобразуем Observable в Task для await

            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Применяем начальную тему для Stove (ключ: {SelectedStoveThemeKey}).");
            await ApplySelectedThemeAsync(SelectedStoveThemeKey, "Stove", isInitial: true).ToTask();

            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Применяем начальную тему для Bubble (ключ: {SelectedBubbleThemeKey}).");
            await ApplySelectedThemeAsync(SelectedBubbleThemeKey, "Bubble", isInitial: true).ToTask();


            // TODO: Здесь может быть другая логика инициализации, например,
            // загрузка настроек, инициализация симуляции пузырьков и т.д.
            // if (PotViewModelInstance is MolecularViewModel mvm)
            // {
            //     await mvm.InitializeAsync(...);
            // }


            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Инициализация завершена.");
        }
        // --- Методы для загрузки и применения тем ---

        // Асинхронный метод для загрузки и применения ТЕКУЩЕЙ выбранной темы
        // Вызывается при изменении SelectedThemeKey или при инициализации.
        // Возвращает IObservable<Unit>, чтобы SelectMany мог на него подписаться.
        private IObservable<Unit> ApplySelectedThemeAsync(string themeKey, string targetName, bool isInitial = false)
        {
             Debug.WriteLine($"[ModelSettingsVM] ApplySelectedThemeAsync: Применяем тему '{themeKey}' для '{targetName}'. Начальная: {isInitial}");

            string? resourceUriString = themeKey switch
            {
                "Main" => $"avares://BoilingPot/Resources/Components/Main{targetName}Theme.axaml", // Пример формирования URI
                "Alt" => $"avares://BoilingPot/Resources/Components/Alt{targetName}Theme.axaml",   // Пример формирования URI
                "Custom" => null, // Кастомная тема берется из _lastLoadedCustom...Style
                _ => null
            };

            IStyle? styleToApply = null;
            if (themeKey == "Custom") // Если выбран кастомный, берем из сохраненной переменной
            {
                 styleToApply = targetName switch
                 {
                      "Pot" => _lastLoadedCustomPotStyle,
                      "Stove" => _lastLoadedCustomStoveStyle,
                      "Bubble" => _lastLoadedCustomBubbleStyle,
                      _ => null
                 };
                 Debug.WriteLine($"[ModelSettingsVM] ApplySelectedThemeAsync: Берем кастомный стиль для '{targetName}' (IsNull={styleToApply == null}).");
            }

            // --- Логика загрузки из ресурса (если themeKey не "Custom") ---
            // Используем SelectMany для обработки асинхронной загрузки
            // Возвращаем Observable, который завершится после загрузки
            return Observable.StartAsync(async () =>
            {
                if (styleToApply == null && !string.IsNullOrEmpty(resourceUriString)) // Если стиль еще не определен (не кастомный)
                {
                    var themeUri = new Uri(resourceUriString, UriKind.Absolute);
                    try
                    {
                        if (Avalonia.Platform.AssetLoader.Exists(themeUri))
                        {
                            using var stream = Avalonia.Platform.AssetLoader.Open(themeUri);
                            using var reader = new StreamReader(stream);
                            string xamlContent = await reader.ReadToEndAsync(); // Асинхронное чтение
                            var loadedObject = AvaloniaRuntimeXamlLoader.Load(xamlContent); // Парсинг

                            if (loadedObject is Styles styles && styles.Count > 0) styleToApply = styles[0];
                            else if (loadedObject is IStyle style) styleToApply = style;

                            if (styleToApply != null) Debug.WriteLine($"[ModelSettingsVM] Тема '{themeKey}' из ресурсов загружена для '{targetName}'.");
                            else Debug.WriteLine($"!!! ОШИБКА: XAML из '{resourceUriString}' для '{targetName}' не содержит Styles или IStyle.");
                        } else { Debug.WriteLine($"!!! ОШИБКА: Файл темы '{resourceUriString}' не найден в ресурсах для '{targetName}'."); }
                    }
                    catch (Exception ex) { Debug.WriteLine($"!!! КРИТИЧЕСКАЯ ОШИБКА загрузки темы '{resourceUriString}' для '{targetName}': {ex}"); }
                }

                // --- Вызываем Interaction для применения стиля ---
                // Передаем стиль и имя цели.
                // .Handle() возвращает IObservable<Unit>, используем await.
                // Если styleToApply null, это будет запрос на очистку стиля.
                 Debug.WriteLine($"[ModelSettingsVM] Вызов ApplyStyleInteraction для '{targetName}'. Стиль IsNull={styleToApply == null}");
                await ApplyStyleInteraction.Handle(Tuple.Create(styleToApply, targetName));
                 Debug.WriteLine($"[ModelSettingsVM] ApplyStyleInteraction для '{targetName}' обработан.");

                // Важно: Возвращаем Unit.Default, чтобы SelectMany продолжил выполнение
                return Unit.Default;
            })
            // Обработка ошибок в потоке (опционально)
            .Catch<Unit, Exception>(ex => {
                 Debug.WriteLine($"!!! Observable Catch: Ошибка в потоке ApplySelectedThemeAsync для '{targetName}': {ex}");
                 // Можно вернуть пустой Observable.Return(Unit.Default);
                 return Observable.Return(Unit.Default);
            });
        }

        // Асинхронный метод для команды загрузки файла темы (Pot)
        private async Task ExecuteLoadPotThemeAsync()
        {
            Debug.WriteLine("[ModelSettingsVM] LoadPotThemeCommand: Команда вызвана.");
            var topLevel = AppExtensions.GetTopLevel(Application.Current);
            if (topLevel == null) { Debug.WriteLine("[ModelSettingsVM] LoadPotThemeCommand: Не удалось получить TopLevel."); return; }

            Debug.WriteLine("[ModelSettingsVM] LoadPotThemeCommand: Показ диалога выбора файла...");
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите файл темы (.axaml) для кастрюли",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("AXAML Files") { Patterns = new[] { "*.axaml" } } }
            });

            if (files.Count >= 1)
            {
                string filePath = files[0].Path.AbsolutePath;
                Debug.WriteLine($"[ModelSettingsVM] LoadPotThemeCommand: Выбран файл: {filePath}");
                var loadedStyle = _themeLoader.LoadStyleFromFile(filePath); // Используем сервис

                if (loadedStyle != null)
                {
                     Debug.WriteLine($"[ModelSettingsVM] ExecuteLoadPotThemeAsync: Стиль из файла загружен.");
                     _lastLoadedCustomPotStyle = loadedStyle; // Сохраняем загруженный стиль

                     // !!! Активируем выбор кастомной темы !!!
                     // Это вызовет WhenAnyValue(x => x.SelectedPotThemeKey) и запустит ApplySelectedThemeAsync("Custom", "Pot")
                     SelectedPotThemeKey = "Custom"; // Устанавливаем ключ
                     Debug.WriteLine("[ModelSettingsVM] LoadPotThemeCommand: SelectedPotThemeKey установлен в 'Custom'.");
                }
                else
                {
                    Debug.WriteLine($"!!! ОШИБКА: Не удалось загрузить стиль из {Path.GetFileName(filePath)}.");
                    // TODO: Сообщить об ошибке пользователю (Interaction?)
                    // TODO: Возможно, сбросить RadioButton выбор на предыдущий, если загрузка не удалась
                }
            } else { Debug.WriteLine("[ModelSettingsVM] LoadPotThemeCommand: Выбор файла отменен или не удалось получить путь."); }
        }

        // Асинхронный метод для команды загрузки файла темы (Stove)
        private async Task ExecuteLoadStoveThemeAsync()
        {
             Debug.WriteLine("[ModelSettingsVM] LoadStoveThemeCommand: Команда вызвана.");
             var topLevel = AppExtensions.GetTopLevel(Application.Current);
             if (topLevel == null) return;
             var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { /* ... */ });

             if (files.Count >= 1)
             {
                 string filePath = files[0].Path.AbsolutePath;
                 var loadedStyle = _themeLoader.LoadStyleFromFile(filePath);
                 if (loadedStyle != null)
                 {
                      _lastLoadedCustomStoveStyle = loadedStyle;
                      SelectedStoveThemeKey = "Custom";
                 } else { /* Ошибка */ }
             }
        }

        // Асинхронный метод для команды загрузки файла темы (Bubble)
        private async Task ExecuteLoadBubbleThemeAsync()
        {
             Debug.WriteLine("[ModelSettingsVM] LoadBubbleThemeCommand: Команда вызвана.");
             var topLevel = AppExtensions.GetTopLevel(Application.Current);
             if (topLevel == null) return;
             var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { /* ... */ });

             if (files.Count >= 1)
             {
                 string filePath = files[0].Path.AbsolutePath;
                 var loadedStyle = _themeLoader.LoadStyleFromFile(filePath);
                 if (loadedStyle != null)
                 {
                      _lastLoadedCustomBubbleStyle = loadedStyle;
                      SelectedBubbleThemeKey = "Custom";
                 } else { /* Ошибка */ }
             }
        }
    }

    // AppExtensions оставляем как есть
    // public static class AppExtensions { /* ... */ }


    // Хелпер AppExtensions оставляем как есть

    // Вспомогательный класс для TopLevel и фильтра файлов (можно вынести в отдельный файл)
    public static class AppExtensions
    {
        public static TopLevel? GetTopLevel(this Avalonia.Application? app)
        {
            if (app?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }

            return null;
        }
    }
}