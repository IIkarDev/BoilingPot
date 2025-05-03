// ViewModels/SettingsViewModels/ModelSettingsViewModel.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Styling; // Для IStyle
using BoilingPot.Services;
using BoilingPot.ViewModels.Components; // Для IPotViewModel
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Diagnostics; // <<< ДОБАВИТЬ using для Debug !!!

namespace BoilingPot.ViewModels.SettingsViewModels
{
    public partial class ModelSettingsViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPluginLoaderService _pluginLoader;

        public IPotViewModel PotViewModelInstance { get; }

        [ObservableProperty] private bool _isMainPotTheme = true;
        [ObservableProperty] private bool _isAltPotTheme = false;
        [ObservableProperty] private bool _isCustomPotTheme = false;

        [ObservableProperty] private bool _isMainStoveTheme = true;
        [ObservableProperty] private bool _isAltStoveTheme = false;
        [ObservableProperty] private bool _isCustomStoveTheme = false;

        [ObservableProperty] private bool _isMainBubbleTheme = true;
        [ObservableProperty] private bool _isAltBubbleTheme = false;
        [ObservableProperty] private bool _isCustomBubbleTheme = false;

        // Событие для запроса применения стиля к View
        public event Action<IStyle?, string>? ApplyStyleRequested; // Стиль, ИмяПлейсхолдера

        public ModelSettingsViewModel(IServiceProvider serviceProvider, IPluginLoaderService pluginLoader)
        {
            _serviceProvider = serviceProvider;
            _pluginLoader = pluginLoader;
            Debug.WriteLine("[ModelSettingsVM] Конструктор: Начало");

            // Создаем ЕДИНСТВЕННЫЙ экземпляр PotViewModel
            PotViewModelInstance = _serviceProvider.GetRequiredService<PotViewModelBase>(); // Используем базовый класс, DI подставит реализацию
            Debug.WriteLine($"[ModelSettingsVM] Конструктор: PotViewModelInstance создан (тип: {PotViewModelInstance.GetType().Name}).");

            // Применяем начальную тему (MainPotTheme) при запуске
            // ApplyInitialTheme();
            Debug.WriteLine("[ModelSettingsVM] Конструктор: Завершение");
        }
        
        private bool _isInitialized = false;

        public async Task InitializeAsync() // Делаем асинхронным
        {
            if (_isInitialized) return; // Инициализируем только один раз
            _isInitialized = true;

            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Начало применения начальной темы.");
            var themeUri = new Uri("avares://BoilingPot/Resources/Components/AltPotTheme.axaml");
            await ApplyThemeInternal(themeUri, "Pot", isInitial: true);
            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Завершение применения начальной темы.");

            // Применить начальные темы и для других элементов (плита, пузыри), если нужно
            // await ApplyThemeInternal(new Uri("...Stove..."), "Stove", isInitial: true);
        }

        // Вызывается при изменении выбора RadioButton
        partial void OnIsMainPotThemeChanged(bool value)
        {
            Debug.WriteLine($"[ModelSettingsVM] OnIsMainPotThemeChanged: Новое значение = {value}");
            if (value)
            {
                Debug.WriteLine("[ModelSettingsVM] OnIsMainPotThemeChanged: Применяем MainPotTheme.");
                ApplyTheme("avares://BoilingPot/Resources/Components/MainPotTheme.axaml", "Pot");
            }
        }

        partial void OnIsAltPotThemeChanged(bool value)
        {
            Debug.WriteLine($"[ModelSettingsVM] OnIsAltPotThemeChanged: Новое значение = {value}");
            if (value)
            {
                 Debug.WriteLine("[ModelSettingsVM] OnIsAltPotThemeChanged: Применяем AltPotTheme.");
                 ApplyTheme("avares://BoilingPot/Resources/Components/AltPotTheme.axaml", "Pot");
            }
        }

        partial void OnIsCustomPotThemeChanged(bool value)
        {
             Debug.WriteLine($"[ModelSettingsVM] OnIsCustomPotThemeChanged: Новое значение = {value}");
            if (value)
            {
                Debug.WriteLine("[ModelSettingsVM] OnIsCustomPotThemeChanged: Выбрана кастомная тема, инициируем загрузку (если стиль еще не загружен).");
                // Здесь можно добавить проверку, загружен ли уже кастомный стиль
                // Если да - применить его, если нет - вызвать LoadPotThemeCommand
                LoadPotThemeCommand.Execute(null); // В любом случае покажем диалог
            }
        }


        // Общий метод для загрузки и применения стиля по URI ресурса приложения
        private async void ApplyTheme(string uriString, string targetName)
        {
             Debug.WriteLine($"[ModelSettingsVM] ApplyTheme: Попытка применить тему из URI ресурса: {uriString} для {targetName}");
             var themeUri = new Uri(uriString, UriKind.Absolute); // Указываем, что это абсолютный avares URI
             await ApplyThemeInternal(themeUri, targetName);
        }

        // Внутренний метод для загрузки и применения стиля (из URI или FilePath)
        private async Task ApplyThemeInternal(Uri themeUri, string targetName, bool isInitial = false, string? filePath = null)
        {
            IStyle? loadedStyle = null;
            string sourceDescription = filePath ?? themeUri.ToString(); // Источник для логов

            try
            {
                 Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Загрузка стиля из '{sourceDescription}' для '{targetName}'");
                 Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Файл существует '" +
                                 $"{File.Exists(filePath)}'");
                 string xamlContent;
                 if (filePath != null && File.Exists(filePath)) // Загрузка из файла
                 {
                      xamlContent = await File.ReadAllTextAsync(filePath);
                      Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Содержимое файла '{Path.GetFileName(filePath)}' прочитано.");
                 }
                 else if (themeUri != null && Avalonia.Platform.AssetLoader.Exists(themeUri)) // Загрузка из ресурсов
                 {
                     using var stream = Avalonia.Platform.AssetLoader.Open(themeUri);
                     using var reader = new StreamReader(stream);
                     xamlContent = await reader.ReadToEndAsync();
                     Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Ресурс '{themeUri}' прочитан.");
                 }
                 else if (themeUri == null)
                 {
                      Debug.WriteLine($"!!! ОШИБКА: Источник стиля '{sourceDescription}' не найден.");
                      return; // Выходим, если источник не найден
                 }
                 else
                 {
                     return;
                 }

                 // Парсим XAML
                 Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Попытка парсинга XAML для '{targetName}'...");
                 var loadedObject = AvaloniaRuntimeXamlLoader.Load(xamlContent);
                 Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: XAML распарсен, тип объекта: {loadedObject?.GetType().FullName ?? "null"}");

                 // Извлекаем стиль
                if (loadedObject is Styles styles && styles.Count > 0)
                {
                    loadedStyle = styles[0]; // Берем первый стиль из <Styles>
                     Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Извлечен стиль (первый из <Styles>) для '{targetName}'.");
                }
                else if (loadedObject is IStyle style)
                {
                    loadedStyle = style; // Корневой элемент был <Style> или <ControlTheme>
                     Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Извлечен стиль (корневой элемент) для '{targetName}'.");
                }
                else
                {
                    Debug.WriteLine($"!!! ОШИБКА: Загруженный XAML из '{sourceDescription}' не является Styles или IStyle для '{targetName}'.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! КРИТИЧЕСКАЯ ОШИБКА загрузки/парсинга стиля '{sourceDescription}' для '{targetName}': {ex}");
                // Возможно, показать ошибку пользователю
            }

             // Применяем стиль, если он был успешно загружен
            if (loadedStyle != null)
            {
                Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Вызов ApplyStyleRequested для '{targetName}'...");
                ApplyStyleRequested?.Invoke(loadedStyle, targetName); // Отправляем событие для View
                Debug.WriteLine($"[ModelSettingsVM] ApplyStyleRequested для '{targetName}' вызвано.");
            } else {
                 Debug.WriteLine($"[ModelSettingsVM] ApplyThemeInternal: Стиль для '{targetName}' не был загружен/извлечен.");
            }
        }


        // Команда для кнопки "Загрузить тему (.axaml)..."
        [RelayCommand]
        private async Task LoadPotTheme()
        {
            Debug.WriteLine("[ModelSettingsVM] LoadPotTheme: Команда вызвана.");
            var topLevel = AppExtensions.GetTopLevel(Application.Current);
            if (topLevel == null) {
                 Debug.WriteLine("[ModelSettingsVM] LoadPotTheme: Не удалось получить TopLevel.");
                 return;
            }

            Debug.WriteLine("[ModelSettingsVM] LoadPotTheme: Показ диалога выбора файла...");
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите файл темы (.axaml) для кастрюли",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("AXAML Files") { Patterns = new[] { "*.axaml" } } }
            });

            if (files.Count >= 1)
            {
                string filePath = files[0].Path.LocalPath;
                Debug.WriteLine($"[ModelSettingsVM] LoadPotTheme: Выбран файл: {filePath}");

                // --- Используем внутренний метод для загрузки и применения ---
                await ApplyThemeInternal(themeUri: null, targetName: "Pot", filePath: filePath); // Передаем путь к файлу

                // Если стиль применился успешно (ApplyStyleRequested был вызван),
                // то активируем RadioButton для кастомной темы.
                // Делаем это ПОСЛЕ await ApplyThemeInternal
                if (ApplyStyleRequested != null) // Проверяем, что есть подписчики (значит, стиль был отправлен)
                {
                     // Устанавливаем флаг кастомной темы асинхронно, чтобы UI успел обновиться
                     //await Task.Delay(1); // Небольшая задержка не нужна, т.к. IsCustomPotTheme вызовет OnChanged
                     _isCustomPotTheme = true; // Устанавливаем флаг (но без вызова OnChanged, т.к. нет ObservableProperty)
                     OnPropertyChanged(nameof(IsCustomPotTheme)); // Уведомляем вручную
                     // Сбрасываем другие флаги, чтобы только кастомная была выбрана
                      _isMainPotTheme = false; OnPropertyChanged(nameof(IsMainPotTheme));
                      _isAltPotTheme = false; OnPropertyChanged(nameof(IsAltPotTheme));
                      Debug.WriteLine("[ModelSettingsVM] LoadPotTheme: Флаг IsCustomPotTheme установлен в true.");
                }
            }
            else
            {
                Debug.WriteLine("[ModelSettingsVM] LoadPotTheme: Выбор файла отменен или не удалось получить путь.");
            }
        }

        // Заглушки для других команд
        [RelayCommand] private void LoadStoveModel() { Debug.WriteLine("[ModelSettingsVM] LoadStoveModel: Команда вызвана (заглушка)."); }
        [RelayCommand] private void LoadBubbleModel() { Debug.WriteLine("[ModelSettingsVM] LoadBubbleModel: Команда вызвана (заглушка)."); }
    }

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