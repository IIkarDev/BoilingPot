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
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices; // Для LINQ операторов Rx
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

        public PotViewModelBase PotViewModelInstance { get; set; }
        public StoveViewModelBase StoveViewModelInstance { get; set; }
        public BubbleViewModelBase BubbleViewModelInstance { get; set; }


        private bool _isInitialized = false; // Флаг, чтобы инициализировать только один раз


        // --- Свойства для выбора тем (RadioButton) ---
        // Используем ключи тем
        public Styles? StyleContainer { get; }

        [Reactive] public string SelectedPotThemeKey { get; set; } = "Main"; // "Main", "Alt", "Custom"
        [Reactive] public string SelectedStoveThemeKey { get; set; } = "Main"; // "Main", "Alt", "Custom"
        [Reactive] public string SelectedBubbleThemeKey { get; set; } = "Main"; // "Main", "Alt", "Custom"

        // --- Команды для загрузки кастомных тем из файла ---
        public ReactiveCommand<Unit, Unit> LoadPotThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadStoveThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadBubbleThemeCommand { get; }

        // --- Взаимодействие (Interaction) для запроса применения стиля к View ---
        // ViewModel не должен сам менять стили View. Он отправляет запрос View.
        // Входной тип  кортеж (IStyle? стиль, string имя цели), выходной - Unit.
        // Переменные для хранения последних загруженных кастомных стилей (если нужно)
        private Styles? _lastLoadedCustomPotStyle = null;
        private Styles? _lastLoadedCustomStoveStyle = null;
        private Styles? _lastLoadedCustomBubbleStyle = null;
        
        // --- Конструктор ---
        public ModelSettingsViewModel(IServiceProvider serviceProvider, IThemeLoaderService themeLoader)
        {
            StyleContainer = Application.Current?.Styles;

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _themeLoader = themeLoader ?? throw new ArgumentNullException(nameof(themeLoader));
            Debug.WriteLine("[ModelSettingsVM] Конструктор RxUI: Начало");

            // !!! Удаляем создание Interaction !!!
            // ApplyStyleInteraction = new Interaction<Tuple<IStyle?, string>, Unit>();

            // Создаем ЕДИНСТВЕННЫЙ экземпляр PotViewModel
            PotViewModelInstance = _serviceProvider.GetRequiredService<PotViewModelBase>();
            StoveViewModelInstance = _serviceProvider.GetRequiredService<StoveViewModelBase>();
            BubbleViewModelInstance = _serviceProvider.GetRequiredService<BubbleViewModelBase>();
            
            Debug.WriteLine($"[ModelSettingsVM] Конструктор RxUI: PotViewModelInstance создан.");

            // --- Инициализация Команд ---
            LoadPotThemeCommand = ReactiveCommand.CreateFromTask(ExecuteLoadPotThemeAsync);
            LoadStoveThemeCommand = ReactiveCommand.CreateFromTask(ExecuteLoadStoveThemeAsync);
            LoadBubbleThemeCommand = ReactiveCommand.CreateFromTask(ExecuteLoadBubbleThemeAsync);

            // --- Реакция на смену выбранной темы (RadioButtons) ---
            // Подписываемся на изменение ключа выбранной темы для каждого элемента
            this.WhenAnyValue(x => x.SelectedPotThemeKey)
                .Skip(1) // Пропускаем начальное значение
                .Subscribe(key => _ = ApplySelectedThemeAsync(key, "Pot")); // Вызываем асинхронный метод (без await)

            this.WhenAnyValue(x => x.SelectedStoveThemeKey)
                .Skip(1)
                .Subscribe(key => _ = ApplySelectedThemeAsync(key, "Stove"));

            this.WhenAnyValue(x => x.SelectedBubbleThemeKey)
                .Skip(1)
                .Subscribe(key => _ = ApplySelectedThemeAsync(key, "Bubble"));


            // --- Применяем начальные темы при запуске ---
            // Запускаем асинхронные методы без ожидания.
             _ = ApplySelectedThemeAsync(SelectedPotThemeKey, "Pot", isInitial: true);
             _ = ApplySelectedThemeAsync(SelectedStoveThemeKey, "Stove", isInitial: true);
             _ = ApplySelectedThemeAsync(SelectedBubbleThemeKey, "Bubble", isInitial: true);


            Debug.WriteLine("[ModelSettingsVM] Конструктор RxUI: Завершение");
        }
        
         public async Task ApplySelectedThemeAsync(string themeKey, string targetName, bool isInitial = false) // Убрали IObservable<Unit>
        {
             Debug.WriteLine($"[ModelSettingsVM] ApplySelectedThemeAsync: Применяем тему '{themeKey}' для '{targetName}'. Начальная: {isInitial}");

            string? resourceUriString = themeKey switch
            {
                "Main" => $"avares://BoilingPot/Resources/Components/Main{targetName}Theme.axaml" , // Пример формирования URI
                "Alt" => $"avares://BoilingPot/Resources/Components/Alt{targetName}Theme.axaml",   // Пример формирования URI
                "Custom" => null, // Кастомная тема берется из _lastLoadedCustom...Style
                _ => null
            };

            Styles? styleToApply = (Styles)AvaloniaXamlLoader.Load(new Uri(resourceUriString!));
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
            
            if (StyleContainer == null)
            {
                 Debug.WriteLine($"!!! ModelSettingsVM: ApplySelectedThemeAsync: Application.Current.Styles равен null. Не удалось применить стиль для '{targetName}'.");
                 return;
            }
            
            StyleContainer.Add(styleToApply!);
            
        }
         
        // Команда для кнопки "Загрузить тему (.axaml)..." (Pot)
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
                     // Это вызовет WhenAnyValue и запустит ApplySelectedThemeAsync("Custom", "Pot")
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
             Debug.WriteLine("[ModelSettingsVM] LoadBubbleModelCommand: Команда вызвана.");
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