using System;
using BoilingPot.ViewModels.SettingsViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
// Для NavigationViewItem

// Для Action

namespace BoilingPot.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        // --- Экземпляры ViewModel для каждой секции ---
        private readonly IServiceProvider _serviceProvider;
        public GeneralSettingsViewModel GeneralSettings { get; }
        public ThemeSettingsViewModel ThemeSettings { get; }
        public ModelSettingsViewModel ModelSettings { get; }

        
        [ObservableProperty] private bool _isShowingSettings = false;
        
        [ObservableProperty] private object? _selectedNavItem; 

        [ObservableProperty] private ViewModelBase _currentSettingSectionViewModel; 

        public SettingsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            GeneralSettings = _serviceProvider.GetRequiredService<GeneralSettingsViewModel>();
            ThemeSettings = _serviceProvider.GetRequiredService<ThemeSettingsViewModel>();
            ModelSettings = _serviceProvider.GetRequiredService<ModelSettingsViewModel>();

            // Устанавливаем начальную секцию
            _currentSettingSectionViewModel = GeneralSettings; // Начинаем с "Общие"
            // _selectedNavItem = ???; // Установить начальный выбранный элемент NavigationView сложно из ViewModel
                                    // Проще задать SelectedItem в XAML при первом отображении, если нужно
        }

        partial void OnSelectedNavItemChanged(object? value)
        {
            if (value is NavigationViewItem selectedItem && selectedItem.Tag is string tag)
            {
                switch (tag)
                {
                    case "General":
                        CurrentSettingSectionViewModel = GeneralSettings;
                        break;
                    case "Themes":
                        CurrentSettingSectionViewModel = ThemeSettings;
                        break;
                    case "Models":
                        CurrentSettingSectionViewModel = ModelSettings;
                        break;
                    default:
                        CurrentSettingSectionViewModel = GeneralSettings; // По умолчанию
                        break;
                }
            }
        }
        
        [RelayCommand] private void ShowSettings() { IsShowingSettings = !IsShowingSettings; }
    }
}