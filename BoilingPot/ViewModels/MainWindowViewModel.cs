using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reactive; 
using Avalonia.Controls;
using BoilingPot.Services;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace BoilingPot.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThemeLoaderService _themeLoader; 

        [Reactive] public int FlameLevel { get; set; }
        [Reactive] public string ProcessSpeed { get; set; }
        public string[] VolumeOptions { get; }
        [Reactive] public object? SelectedVolume { get; set; }
        public string[] LiquidTypes { get; }
        [Reactive] public object? SelectedLiquidType { get; set; }
        [Reactive] public NavigationViewItem SelectedNavItem { get; set; }
        [Reactive] public ViewModelBase CurrentViewModel { get; private set; }
        [Reactive] public Control? DynamicViewContent { get; private set; } 
        [Reactive] public bool IsDynamicViewActive { get; private set; }

        // Флаги видимости панелей
        [Reactive] public bool IsShowingSettings { get; set; }
        [Reactive] public bool IsShowingAbout { get; set; }
        [Reactive] public bool IsShowingDataPanel { get; set; }

        // Ссылка на SettingsViewModel (получаем из DI)
        public SettingsViewModel SettingsVM { get; }

        // --- Команды ---
        public ReactiveCommand<Unit, Unit> CoolDownCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowStructureCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToHomeCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToCommonCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToMolecularCommand { get; }
        
        public ReactiveCommand<Unit, Unit> LoadDynamicThemeCommand { get; } // Команда теперь только в ModelSettingsVM
        public ReactiveCommand<Unit, Unit> LoadFileCommand { get;  }
        public ReactiveCommand<Unit, Unit> ShowSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowDataPanelCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitApplicationCommand { get; }

        // Переключатель состояния Home/NotHome (для IsVisible в MainWindow)
        // Используем ObservableAsPropertyHelper для создания вычисляемого свойства
        private readonly ObservableAsPropertyHelper<bool> _isHomeViewSelectedHelper;
        private readonly ObservableAsPropertyHelper<bool> _isCommonViewSelectedHelper;

        public bool IsHomeViewSelected => _isHomeViewSelectedHelper.Value;
        [Reactive] public bool IsCommonViewSelected { get; set; }

        public MainViewModel(IServiceProvider serviceProvider, IThemeLoaderService themeLoader, SettingsViewModel settingsViewModel)
        {
            _serviceProvider = serviceProvider;
            _themeLoader = themeLoader;
            SettingsVM = settingsViewModel; // Получаем SettingsVM из DI

            Debug.WriteLine(">>> MainViewModel СОЗДАН (RxUI)");

            // --- Вычисляемые свойства для видимости ---
            // Когда меняется CurrentViewModel, пересчитываем IsHomeViewSelected
            _isHomeViewSelectedHelper = this.WhenAnyValue(x => x.CurrentViewModel)
                                            .Select(vm => vm is HomeViewModel) // true, если текущий VM - HomeViewModel
                                            .ToProperty(this, x => x.IsHomeViewSelected); // Преобразуем в свойство
            //
            // _isCommonViewSelectedHelper = this.WhenAnyValue(x => x.CurrentViewModel)
            //     .Select(vm => vm is CommonViewModel) // true, если текущий VM - HomeViewModel
            //     .ToProperty(this, x => x.IsCommonViewSelected);

            IsCommonViewSelected = true;

            this.WhenAnyValue(x => x.IsCommonViewSelected)
                .Subscribe(isCommonViewSelected =>
                {
                    Debug.WriteLine($">>> IsCommonViewSelected = {isCommonViewSelected}");
                    if (isCommonViewSelected) ExecuteGoToCommon();
                    else ExecuteGoToMolecular();
                });

            // --- Инициализация Команд ---
            GoToHomeCommand = ReactiveCommand.Create(ExecuteGoToHome);
            GoToCommonCommand = ReactiveCommand.Create(ExecuteGoToCommon);
            GoToMolecularCommand = ReactiveCommand.Create(ExecuteGoToMolecular);
            // LoadDynamicThemeCommand больше не нужен здесь, он в ModelSettingsViewModel
            ShowSettingsCommand = ReactiveCommand.Create(ExecuteShowSettings);
            ShowAboutCommand = ReactiveCommand.Create(ExecuteShowAbout);
            ShowDataPanelCommand = ReactiveCommand.Create(ExecuteShowDataPanel);
            ExitApplicationCommand = ReactiveCommand.Create(ExecuteExitApplication);

            // --- !!! Обработка Interaction от SettingsViewModel !!! ---
            SettingsVM.CloseSettingsInteraction.RegisterHandler(interaction =>
            {
                 Debug.WriteLine("[MainViewModel] Получен запрос на закрытие настроек от SettingsVM.");
                 IsShowingSettings = false; // Скрываем панель
                 SettingsVM.IsShowingSettings = IsShowingSettings;
                 interaction.SetOutput(Unit.Default); // Сообщаем, что запрос обработан
            });
            
            this.WhenAnyValue(x => x.SelectedNavItem)
                .Where(item => item is NavigationViewItem) // Убедимся, что это нужный тип
                .Cast<NavigationViewItem>() // Приводим тип
                .Where(item => item.Tag is string) // Убедимся, что Tag - строка
                .Select(item => item.Tag as string) // Берем Tag
                .Subscribe(tag => // Выполняем действие при получении нового Tag
                {
                    Debug.WriteLine($"[SettingsViewModel] Выбран NavItem с Tag: {tag}");
                    switch (tag)
                    {
                        case "Home": ExecuteGoToHome(); break;
                        case "Load": break;
                        case "Save": break;
                        case "Settings": ExecuteShowSettings(); break;
                        case "About": ExecuteShowAbout(); break;
                        case "Exit": ExecuteExitApplication(); break;
                    }
                    Dispatcher.UIThread.Post(
                        () => SelectedNavItem = null, DispatcherPriority.Background);
                });

            // Устанавливаем начальный вид
            ExecuteGoToHome();
        }
        
        // --- Реализация Методов Команд ---
    
        private void ExecuteGoToHome()
        {
            DynamicViewContent = null; IsDynamicViewActive = false;
            IsCommonViewSelected = false;
            CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
            Debug.WriteLine(">>> Переход на Home");
        }
        private void ExecuteGoToCommon()
        {
            DynamicViewContent = null; IsDynamicViewActive = false;
            IsCommonViewSelected = true;
            CurrentViewModel = _serviceProvider.GetRequiredService<CommonViewModel>();
            Debug.WriteLine(">>> Переход на Common");
        }
        private void ExecuteGoToMolecular()
        {
            IsCommonViewSelected = false;
            DynamicViewContent = null; IsDynamicViewActive = false;
            CurrentViewModel = _serviceProvider.GetRequiredService<MolecularViewModel>();
            Debug.WriteLine(">>> Переход на Molecular");
        }
        private void ExecuteShowSettings() { SettingsVM.IsShowingSettings = !SettingsVM.IsShowingSettings; }
        private void ExecuteShowAbout() { IsShowingAbout = !IsShowingAbout; }
        private void ExecuteShowDataPanel() { IsShowingDataPanel = !IsShowingDataPanel; }

        private void ExecuteExitApplication()
        {
             if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
             { desktop.Shutdown(); }
        }

        
        
        // Метод загрузки динамической темы теперь не нужен здесь
        // private async Task ExecuteLoadDynamicViewAsync() { ... }
    }
}