// Models/AppSettings.cs
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls; // Для SplitViewPanePlacement, Symbol
using System.Collections.Generic;

namespace BoilingPot.Models
{
    public class AppSettings
    {
        // --- Общие Настройки (из GeneralSettingsViewModel) ---
        public string SelectedLanguage { get; set; } = "Русский";
        public bool ShowDataPanelButton { get; set; } = true;
        public string SelectedDataPanelButtonPosition { get; set; } = "Верхний правый угол";
        // Для PanePlacement и Symbol лучше хранить строки или enum, которые потом конвертируются
        public string DataPanePlacementString { get; set; } = "Right"; // "Left" или "Right"
        public string DataPanelButtonSymbolString { get; set; } = "ChevronLeft"; // "ChevronLeft" или "ChevronRight"
        public bool IsDataPanelOnLeft { get; set; } // Старый флаг, можно вычислять из DataPanePlacementString

        public bool ShowHomeNavItem { get; set; } = true;
        public bool ShowLoadNavItem { get; set; } = true;
        public bool ShowSaveNavItem { get; set; } = true;
        public bool ShowSettingsNavItem { get; set; } = true;
        public bool ShowAboutNavItem { get; set; } = true;
        public bool ShowExitNavItem { get; set; } = true;

        // --- Настройки Темы (из ThemeSettingsViewModel) ---
        public string SelectedThemeKey { get; set; } = "System"; // "Light", "Dark", "System"
        public string? SelectedAccentPaletteName { get; set; } // ИМЯ палитры

        // --- Настройки Моделей (из ModelSettingsViewModel) ---
        public string PotThemeKey { get; set; } = "Main";
        public string? CustomPotThemeFilePath { get; set; }
        public string StoveThemeKey { get; set; } = "Main";
        public string? CustomStoveThemeFilePath { get; set; }
        public string BubbleThemeKey { get; set; } = "Main";
        public string? CustomBubbleThemeFilePath { get; set; }

        // --- Параметры Симуляции/ControlPanel (из MainViewModel) ---
        public double ProcessSpeed { get; set; } = 1.0;
        public int FlameLevel { get; set; } = 0;
        public string? SelectedVolume { get; set; } = "1.0 литр";
        public string? SelectedLiquidType { get; set; } = "Вода";

        public AppSettings() { }
    }
}