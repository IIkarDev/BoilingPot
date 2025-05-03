using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
// Для Application.Current
// Для AvaloniaXamlLoader и ResourceInclude
// Для IResourceHost

namespace BoilingPot.ViewModels.SettingsViewModels
{
    public partial class ThemeSettingsViewModel : ViewModelBase
    {
        
        [ObservableProperty] private string _selectedTheme = "Dark";
        
        partial void OnSelectedThemeChanged(string value)
        {
            ApplyTheme(value); // Применяем выбранную тему
        }

        private void ApplyTheme(string themeName)
        {
            ThemeVariant? newVariant = themeName switch
            {
                "Light" => ThemeVariant.Light,
                "DarkTheme" => ThemeVariant.Dark,
                "SystemTheme" => ThemeVariant.Default, // Default будет использовать системную
                _ => ThemeVariant.Default // По умолчанию - системная
            };

            // Устанавливаем запрошенную тему для всего приложения
            Application.Current.RequestedThemeVariant = newVariant;
        }
        
        [ObservableProperty]
        private AccentPaletteInfo? _selectedAccentPalette;

        public List<AccentPaletteInfo> AccentPalettes { get; } = new List<AccentPaletteInfo>
        {
            new AccentPaletteInfo("Taup (По ум.)", "avares://AvaloniaApplication1/Resources/Palettes/DefaultAccentPalette.axaml"), 
            new AccentPaletteInfo("Purple", "avares://AvaloniaApplication1/Resources/Palettes/PurpleAccentPalette.axaml"), 
            new AccentPaletteInfo("Red", "avares://AvaloniaApplication1/Resources/Palettes/RedAccentPalette.axaml"),
            new AccentPaletteInfo("Blue", "avares://AvaloniaApplication1/Resources/Palettes/BlueAccentPalette.axaml"),
            new AccentPaletteInfo("Teal", "avares://AvaloniaApplication1/Resources/Palettes/TealAccentPalette.axaml"), 
            new AccentPaletteInfo("Orange ", "avares://AvaloniaApplication1/Resources/Palettes/OrangeAccentPalette.axaml"), 
            new AccentPaletteInfo("Gray", "avares://AvaloniaApplication1/Resources/Palettes/GrayAccentPalette.axaml"),
            new AccentPaletteInfo("Green", "avares://AvaloniaApplication1/Resources/Palettes/GreenAccentPalette.axaml"),
        };

        public ThemeSettingsViewModel()
        {
            // Выбираем палитру по умолчанию при запуске
            _selectedAccentPalette = AccentPalettes.FirstOrDefault();
            ApplyAccentPalette(_selectedAccentPalette); // Применяем при запуске, если нужно
        }

        // Вызывается при смене выбранной палитры в ComboBox
        partial void OnSelectedAccentPaletteChanged(AccentPaletteInfo? value)
        {
            ApplyAccentPalette(value);
        }

        // Метод для применения выбранной палитры
        private void ApplyAccentPalette(AccentPaletteInfo? paletteInfo)
        {
            if (paletteInfo == null || Application.Current == null) return;

            try
            {
                // 1. Загружаем словарь ресурсов из выбранного файла палитры
                var newPaletteResources = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(paletteInfo.ResourceUri));

                // 2. Получаем доступ к глобальным ресурсам приложения
                var appResources = Application.Current.Resources;

                // 3. Перебираем КЛЮЧИ, которые мы ожидаем найти в палитре
                //    и перезаписываем их значения в глобальных ресурсах.
                //    Важно, чтобы ключи совпадали!
                string[] accentKeys = {
                    "MainAccentColor", "AltAccentColor", "LightAccentColor", "DarkAccentColor",
                    "ForegroundAccentColor", "BackgroundAccentColor", "PointeroverAccentColor", "PressedAccentColor"
                    // Добавьте сюда ВСЕ ключи, которые вы определили для акцентов
                };

                foreach (var key in accentKeys)
                {
                    if (newPaletteResources.TryGetResource(key, Application.Current.ActualThemeVariant, out var newValue))
                    {
                        // Перезаписываем ресурс в глобальном словаре
                        appResources[key] = newValue;
                         System.Diagnostics.Debug.WriteLine($"Applied resource for key: {key}");
                    }
                    else
                    {
                         System.Diagnostics.Debug.WriteLine($"WARNING: Key '{key}' not found in palette '{paletteInfo.Name}'");
                    }

                    // if (Application.Current.RequestedThemeVariant == ThemeVariant.Dark)
                    // {
                    //     appResources["ThemeAccentColor"] = appResources["DarkAccentColor"];
                    // } 
                    // else if (Application.Current.RequestedThemeVariant == ThemeVariant.Light)
                    // {
                    //     appResources["ThemeAccentColor"] = appResources["LightAccentColor"];
                    // }
                }
                 System.Diagnostics.Debug.WriteLine($"Applied accent palette: {paletteInfo.Name}");
            }
            catch (Exception ex)
            {
                // Обработка ошибок (например, файл палитры не найден или содержит ошибки)
                System.Diagnostics.Debug.WriteLine($"Error applying accent palette '{paletteInfo?.Name}': {ex.Message}");
            }
        }

        // --- Остальные свойства и методы для тем (Light/Dark) и т.д. ---
        // ...
    }

    // Вспомогательный класс для хранения информации о палитре
    public class AccentPaletteInfo
    {
        public string Name { get; }
        public string ResourceUri { get; } // URI в формате avares://

        public AccentPaletteInfo(string name, string resourceUri)
        {
            Name = name;
            ResourceUri = resourceUri;
        }

        // Переопределяем ToString для корректного отображения в ComboBox
        public override string ToString() => Name;
    }
}