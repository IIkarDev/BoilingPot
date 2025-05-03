using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using BoilingPot.ViewModels.SettingsViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BoilingPot.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    public SettingsViewModel SettingsVm { get; }
    private readonly GeneralSettingsViewModel _generalSettings;

    // --- Опционально: раскомментируйте для динамической видимости меню ---
    // [ObservableProperty] private bool _showHomeNavItem = true;
    // [ObservableProperty] private bool _showLoadNavItem = true;
    // [ObservableProperty] private bool _showSaveNavItem = true;
    // [ObservableProperty] private bool _showAboutNavItem = true;
    // [ObservableProperty] private bool _showSettingsNavItem = true;
    // [ObservableProperty] private bool _showExitNavItem = true;
    // --- Конец опционального блока ---

    [ObservableProperty] private object? _selectedNavigationViewItem;
    [ObservableProperty] private bool _isShowingAbout = false;
    [ObservableProperty] private bool _isShowingDataPanel = false;
    [ObservableProperty] private ViewModelBase _currentView;

    [ObservableProperty] private int _flameLevel = 1;
    [ObservableProperty] private int _processSpeed = 1;
    [ObservableProperty] private string? _selectedLiquidType;
    [ObservableProperty] private string? _selectedVolume;
    [ObservableProperty] private string? _volumeText; // Оставляем для ComboBox или прямого ввода

    // Флаги для управления состоянием UI (например, какой вид выбран)
    [ObservableProperty] private bool _isHomeViewSelected;
    [ObservableProperty] private bool _isCommonViewSelected;
    [ObservableProperty] private bool _isMolecularViewSelected;
    // Рекомендуется удалить это свойство и использовать конвертер в XAML
    [ObservableProperty] private bool _notIsHomeViewSelected; 

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        // Получаем зависимости через DI
        SettingsVm = _serviceProvider.GetRequiredService<SettingsViewModel>();
        // Устанавливаем начальное представление
        CurrentView = _serviceProvider.GetRequiredService<HomeViewModel>();
        IsHomeViewSelected = true;
        NotIsHomeViewSelected = !IsHomeViewSelected; // Или удалите эту строку
    }

    [RelayCommand]
    private void NavigateToHomeView()
    {
        // Получаем HomeViewModel из DI контейнера
        CurrentView = _serviceProvider.GetRequiredService<HomeViewModel>();
        IsHomeViewSelected = true;
        IsCommonViewSelected = false;
        IsMolecularViewSelected = false;
        NotIsHomeViewSelected = !IsHomeViewSelected; // Обновляем (или удаляем)
        
        // Возможно, потребуется передать какие-то данные в HomeViewModel?
        // if (CurrentView is HomeViewModel homeVm) { /* ... */ }
    }

    // Обновленный обработчик изменения SelectedVolume
    partial void OnSelectedVolumeChanged(string? value)
    {
        // Обновляем и текстовое поле, если оно используется отдельно
        VolumeText = value; 
        UpdatePotVolumeInCommonViewModel(value);
    }

    // Обновленный обработчик изменения VolumeText (если текст можно менять напрямую)
    partial void OnVolumeTextChanged(string? value)
    {
         UpdatePotVolumeInCommonViewModel(value);
    }
    
    // При навигации к CommonView, также обновим объем, если он уже выбран
    [RelayCommand]
    private void NavigateToCommonView()
    {
        var commonVm = _serviceProvider.GetRequiredService<CommonViewModel>();
        // Передаем текущее значение объема ПЕРЕД установкой CurrentView
        commonVm.UpdatePotVolume(VolumeText); 
        CurrentView = commonVm;

        IsHomeViewSelected = false;
        IsCommonViewSelected = true;
        IsMolecularViewSelected = false;
        NotIsHomeViewSelected = !IsHomeViewSelected; 
    }

    [RelayCommand]
    private void NavigateToMolecularView()
    {
        // Получаем MolecularViewModel из DI контейнера
        CurrentView = _serviceProvider.GetRequiredService<MolecularViewModel>();
        IsHomeViewSelected = false;
        IsCommonViewSelected = false;
        IsMolecularViewSelected = true;
        NotIsHomeViewSelected = !IsHomeViewSelected; // Обновляем (или удаляем)

        // Возможно, потребуется передать какие-то данные в MolecularViewModel?
        // if (CurrentView is MolecularViewModel molVm) { /* ... */ }
    }

    [RelayCommand]
    private void Start()
    {
        // Переходим на CommonView при старте (как было в оригинальном коде)
        NavigateToCommonViewCommand.Execute(null);
    }

    [RelayCommand]
    private void ShowDataPanel()
    {
        IsShowingDataPanel = !IsShowingDataPanel;
    }

    [RelayCommand]
    private void ShowAbout()
    {
        IsShowingAbout = !IsShowingAbout;
    }

    partial void OnSelectedNavigationViewItemChanged(object? value)
    {
        if (value is NavigationViewItem selectedItem && selectedItem.Tag is string tag)
        {
            switch (tag)
            {
                case "Home":
                    NavigateToHomeViewCommand.Execute(null); // Убедитесь, что команды вызываются корректно
                    break;
                case "Load":
                    LoadFileCommand.Execute(null);
                    break;
                case "Save":
                    // Вероятно, здесь должна быть команда Save, а не LoadFile?
                    SaveSettingsCommand.Execute(null); // Исправлено на SaveSettingsCommand
                    break;
                case "Settings":
                    SettingsVm.ShowSettingsCommand.Execute(null);
                    break;
                case "Exit":
                    ExitApplicationCommand.Execute(null);
                    break;
                case "About":
                    ShowAboutCommand.Execute(null);
                    break;
            }
        }

        // Сбрасываем выбор элемента навигации после обработки
        Dispatcher.UIThread.Post(
            () => SelectedNavigationViewItem = null, DispatcherPriority.Background);
    }

    // Обработчики изменения Is*Selected теперь в основном для UI-реагирования,
    // основная логика навигации - в командах RelayCommand.
    // Если Handle* методы выполняли ту же навигацию, их можно упростить или убрать,
    // если навигация уже происходит через RelayCommand.
    // Оставил их для совместимости, если они используются где-то еще.
    partial void OnIsCommonViewSelectedChanged(bool value)
    {
        if (IsCommonViewSelected && CurrentView is not CommonViewModel)
        {
            NavigateToCommonViewCommand.Execute(null);
        }
    }

    partial void OnIsMolecularViewSelectedChanged(bool value)
    {
        if (IsMolecularViewSelected && CurrentView is not MolecularViewModel)
        {
            NavigateToMolecularViewCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void ExitApplication()
    {
        if (Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktopLifetime) desktopLifetime.Shutdown();
    }

    [RelayCommand]
    private void LoadFile()
    {
    }

    [RelayCommand]
    private void SaveSettings()
    {
    }

    [RelayCommand]
    private void ProcessSpeedChange() { /* ... */ }

    [RelayCommand]
    private void FlameLevelChange() { /* ... */ }

    [RelayCommand]
    private void CoolDownCommand() { /* ... */ }

    [RelayCommand]
    private void ShowStructureCommand() { /* ... */ }

    // Этот метод теперь корректно работает с любым CurrentView, 
    // если он типа CommonViewModel
    

    // Вспомогательный метод для обновления громкости в CommonViewModel, если он активен
    private void UpdatePotVolumeInCommonViewModel(string? volume)
    {
         // Получаем CommonViewModel через провайдер, чтобы убедиться,
         // что работаем с тем же экземпляром (если он Singleton/Scoped)
         // или чтобы обновить его состояние перед следующим показом (если Transient)
         // Примечание: Если CommonViewModel - Singleton, можно хранить ссылку на него.
         
         // Безопасный способ: Обновляем только если CommonView активен
         if (CurrentView is CommonViewModel commonVm)
         {
             commonVm.UpdatePotVolume(volume);
         }
         // Альтернатива (если CommonViewModel - Singleton):
         // var commonVmInstance = _serviceProvider.GetService<CommonViewModel>();
         // commonVmInstance?.UpdatePotVolume(volume);
    }

    public string[] VolumeOptions { get; } =
    {
        "1.0 литр", "1.5 литра", "2.5 литра", "3.5 литра", "5.0 литров", "6.0 литров", "10.0 литров"
    };
    public string[] LiquidTypes { get; } =
    {
        "1.0 литр", "1.5 литра", "2.5 литра", "3.5 литра", "5.0 литров", "6.0 литров", "10.0 литров"
    };
}