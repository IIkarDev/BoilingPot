using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reactive; 
using Avalonia.Controls;
using BoilingPot.Services;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Threading;
using BoilingPot.ViewModels.Components;
using BoilingPot.ViewModels.SettingsViewModels;
using FluentAvalonia.UI.Controls;

namespace BoilingPot.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThemeLoaderService _themeLoader;
        private readonly ModelSettingsViewModel  _modelSettingsViewModel;

        /// <summary>
        /// DATAPANEL СВОЙСТВА
        /// </summary>
        
        [Reactive] public bool IsSimulationPausedBySpeed { get; private set; } // Пауза из-за нулевой скорости

        [Reactive] public bool IsHeatingActive { get; private set; }
        
        private DispatcherTimer? _simulationTimer;
        private DateTime _lastTickRealTime; // Реальное время последнего успешного тика
        private TimeSpan _accumulatedSimulatedTime = TimeSpan.Zero;
        
        private readonly TimeSpan _baseTimerInterval = TimeSpan.FromMilliseconds(100); // Например, 10 раз в секунду реального времени

        // --- Существующие свойства для DataPanel (предполагаемые) ---
        // [Reactive] public double PowerRating { get; set; } = 2000; // Мощность, Вт
        [Reactive] public double PowerRating { get; set; } = 0;
        private readonly Dictionary<int, double> _powerLevels = new Dictionary<int, double>
        {
            {0, 0},      // Уровень 0 (Выключено) - 0 Вт
            {1, 800},    // Уровень 1 - 800 Вт
            {2, 1300},   // Уровень 2 - 1300 Вт
            {3, 1900},   // Уровень 3 - 1900 Вт
            {4, 2700},   // Уровень 4 - 2700 Вт
            {5, 3500}    // Уровень 5 - 3500 Вт
        };
      [Reactive] public double SpecificHeatCapacity { get; set; } = 0; // Уд. теплоемкость воды, Дж/(кг·°C)
      [Reactive] public TimeSpan ElapsedTime { get; set; } // Время нагрева

      // --- НОВЫЕ СВОЙСТВА для DataPanel ---
      [Reactive] public double InitialTemperature { get; set; } = 20.0; // Начальная температура, °C
      [Reactive] public double CurrentAverageTemperature { get; set; } // Текущая средняя температура, °C
      [Reactive] public double LiquidVolume { get; set; } // Объем жидкости из настроек, м³ (нужно будет конвертировать из литров)
      [Reactive] public double LiquidDensity { get; set; } = 0; // Плотность воды, кг/м³ (будет меняться от типа жидкости)


      // Вычисляемое свойство для массы
      [Reactive] public double LiquidMass { get; set; } = 0;
      // return LiquidVolume * LiquidDensity; 

      // Вычисляемое свойство для переданной теплоты
      [Reactive] public double HeatTransferred { get; set; } = 0;
          // SpecificHeatCapacity * LiquidMass * (CurrentAverageTemperature - InitialTemperature);
      
        [Reactive] public int FlameLevel { get; set; }
        [Reactive] public double ProcessSpeed { get; set; } = 1.0d;
        [Reactive] public string? SelectedVolume { get; set; }
        public string[] VolumeOptions { get; } =
        {
            "1.0 литр", "1.5 литра", "2.5 литра", "3.5 литра", "5.0 литров", "6.0 литров", "10.0 литров"
        };
        public string[] LiquidTypes { get; } =
        {
            "Вода", "Спирт", "Масло (раст.)", "Парафин", "Керосин", "Ртуть (жид.)"
        };
        [Reactive] public string? SelectedLiquidType { get; set; }
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

        public MainViewModel(IServiceProvider serviceProvider, IThemeLoaderService themeLoader, SettingsViewModel settingsViewModel, ModelSettingsViewModel modelSettingsViewModel)
        {
            _modelSettingsViewModel = modelSettingsViewModel;
            _serviceProvider = serviceProvider;
            _themeLoader = themeLoader;
            SettingsVM = settingsViewModel; // Получаем SettingsVM из DI

            Debug.WriteLine(">>> MainViewModel СОЗДАН (RxUI)");
            _isHomeViewSelectedHelper = this.WhenAnyValue(x => x.CurrentViewModel)
                                            .Select(vm => vm is HomeViewModel) // true, если текущий VM - HomeViewModel
                                            .ToProperty(this, x => x.IsHomeViewSelected); // Преобразуем в свойство
            
            IsCommonViewSelected = true;

            this.WhenAnyValue(x => x.IsCommonViewSelected)
                .Subscribe(isCommonViewSelected =>
                {
                    Debug.WriteLine($">>> IsCommonViewSelected = {isCommonViewSelected}");
                    if (isCommonViewSelected) ExecuteGoToCommon();
                    else ExecuteGoToMolecular();
                });

            
            // --- НОВОЕ: Подписка на изменение ProcessSpeed (для паузы/возобновления) ---
            this.WhenAnyValue(x => x.ProcessSpeed)
                .Subscribe(newSpeed =>
                {
                    Debug.WriteLine($"[MainViewModel] ProcessSpeed изменен на: {newSpeed}x");
                    if (IsHeatingActive) // Реагируем на скорость, только если нагрев в принципе активен
                    {
                        if (newSpeed <= 0 && (_simulationTimer?.IsEnabled ?? false))
                        {
                            // Скорость стала 0 или меньше, ставим симуляцию на ПАУЗУ
                            PauseSimulationBySpeed();
                        }
                        else if (newSpeed > 0 && IsSimulationPausedBySpeed)
                        {
                            // Скорость стала > 0, возобновляем симуляцию с ПАУЗЫ
                            ResumeSimulationFromSpeedPause();
                        }
                        // Если newSpeed > 0 и не было паузы по скорости, то скорость просто
                        // будет учтена в следующем SimulationTimer_Tick.
                    }
                });


            this.WhenAnyValue(x => x.FlameLevel)
                .Subscribe(level =>
                {
                    PowerRating = _powerLevels[FlameLevel];
                    // --- Логика управления таймером ---
                    if (level > 0 && !IsHeatingActive && HeatTransferred == 0)
                    {
                        // Нагрев ВКЛЮЧАЕТСЯ (был 0, стал > 0)
                        StartHeating();
                    }
                    else if (level == 0)
                    {
                        // Нагрев ВЫКЛЮЧАЕТСЯ (был > 0, стал 0)
                        IsHeatingActive = false;
                    }
                    else if (level > 0 && IsHeatingActive)
                    {
                        // Уровень нагрева ИЗМЕНИЛСЯ, но он все еще включен
                        Debug.WriteLine($"[MainViewModel] Уровень нагрева изменен на {level}, таймер продолжает работать.");
                        // Здесь можно изменить интервал таймера или другие параметры симуляции,
                        // если они зависят от FlameLevel не только через CurrentPowerRating
                    }
                });

            this.WhenAnyValue(x => x.HeatTransferred)
                .Subscribe(heatTransferred =>
                {
                    if (FlameLevel == 0 && heatTransferred == 0)
                    {
                        StopHeating();
                        CurrentAverageTemperature = InitialTemperature;
                    }
                });
            
            // Инициализируем начальное состояние
            ElapsedTime = TimeSpan.Zero;
            IsHeatingActive = false;
            IsSimulationPausedBySpeed = false;


            CoolDownCommand = ReactiveCommand.Create(() => { HeatTransferred = 0; });

            // --- Инициализация Команд ---
            GoToHomeCommand = ReactiveCommand.Create(ExecuteGoToHome);
            GoToCommonCommand = ReactiveCommand.Create(ExecuteGoToCommon);
            GoToMolecularCommand = ReactiveCommand.Create(ExecuteGoToMolecular);
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
            
            this.WhenAnyValue(x => x.SelectedVolume)
                .Skip(1)
                .Subscribe(selectedVolume =>
                {
                    if (CurrentViewModel is CommonViewModel commonVm && selectedVolume != null)
                    {
                        _ = commonVm.UpdatePotVolumeText(selectedVolume);
                        UpdateLiquidParameters(selectedVolume, SelectedLiquidType);
                        _ = commonVm.ModelVm.ApplySelectedThemeAsync(commonVm.ModelVm.SelectedPotThemeKey, "Pot");
                        Debug.WriteLine($"[MainViewModel] SelectedKEEEEEEEY = {commonVm.ModelVm.SelectedPotThemeKey}");
                    }
                });
            
            this.WhenAnyValue(x => x.SelectedLiquidType)
                .Skip(1)
                .Subscribe(selectedLiquidType =>
                {
                    if (CurrentViewModel is CommonViewModel commonVm && selectedLiquidType != null)
                    {
                        _ = commonVm.UpdateLiquidType(SelectedLiquidType!);
                        UpdateLiquidParameters(SelectedVolume, selectedLiquidType);
                        _ = commonVm.ModelVm.ApplySelectedThemeAsync(commonVm.ModelVm.SelectedPotThemeKey, "Pot");
                        Debug.WriteLine($"[MainViewModel] SelectedKEEEEEEEY = {commonVm.ModelVm.SelectedPotThemeKey}");
                    }
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
                        case "Data": ExecuteShowDataPanel(); break;
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

        private void ExecuteShowDataPanelButton()
        {
            SettingsVM.GeneralSettings.ShowDataPanelButton = !SettingsVM.GeneralSettings.ShowDataPanelButton;
        }

        private void ExecuteExitApplication()
        {
             if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
             { desktop.Shutdown(); }
        }

        
        // Метод для обновления параметров жидкости при выборе из ComboBox
        private void UpdateLiquidParameters(string? selectedVolume, string? selectedLiquidType)
        {
            // --- Обновление объема и конвертация в м³ ---
            if (selectedVolume != null && double.TryParse(selectedVolume.Split(' ')[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double volValueLiters))
            {
                LiquidVolume = volValueLiters / 1000.0; // Конвертируем литры в м³
            }
            else
            {
                LiquidVolume = 0.001; // 1 литр по умолчанию
            }
            Debug.WriteLine($"CURRENT VOLUME TRIED TO PARSE {selectedVolume?.Split(' ')[0]}");

            // --- Обновление плотности и уд. теплоемкости в зависимости от типа жидкости ---
            switch (selectedLiquidType)
            {
                case "Вода":
                    LiquidDensity = 998; // кг/м³ при ~20°C
                    SpecificHeatCapacity = 4186; // Дж/(кг·°C)
                    break;
                case "Керосин":
                    LiquidDensity = 800;
                    SpecificHeatCapacity = 2090;
                    break;
                case "Масло (раст.)": // Подсолнечное как пример
                    LiquidDensity = 920;
                    SpecificHeatCapacity = 1900; // Примерно
                    break;
                case "Парафин": // Твердый при комн. темп, но для симуляции нагрева жидкого
                    LiquidDensity = 770; // Жидкий
                    SpecificHeatCapacity = 2100; // Жидкий
                    break;
                case "Спирт": // Этанол
                    LiquidDensity = 789;
                    SpecificHeatCapacity = 2440;
                    break;
                case "Ртуть (жид.)":
                    LiquidDensity = 13534;
                    SpecificHeatCapacity = 139.5;
                    break;
                default:
                    LiquidDensity = 0; // По умолчанию вода
                    SpecificHeatCapacity = 0;
                    break;
            }
            // Обновляем CurrentAverageTemperature, чтобы начать с InitialTemperature
            LiquidMass = LiquidVolume * LiquidDensity; 
            CurrentAverageTemperature = InitialTemperature;
            HeatTransferred = SpecificHeatCapacity * LiquidMass * (CurrentAverageTemperature - InitialTemperature);
            
                // Сбрасываем состояние симуляции при смене параметров
                // Сначала полностью останавливаем нагрев
            bool wasHeating = IsHeatingActive;
            StopHeating(); // Это обнулит IsHeatingActive и IsSimulationPausedBySpeed

            _accumulatedSimulatedTime = TimeSpan.Zero;
            ElapsedTime = _accumulatedSimulatedTime;
            // CurrentAverageTemperature = InitialTemperature; // Сброс температуры

            // Если нагрев был активен до смены параметров, пытаемся его возобновить
            if (wasHeating && FlameLevel > 0)
            {
                Debug.WriteLine("[MainViewModel] UpdateLiquidParameters: Возобновление нагрева после смены параметров.");
                StartHeating();
            }
        }
        

    // Метод для начала процесса нагрева (когда FlameLevel > 0)
    private void StartHeating()
    {
        Debug.WriteLine("[MainViewModel] StartHeating: Начало процесса нагрева.");
        IsHeatingActive = true;
        _accumulatedSimulatedTime = TimeSpan.Zero; // Сбрасываем симулированное время
        ElapsedTime = _accumulatedSimulatedTime;   // Обновляем UI
        _lastTickRealTime = DateTime.Now;          // Запоминаем время старта/возобновления

        // Если ProcessSpeed > 0, сразу запускаем таймер
        if (ProcessSpeed > 0)
        {
            IsSimulationPausedBySpeed = false;
            EnsureTimerStarted();
        }
        else // Если ProcessSpeed == 0, то нагрев активен, но симуляция на паузе по скорости
        {
            IsSimulationPausedBySpeed = true;
            EnsureTimerStopped(); // Убедимся, что таймер не тикает зря
            Debug.WriteLine("[MainViewModel] StartHeating: Нагрев активен, но ProcessSpeed = 0. Симуляция на паузе.");
        }
    }

    // Метод для полной остановки процесса нагрева (когда FlameLevel = 0)
    private void StopHeating()
    {
        Debug.WriteLine("[MainViewModel] StopHeating: Остановка процесса нагрева.");
        EnsureTimerStopped();
        IsHeatingActive = false;
        IsSimulationPausedBySpeed = false; // Сбрасываем флаг паузы по скорости
        Debug.WriteLine($"[MainViewModel] StopHeating: Нагрев остановлен. Итоговое ElapsedTime = {ElapsedTime}");
        // Можно сбросить ElapsedTime тут, если нужно:
        // _accumulatedSimulatedTime = TimeSpan.Zero;
        // ElapsedTime = _accumulatedSimulatedTime;
    }

    // Метод для постановки симуляции на паузу из-за нулевой скорости
    private void PauseSimulationBySpeed()
    {
        Debug.WriteLine("[MainViewModel] PauseSimulationBySpeed: Симуляция ставится на паузу (ProcessSpeed <= 0).");
        EnsureTimerStopped(); // Останавливаем тики таймера
        IsSimulationPausedBySpeed = true;
    }

    // Метод для возобновления симуляции после паузы из-за скорости
    private void ResumeSimulationFromSpeedPause()
    {
        Debug.WriteLine("[MainViewModel] ResumeSimulationFromSpeedPause: Возобновление симуляции (ProcessSpeed > 0).");
        if (IsHeatingActive) // Возобновляем, только если нагрев все еще должен быть активен
        {
            IsSimulationPausedBySpeed = false;
            _lastTickRealTime = DateTime.Now; // Обновляем время последнего тика, чтобы не было скачка
            EnsureTimerStarted();
        }
    }

    // Вспомогательные методы для управления таймером
    private void EnsureTimerStarted()
    {
        if (_simulationTimer == null || !_simulationTimer.IsEnabled)
        {
            _simulationTimer = new DispatcherTimer { Interval = _baseTimerInterval };
            _simulationTimer.Tick -= SimulationTimer_Tick; // Отписка на всякий случай
            _simulationTimer.Tick += SimulationTimer_Tick;
            _simulationTimer.Start();
            Debug.WriteLine($"[MainViewModel] EnsureTimerStarted: Таймер запущен/создан с интервалом {_baseTimerInterval.TotalMilliseconds} мс.");
        }
    }

    private void EnsureTimerStopped()
    {
        if (_simulationTimer != null && _simulationTimer.IsEnabled)
        {
            _simulationTimer.Stop();
            Debug.WriteLine("[MainViewModel] EnsureTimerStopped: Таймер остановлен.");
        }
        // _simulationTimer = null; // Можно не обнулять, чтобы переиспользовать
    }


    // Обработчик тика таймера
    private void SimulationTimer_Tick(object? sender, EventArgs e)
    {
        // Таймер тикает, только если IsHeatingActive И ProcessSpeed > 0
        if (!IsHeatingActive || IsSimulationPausedBySpeed || ProcessSpeed <= 0)
        {
             // Эта ситуация не должна возникать, если таймер правильно останавливается, но на всякий случай.
             // if(IsHeatingActive && (_simulationTimer?.IsEnabled ?? false)) EnsureTimerStopped();
             // return;
        }

        DateTime currentRealTime = DateTime.Now;
        // Реальное время, прошедшее с последнего тика (или с момента _lastTickRealTime)
        TimeSpan realTimeDelta = currentRealTime - _lastTickRealTime;
        _lastTickRealTime = currentRealTime; // Обновляем время последнего тика

        // Симулированное время, прошедшее за этот реальный интервал
        double simulatedMillisecondsThisTick = realTimeDelta.TotalMilliseconds * ProcessSpeed;
        _accumulatedSimulatedTime += TimeSpan.FromMilliseconds(simulatedMillisecondsThisTick);
        ElapsedTime = _accumulatedSimulatedTime;
        // --- Логика симуляции нагрева ---
        
        if (IsHeatingActive) HeatTransferred += PowerRating * simulatedMillisecondsThisTick / 1000.0;
        else if (HeatTransferred >= 0) HeatTransferred -= 0.07 * simulatedMillisecondsThisTick;
        
        CurrentAverageTemperature = InitialTemperature + HeatTransferred / (LiquidMass * SpecificHeatCapacity);
        // ... (расчет температуры и т.д., используя simulatedMillisecondsThisTick или realTimeDelta * ProcessSpeed) ...
    }
    }
}