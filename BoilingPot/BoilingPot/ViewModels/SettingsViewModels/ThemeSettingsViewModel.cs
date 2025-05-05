// ViewModels/SettingsViewModels/ThemeSettingsViewModel.cs

using Avalonia; // Для Application.Current
using Avalonia.Markup.Xaml; // Для AvaloniaXamlLoader.Load
using Avalonia.Styling; // Для ThemeVariant, IStyle, ResourceDictionary, Styles
using ReactiveUI; // Базовый класс RxUI
using ReactiveUI.Fody.Helpers; // Для [Reactive]
using System;
using System.Collections.Generic;
using System.Diagnostics; // Для Debug
using System.Linq; // Для Linq
using System.Reactive; // Для Unit
using System.Reactive.Linq;
using Avalonia.Controls; // Для WhenAnyValue, SelectMany, Subscribe

// Пространство имен для ViewModel секций настроек
namespace BoilingPot.ViewModels.SettingsViewModels
{
    // ViewModel для секции "Настройки Темы".
    // Управляет выбором темы приложения и акцентной палитры.
    public partial class ThemeSettingsViewModel : ViewModelBase
    {
        // --- Свойства для настроек темы ---

        // Выбранная тема (светлая/темная/системная).
        // Значения: "Light", "Dark", "System" (или "Default")
        [Reactive] public string SelectedThemeKey { get; set; } = "System"; // Начальное значение

        // Выбранная акцентная палитра (объект AccentPaletteInfo).
        [Reactive] public AccentPaletteInfo? SelectedAccentPalette { get; set; }

        // Список доступных акцентных палитр.
        public List<AccentPaletteInfo> AccentPalettes { get; } // Инициализируется в конструкторе

        // --- Команды (если нужны) ---
        // public ReactiveCommand<Unit, Unit> ApplyThemeSettingsCommand { get; }

        // --- Конструктор ---
        public ThemeSettingsViewModel()
        {
             Debug.WriteLine("[ThemeSettingsVM] Конструктор RxUI: Начало");

            // Инициализация списка акцентных палитр
            AccentPalettes = new List<AccentPaletteInfo>
            {
                // Пути к файлам палитр в ресурсах приложения (avares://Сборка/Путь/Файл.axaml)
                new AccentPaletteInfo("Taup (По ум.)", "avares://BoilingPot/Resources/Palettes/DefaultAccentPalette.axaml"),
                new AccentPaletteInfo("Purple", "avares://BoilingPot/Resources/Palettes/PurpleAccentPalette.axaml"),
                new AccentPaletteInfo("Red", "avares://BoilingPot/Resources/Palettes/RedAccentPalette.axaml"),
                new AccentPaletteInfo("Blue", "avares://BoilingPot/Resources/Palettes/BlueAccentPalette.axaml"),
                new AccentPaletteInfo("Teal", "avares://BoilingPot/Resources/Palettes/TealAccentPalette.axaml"),
                new AccentPaletteInfo("Orange", "avares://BoilingPot/Resources/Palettes/OrangeAccentPalette.axaml"),
                new AccentPaletteInfo("Gray", "avares://BoilingPot/Resources/Palettes/GrayAccentPalette.axaml"),
                new AccentPaletteInfo("Green", "avares://BoilingPot/Resources/Palettes/GreenAccentPalette.axaml"),
            };

            // Устанавливаем начальную выбранную палитру (первую в списке)
            SelectedAccentPalette = AccentPalettes.FirstOrDefault();
            Debug.WriteLine($"[ThemeSettingsVM] Конструктор RxUI: Выбрана начальная палитра: {SelectedAccentPalette?.Name ?? "null"}");


            // --- Реакция на изменение свойств ---

            // Подписываемся на изменение ключа темы (Light/Dark/System)
            this.WhenAnyValue(x => x.SelectedThemeKey)
                 // Skip(1) пропускает начальное значение, если не хотим применять тему сразу при создании VM
                 // .Skip(1)
                 .Subscribe(key => ApplyTheme(key)); // Применяем тему при изменении

            // Подписываемся на изменение выбранной акцентной палитры
            this.WhenAnyValue(x => x.SelectedAccentPalette)
                 // Skip(1) пропускает начальное значение
                 .Skip(1)
                 .Subscribe(palette => ApplyAccentPalette(palette)); // Применяем палитру при изменении

             // TODO: Загрузить текущие настройки темы и палитры при инициализации

             Debug.WriteLine("[ThemeSettingsVM] Конструктор RxUI: Завершение");
        }

        // --- Методы для применения темы и палитры ---

        // Метод для применения выбранной темы (Light/Dark/System)
        private void ApplyTheme(string themeKey)
        {
            Debug.WriteLine($"[ThemeSettingsVM] ApplyTheme: Применение темы с ключом: {themeKey}");
            ThemeVariant? newVariant = themeKey switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                "System" => ThemeVariant.Default, // Default будет использовать системную
                _ => ThemeVariant.Default // По умолчанию - системная
            };

            // Проверяем, что приложение существует и устанавливаем запрошенную тему
            if (Application.Current != null)
            {
                 Application.Current.RequestedThemeVariant = newVariant;
                 Debug.WriteLine($"[ThemeSettingsVM] ApplyTheme: Установлена RequestedThemeVariant: {newVariant}");
            }
            else
            {
                 Debug.WriteLine("!!! ThemeSettingsVM: ApplyTheme: Application.Current равен null. Не удалось установить тему.");
            }
        }

        // Метод для применения выбранной акцентной палитры из ResourceDictionary (.axaml)
        private void ApplyAccentPalette(AccentPaletteInfo? paletteInfo)
        {
            if (paletteInfo == null || Application.Current == null)
            {
                 Debug.WriteLine("[ThemeSettingsVM] ApplyAccentPalette: paletteInfo или Application.Current равен null. Палитра не применена.");
                 return;
            }

            Debug.WriteLine($"[ThemeSettingsVM] ApplyAccentPalette: Применение палитры '{paletteInfo.Name}' из '{paletteInfo.ResourceUri}'");

            try
            {
                // 1. Загружаем словарь ресурсов (ResourceDictionary) из файла .axaml.
                // AvaloniaXamlLoader.Load(Uri) умеет загружать ResourceDictionary из avares://
                var newPaletteResources = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(paletteInfo.ResourceUri));
                 Debug.WriteLine($"[ThemeSettingsVM] ApplyAccentPalette: ResourceDictionary из файла загружен.");

                // 2. Получаем доступ к глобальным ресурсам приложения.
                var appResources = Application.Current.Resources;

                // 3. Перебираем КЛЮЧИ, которые мы ожидаем найти в файле палитры,
                //    и перезаписываем их значения в глобальных ресурсах приложения.
                //    Это позволяет всем DynamicResource, которые используют эти ключи, обновиться.
                string[] accentKeys = {
                    "MainAccentColor", "AltAccentColor", "DarkAccentColor", "LightAccentColor",
                    "ForegroundAccentColor", "BackgroundAccentColor", "PointeroverAccentColor", "PressedButtonAccentColor"
                };

                int appliedCount = 0;
                foreach (var key in accentKeys)
                {
                    // Пытаемся получить ресурс по ключу из загруженного словаря.
                    // ActualThemeVariant нужен для ThemeDictionaries внутри палитры, если они есть.
                    if (newPaletteResources.TryGetResource(key, Application.Current.ActualThemeVariant, out var newValue))
                    {
                        // Перезаписываем ресурс в глобальном словаре приложения.
                        // Важно: Если ресурс уже существует, он будет заменен.
                        appResources[key] = newValue;
                        appliedCount++;
                         Debug.WriteLine($"[ThemeSettingsVM] ApplyAccentPalette: Ресурс '{key}' применен.");
                    }
                    else
                    {
                         // Если ресурс не найден в палитре, возможно, это предупреждение или ошибка.
                         Debug.WriteLine($"[ThemeSettingsVM] ApplyAccentPalette: ПРЕДУПРЕЖДЕНИЕ: Ресурс '{key}' не найден в палитре '{paletteInfo.Name}'");
                    }
                }

                if (appliedCount > 0)
                {
                    Debug.WriteLine($"[ThemeSettingsVM] ApplyAccentPalette: Успешно применена палитра: {paletteInfo.Name}. Обновлено {appliedCount} ресурсов.");
                }
                else
                {
                    Debug.WriteLine($"!!! ThemeSettingsVM: ApplyAccentPalette: Не удалось применить палитру {paletteInfo.Name}. Ни один ожидаемый ресурс не найден.");
                }

            }
            catch (Exception ex)
            {
                // Обработка ошибок (например, файл палитры не найден или содержит ошибки XAML)
                Debug.WriteLine($"!!! ThemeSettingsVM: ApplyAccentPalette: Ошибка при применении палитры '{paletteInfo?.Name}': {ex}");
            }
        }

        // --- Вспомогательный класс для хранения информации о палитре ---
        // Это не ViewModel, а простая модель данных для ComboBox.
        public class AccentPaletteInfo
        {
            public string Name { get; } // Имя для отображения в ComboBox
            public string ResourceUri { get; } // URI файла палитры (.axaml)

            public AccentPaletteInfo(string name, string resourceUri)
            {
                Name = name;
                ResourceUri = resourceUri;
            }

            // Переопределяем ToString() для корректного отображения в ComboBox.
            public override string ToString() => Name;
        }
    }
}