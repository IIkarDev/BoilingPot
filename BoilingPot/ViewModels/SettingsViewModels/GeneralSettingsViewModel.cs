using CommunityToolkit.Mvvm.ComponentModel;

namespace BoilingPot.ViewModels.SettingsViewModels
{
    public partial class GeneralSettingsViewModel : ViewModelBase
    {
        // --- Свойства для секции "Общие" ---
        public bool IsLoadFeatureEnabled = true;
        public bool IsSaveFeatureEnabled = true;

        // Пример: Язык
        [ObservableProperty]
        private string? _selectedLanguage; // Нужны будут и VolumeOptions

        // Пример: Отображать кнопку панели данных
        [ObservableProperty]
        private bool _showDataPanelButton;

        // Пример: Положение кнопки панели данных
        [ObservableProperty]
        private string? _dataPanelButtonPosition; // Нужны будут и VolumeOptions

        // Пример: Разместить панель данных слева
        [ObservableProperty]
        private bool _isDataPanelOnLeft;

        // Примеры: Свитчеры отображения для NavigationView
        [ObservableProperty]
        private bool _showHomeNavItem = true;
        [ObservableProperty]
        private bool _showLoadNavItem = true;
        [ObservableProperty]
        private bool _showSaveNavItem = true;
        [ObservableProperty]
        private bool _showAboutNavItem = true;
        [ObservableProperty]
        private bool _showSettingsNavItem = true;
        [ObservableProperty]
        private bool _showExitNavItem = true;
        
        

        public GeneralSettingsViewModel()
        {
            // TODO: Загрузить начальные значения настроек
            _selectedLanguage = "Русский"; // Пример
            _showDataPanelButton = true;
            _dataPanelButtonPosition = "Верхний правый угол"; // Пример
        }

        public string[] LanguageOptions { get; } = { "Русский", "English" }; // Пример
        public string[] PositionOptions { get; } = { "Верхний правый угол", "Нижний правый угол", "Верхний левый угол", "Нижний левый угол" }; // Пример
    }
}