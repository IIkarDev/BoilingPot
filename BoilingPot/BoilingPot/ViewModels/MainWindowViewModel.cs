using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Threading;
using BoilingPot.Models;
using BoilingPot.Services;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace BoilingPot.ViewModels;

/// <summary>
///     Главный ViewModel приложения.
///     Отвечает за управление основными состояниями UI, навигацию между
///     главными экранами (Home, Common, Molecular) и отображение
///     модальных/всплывающих панелей (Настройки, О программе, Панель данных).
/// </summary>
public class MainViewModel : ViewModelBase
{
    #region Поля и Сервисы

    /// <summary>
    ///     Провайдер сервисов для получения зависимостей (других ViewModel, сервисов).
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Сервис для загрузки тем/стилей из .axaml файлов.
    /// </summary>
    private readonly IThemeLoaderService _themeLoader; // Он не используется напрямую здесь, а в ModelSettingsViewModel
    
    private readonly ISettingsService _settingsService; // Добавляем сервис
    
    // --- Имя и Путь к Файлу Настроек ---
    private const string SettingsFileName = "boiling_pot_settings.json";
    private readonly string _settingsFilePath;
    
    public ReactiveCommand<Unit, Unit> ApplyAndSaveCurrentSettingsCommand { get; } 
    
    #endregion

    #region Свойства Управления Состоянием UI

    /// <summary>
    ///     Текущий активный ViewModel для основного контента (Home, Common, Molecular).
    ///     Привязывается к ContentControl в MainWindow.axaml.
    /// </summary>
    [Reactive]
    public ViewModelBase? CurrentViewModel { get; private set; }

    [Reactive] private CommonViewModel CommonVm { get; set; }

    /// <summary>
    ///     (Если используется) Контрол, загруженный динамически (например, кастомная тема для элемента).
    ///     Может быть не нужен, если вся логика тем в ModelSettingsViewModel.
    /// </summary>
    [Reactive]
    public Control? DynamicViewContent { get; private set; }

    /// <summary>
    ///     (Если используется) Флаг, указывающий, активен ли DynamicViewContent.
    /// </summary>
    [Reactive]
    public bool IsDynamicViewActive { get; private set; }

    [Reactive] public bool IsCommonViewSelected { get; set; }


    // --- Флаги видимости для панелей/окон ---

    /// <summary>
    ///     Управляет видимостью панели/окна настроек.
    /// </summary>
    [Reactive]
    public bool IsShowingSettings { get; set; }

    /// <summary>
    ///     Управляет видимостью панели/окна "О программе".
    /// </summary>
    [Reactive]
    public bool IsShowingAbout { get; set; }

    /// <summary>
    ///     Управляет видимостью выдвижной панели данных.
    /// </summary>
    [Reactive]
    public bool IsShowingDataPanel { get; set; }

    #endregion

    #region Вычисляемые Свойства для Управления Видимостью Экранов

    /// <summary>
    ///     Только для чтения свойство, указывающее, активен ли в данный момент HomeView.
    ///     Вычисляется на основе типа CurrentViewModel.
    /// </summary>
    private readonly ObservableAsPropertyHelper<bool> _isHomeViewSelectedHelper;

    // Переключатель состояния Home/NotHome (для IsVisible в MainWindow)
    // Используем ObservableAsPropertyHelper для создания вычисляемого свойства
    private readonly ObservableAsPropertyHelper<bool>? _isCommonViewSelectedHelper;
    public bool IsHomeViewSelected => _isHomeViewSelectedHelper.Value;

    /// <summary>
    ///     Только для чтения свойство, указывающее, НЕ активен ли в данный момент HomeView.
    ///     (т.е., активен CommonView или MolecularView).
    ///     Вычисляется на основе IsHomeViewSelected.
    /// </summary>
    private readonly ObservableAsPropertyHelper<bool>? _notIsHomeViewSelectedHelper;

    public bool NotIsHomeViewSelected => _notIsHomeViewSelectedHelper.Value;

    #endregion

    #region ViewModel для Всплывающих Панелей

    /// <summary>
    ///     Экземпляр ViewModel для панели/окна настроек.
    ///     Получается из DI контейнера (обычно как Singleton).
    /// </summary>
    public SettingsViewModel SettingsVM { get; }

    #endregion

    #region Свойства для ControlPanel (Пример, если часть логики ControlPanel здесь)

    // Эти свойства, скорее всего, должны быть в своем ControlPanelViewModel,
    // но если ControlPanelViewModel - это часть MainViewModel (что не очень хорошо для разделения),
    // то они могут быть здесь.
    // В твоем коде они были в MainViewModel, поэтому я их оставляю, но с комментарием.

    [Reactive] public int FlameLevel { get; set; } // Уровень нагрева
    [Reactive] public double ProcessSpeed { get; set; } = 1.0; // Скорость процессов
    [Reactive] public string? SelectedVolume { get; set; } // Выбранный объем
    [Reactive] public string? SelectedLiquidType { get; set; } // Выбранный тип жидкости
    [Reactive] public object? SelectedNavItem { get; set; } // Выбранный элемент в NavigationView из ControlPanel

    // Опции для ComboBox'ов из ControlPanel
    public string[] VolumeOptions { get; } =
    {
        "1.0 литр", "1.5 литра", "2.5 литра", "3.5 литра", "5.0 литров", "6.0 литров", "10.0 литров"
    };

    public string[] LiquidTypes { get; } =
    {
        "Вода", "Спирт", "Масло (раст.)", "Парафин", "Керосин", "Ртуть (жид.)"
    };

    // Свойства для DataPanel, которые могут обновляться в зависимости от настроек ControlPanel
    [Reactive] public double PowerRating { get; private set; }
    [Reactive] public TimeSpan ElapsedTime { get; private set; }
    [Reactive] public double InitialTemperature { get; set; } = 20.0;
    [Reactive] public double CurrentAverageTemperature { get; private set; }
    [Reactive] public double LiquidVolume { get; private set; }
    [Reactive] public double LiquidDensity { get; private set; }
    [Reactive] public double BoilingPointTemperature { get; private set; }
    [Reactive] public double SpecificHeatCapacity { get; private set; }
    [Reactive] public double LiquidMass { get; private set; }
    [Reactive] public double HeatTransferred { get; private set; }
    [Reactive] public bool IsHeatingActive { get; private set; }
    [Reactive] public bool IsSimulationPausedBySpeed { get; private set; }

    private DispatcherTimer? _simulationTimer;
    private DateTime _lastTickRealTime;
    private TimeSpan _accumulatedSimulatedTime = TimeSpan.Zero;
    private readonly TimeSpan _baseTimerInterval = TimeSpan.FromMilliseconds(100);

    private readonly Dictionary<int, double> _powerLevels = new()
    {
        { 0, 0 }, { 1, 800 }, { 2, 1300 }, { 3, 1900 }, { 4, 2700 }, { 5, 3500 }
    };

    #endregion

    #region Команды

    // Команды для навигации между основными экранами
    public ReactiveCommand<Unit, Unit> GoToHomeCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToCommonCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToMolecularCommand { get; }
    
    public ReactiveCommand<Unit, Unit> SaveApplicationSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadApplicationSettingsCommand { get; }
    // Команды для управления видимостью панелей
    public ReactiveCommand<Unit, Unit> ShowSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowDataPanelCommand { get; }

    // Другие команды
    public ReactiveCommand<Unit, Unit>? LoadFileCommand { get; } // Заглушка
    public ReactiveCommand<Unit, Unit> CoolDownCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShowStructureCommand { get; } // Заглушка
    public ReactiveCommand<Unit, Unit> ExitApplicationCommand { get; }

    public ReactiveCommand<Unit, Unit>? LoadDynamicThemeCommand { get; } // Команда теперь только в ModelSettingsVM

    #endregion


    /// <summary>
    ///     Конструктор
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="themeLoader"></param>
    /// <param name="settingsViewModel"></param>
    /// <param name="commonViewModel"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MainViewModel(
        IServiceProvider serviceProvider,
        IThemeLoaderService themeLoader,
        SettingsViewModel settingsViewModel,
        ISettingsService settingsService,
        CommonViewModel commonViewModel)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _themeLoader = themeLoader; // Сохраняем, если нужен
        _settingsService = settingsService; // Сохраняем
        CommonVm = commonViewModel;
        SettingsVM = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
        Debug.WriteLine(">>> MainViewModel СОЗДАН (RxUI)");


        // --- Подписка на Interaction от SettingsViewModel для закрытия ---
        SettingsVM.CloseSettingsInteraction.RegisterHandler(interaction =>
        {
            Debug.WriteLine($"[{GetType().Name}] Получен запрос на закрытие настроек от SettingsVM.");
            IsShowingSettings = false; // Скрываем панель настроек
            // SettingsVM.IsShowingSettings = false; // Это свойство в самом SettingsVM, он сам его выставит
            interaction.SetOutput(Unit.Default); // Сообщаем, что обработали
            Debug.WriteLine($"[{GetType().Name}] Обработка CloseSettingsInteraction для SettingsVM завершена.");
        });
        Debug.WriteLine(
            $"[{GetType().Name}] Конструктор RxUI: Обработчик CloseSettingsInteraction для SettingsVM настроен.");

        // ---- ПЕРЕДЕЛЫВАЕМ ЛОГИКУ НАСТРОЕК ----
        // SettingsService теперь внутри MainViewModel
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appSpecificFolder = Path.Combine(appDataPath, "BoilingPotApp");
        Directory.CreateDirectory(appSpecificFolder);
        _settingsFilePath = Path.Combine(appSpecificFolder, SettingsFileName);
        // _jsonOptions = new JsonSerializerOptions { /* ... конвертеры ... */ };
        // _currentAppSettings = new AppSettings(); // Начальные дефолтные
        // --- Конец логики настроек ---

        // --- Инициализация команд ---
        // ... (команды навигации GoToHome, GoToCommon и т.д. как раньше) ...
        SaveApplicationSettingsCommand = ReactiveCommand.CreateFromTask(ExecuteSaveSettingsDialogAsync);
        LoadApplicationSettingsCommand = ReactiveCommand.CreateFromTask(ExecuteLoadSettingsDialogAsync);



        // --- Инициализация Команд ---
        GoToHomeCommand = ReactiveCommand.Create(ExecuteGoToHome);
        GoToCommonCommand = ReactiveCommand.Create(ExecuteGoToCommon);
        GoToMolecularCommand = ReactiveCommand.Create(ExecuteGoToMolecular);

        ShowSettingsCommand = ReactiveCommand.Create(ExecuteShowSettings);
        ShowAboutCommand = ReactiveCommand.Create(ExecuteShowAbout);
        ShowDataPanelCommand = ReactiveCommand.Create(ExecuteShowDataPanel);

        ExitApplicationCommand = ReactiveCommand.Create(ExecuteExitApplication);
        CoolDownCommand = ReactiveCommand.Create(() => { HeatTransferred = 0; });
        ApplyAndSaveCurrentSettingsCommand = ReactiveCommand.CreateFromTask(ExecuteApplyAndSaveCurrentSettingsAsync);

        ApplySettingsFromService(_settingsService.CurrentSettings);

        // --- Подписки на изменения свойств для АВТОСОХРАНЕНИЯ (опционально) ---
        // Или можно сохранять только по кнопке "Применить" в SettingsViewModel
        
        // --- Инициализация Вычисляемых Свойств ---
        _isHomeViewSelectedHelper = this.WhenAnyValue(x => x.CurrentViewModel)
            .Select(vm => vm is HomeViewModel) // true, если текущий VM - HomeViewModel
            .ToProperty(this, x => x.IsHomeViewSelected); // Преобразуем в свойство

        
        // --- Подписки на изменения свойств для ОБНОВЛЕНИЯ AppSettings (но без автосохранения в файл) ---
        // Теперь сохранение в файл будет по явной команде.
        var settingsToWatch = Observable.Merge(
            this.WhenAnyValue(x => x.ProcessSpeed).Select(_ => Unit.Default), // Unit.Default чтобы типы совпали
            this.WhenAnyValue(x => x.FlameLevel).Select(_ => Unit.Default),
            this.WhenAnyValue(x => x.SelectedVolume).Select(_ => Unit.Default),
            this.WhenAnyValue(x => x.SelectedLiquidType).Select(_ => Unit.Default),
            SettingsVM.GeneralSettings.WhenAnyValue(x => x.SelectedLanguage).Select(_ => Unit.Default),
            // ... и так далее для ВСЕХ свойств, которые должны быть частью AppSettings
            SettingsVM.ThemeSettings.WhenAnyValue(x => x.SelectedThemeKey).Select(_ => Unit.Default),
            SettingsVM.ThemeSettings.WhenAnyValue(x => x.SelectedAccentPalette).Select(_ => Unit.Default),
            SettingsVM.ModelSettings.WhenAnyValue(x => x.SelectedPotThemeKey).Select(_ => Unit.Default)
            // Добавьте сюда остальные свойства из General, Theme, Model settings
        );

        settingsToWatch
            .Throttle(TimeSpan.FromMilliseconds(500)) // Небольшая задержка
            .Subscribe(_ =>
            {
                // Просто обновляем объект CurrentSettings в сервисе, но НЕ сохраняем в файл.
                // Сохранение будет по команде.
                UpdateAppSettingsFromViewModels(_settingsService.CurrentSettings);
                Debug.WriteLine("[MainViewModel] AppSettings в памяти обновлены после изменения ViewModel.");
            });

        // Подписка на нажатие на выбранный элемент в NavigationView из ControlPanel
        this.WhenAnyValue(x => x.SelectedNavItem)
            .Where(item => item is NavigationViewItem)! // Убедимся, что это нужный тип
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
                    case "Load": _ = ExecuteLoadSettingsDialogAsync(); break;
                    case "Save": _ = ExecuteSaveSettingsDialogAsync(); break;
                    case "Settings": ExecuteShowSettings(); break;
                    case "About": ExecuteShowAbout(); break;
                    case "Exit": ExecuteExitApplication(); break;
                }

                Dispatcher.UIThread.Post(
                    () => SelectedNavItem = null, DispatcherPriority.Background);
            });

        //  --- Подписка на изменение вида
        this.WhenAnyValue(x => x.IsCommonViewSelected)
            .Subscribe(isCommonViewSelected =>
            {
                Debug.WriteLine($">>> IsCommonViewSelected = {isCommonViewSelected}");
                if (isCommonViewSelected) ExecuteGoToCommon();
                else ExecuteGoToMolecular();
            });

        // --- Подписка на СВОИ ProcessSpeed и FlameLevel ---
        // Когда они меняются, и если текущий View - это MolecularViewModel,
        // мы обновляем соответствующие свойства в MolecularViewModel.
        this.WhenAnyValue(x => x.ProcessSpeed, x => x.FlameLevel,
                x => x.CurrentViewModel,
                x => x.CurrentAverageTemperature, x => x.BoilingPointTemperature)
            .Where(tuple => tuple.Item3 is MolecularViewModel) // Реагируем только если текущий вид - молекулярный
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(tuple =>
            {
                var (speed, flame, currentVm, curtemp, curboiltemp) = tuple;
                if (currentVm is MolecularViewModel molecularVm)
                {
                    Debug.WriteLine(
                        $"[MainViewModel] Обновление параметров в MolecularViewModel: Speed={speed}, Flame={flame}");
                    molecularVm.CurrentProcessSpeed = speed;
                    molecularVm.CurrentFlameLevel = flame;
                    molecularVm.CurrentTemperature = curtemp;
                    molecularVm.CurrentBoilingTemperature = curboiltemp;
                }
            });


        // --- Подписки на изменения свойств из ControlPanel (FlameLevel, ProcessSpeed и т.д.) ---
        SetupControlPanelSubscriptions();

        // --- Инициализируем начальное состояние ---
        IsCommonViewSelected = true;

        SettingsVM.CloseSettingsInteraction.RegisterHandler(interaction =>
        {
            IsShowingSettings = false;
            interaction.SetOutput(Unit.Default);
        });
        
        // --- Устанавливаем начальный вид ---
        ExecuteGoToHome();
    }


    #region Логика Симуляции и Панели Данных (Перенесено из старого кода)

    private void SetupControlPanelSubscriptions()
    {
        Debug.WriteLine($"[{GetType().Name}] SetupControlPanelSubscriptions: Настройка подписок.");

        // Подписка на изменение скорости процесса
        this.WhenAnyValue(x => x.ProcessSpeed)
            .Subscribe(newSpeed =>
            {
                Debug.WriteLine($"[MainViewModel] ProcessSpeed изменен на: {newSpeed}x");
                if (IsHeatingActive) // Реагируем на скорость, только если нагрев в принципе активен
                {
                    if (newSpeed <= 0 && (_simulationTimer?.IsEnabled ?? false))
                        // Скорость стала 0 или меньше, ставим симуляцию на ПАУЗУ
                        PauseSimulationBySpeed();
                    else if (newSpeed > 0 && IsSimulationPausedBySpeed)
                        // Скорость стала > 0, возобновляем симуляцию с ПАУЗЫ
                        ResumeSimulationFromSpeedPause();
                    // Если newSpeed > 0 и не было паузы по скорости, то скорость просто
                    // будет учтена в следующем SimulationTimer_Tick.
                }
            });

        // Подписка на изменение уровня нагрева (FlameLevel)
        this.WhenAnyValue(x => x.FlameLevel)
            .Subscribe(level =>
            {
                PowerRating = _powerLevels[FlameLevel];
                // --- Логика управления таймером ---
                if (level > 0 && !IsHeatingActive && HeatTransferred == 0)
                    StartHeating();
                // Нагрев ВКЛЮЧАЕТСЯ (был 0, стал > 0)
                else if (level > 0)
                    IsHeatingActive = true;
                // Нагрев ПРОДОЛЖАЕТСЯ
                else if (level == 0)
                    // Нагрев ВЫКЛЮЧАЕТСЯ (был > 0, стал 0)
                    IsHeatingActive = false;
            });

        // Подписка на изменение выбранного объема
        this.WhenAnyValue(x => x.SelectedVolume)
            .Skip(1)
            .Subscribe(selectedVolume =>
            {
                if (selectedVolume != null)
                {
                    _ = CommonVm.UpdatePotVolumeText(selectedVolume);
                    UpdateLiquidParameters(selectedVolume, SelectedLiquidType);
                    _ = CommonVm.ModelVm.ApplySelectedThemeAsync(CommonVm.ModelVm.SelectedPotThemeKey, "Pot");
                    Debug.WriteLine($"[MainViewModel] SelectedKEEEEEEEY = {CommonVm.ModelVm.SelectedPotThemeKey}");
                }
            });

        // Подписка на изменение типа жидкости
        this.WhenAnyValue(x => x.SelectedLiquidType)
            .Skip(1)
            .Subscribe(selectedLiquidType =>
            {
                if (selectedLiquidType != null)
                {
                    _ = CommonVm.UpdateLiquidType(SelectedLiquidType!);
                    UpdateLiquidParameters(SelectedVolume, selectedLiquidType);
                    _ = CommonVm.ModelVm.ApplySelectedThemeAsync(CommonVm.ModelVm.SelectedPotThemeKey, "Pot");
                    Debug.WriteLine($"[MainViewModel] SelectedKEEEEEEEY = {CommonVm.ModelVm.SelectedPotThemeKey}");

                    if (CurrentViewModel is MolecularViewModel molecularVm) molecularVm.GenerateBubbles();
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

        // Инициализация начальных значений для DataPanel
        UpdateLiquidParameters(SelectedVolume ?? VolumeOptions.FirstOrDefault(),
            SelectedLiquidType ?? LiquidTypes.FirstOrDefault());
        ElapsedTime = TimeSpan.Zero;
        IsHeatingActive = false;
        IsSimulationPausedBySpeed = false;
    }

    #endregion

    #region Методы Выполнения Команд

    private void ExecuteGoToHome()
    {
        DynamicViewContent = null;
        IsDynamicViewActive = false;
        IsCommonViewSelected = false;
        CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
        Debug.WriteLine(">>> Переход на Home");
    }

    private void ExecuteGoToCommon()
    {
        DynamicViewContent = null;
        IsDynamicViewActive = false;
        IsCommonViewSelected = true;
        CurrentViewModel = CommonVm;
        Debug.WriteLine(">>> Переход на Common");
    }

    private void ExecuteGoToMolecular()
    {
        IsCommonViewSelected = false;
        DynamicViewContent = null;
        IsDynamicViewActive = false;
        CurrentViewModel = _serviceProvider.GetRequiredService<MolecularViewModel>();
        Debug.WriteLine(">>> Переход на Molecular");
    }

    private void ExecuteShowSettings()
    {
        SettingsVM.IsShowingSettings = !SettingsVM.IsShowingSettings;
    }

    private void ExecuteShowAbout()
    {
        IsShowingAbout = !IsShowingAbout;
    }

    private void ExecuteShowDataPanel()
    {
        IsShowingDataPanel = !IsShowingDataPanel;
    }

    private void ExecuteExitApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
    
        // --- Метод для обновления свойств ViewModel из объекта AppSettings ---
        private void ApplySettingsFromService(AppSettings settings)
        {
            Debug.WriteLine("[MainViewModel] ApplySettingsFromService: Применение загруженных/текущих настроек.");

            // ControlPanel (MainViewModel)
            this.ProcessSpeed = settings.ProcessSpeed;
            this.FlameLevel = settings.FlameLevel;
            this.SelectedVolume = settings.SelectedVolume;
            this.SelectedLiquidType = settings.SelectedLiquidType;

            // GeneralSettingsViewModel
            var gsVM = SettingsVM.GeneralSettings;
            gsVM.SelectedLanguage = settings.SelectedLanguage;
            gsVM.ShowDataPanelButton = settings.ShowDataPanelButton;
            gsVM.SelectedDataPanelButtonPosition = settings.SelectedDataPanelButtonPosition;
            gsVM.IsDataPanelOnLeft = settings.IsDataPanelOnLeft;
            if(Enum.TryParse<HorizontalAlignment>(settings.DataPanePlacementString, out var horAlign))
                gsVM.DataPanelButtonHorAlignment = horAlign; else gsVM.DataPanelButtonHorAlignment = HorizontalAlignment.Right;
            if(Enum.TryParse<VerticalAlignment>(settings.SelectedDataPanelButtonPosition.ToString(), out var verAlign)) 
                gsVM.DataPanelButtonVerAlignment = verAlign; else gsVM.DataPanelButtonVerAlignment = VerticalAlignment.Top;
            if(Enum.TryParse<Symbol>(settings.DataPanelButtonSymbolString, true, out var symbol))
                gsVM.DataPanelButtonSymbol = symbol; else gsVM.DataPanelButtonSymbol = Symbol.ChevronLeft;

            gsVM.ShowHomeNavItem = settings.ShowHomeNavItem; /* ... и другие NavItem ... */

            // ThemeSettingsViewModel
            var tsVM = SettingsVM.ThemeSettings;
            tsVM.SelectedThemeKey = settings.SelectedThemeKey; // Вызовет ApplyTheme
            tsVM.SelectedAccentPalette = tsVM.AccentPalettes.FirstOrDefault(p => p.Name == settings.SelectedAccentPaletteName) ?? tsVM.AccentPalettes.FirstOrDefault(); // Вызовет ApplyAccentPalette

            // ModelSettingsViewModel
            var msVM = SettingsVM.ModelSettings;
            // Сохраняем пути к кастомным темам, чтобы ViewModel мог их загрузить при выборе "Custom"
            // msVM.SetCustomThemeFilePath("Pot", settings.CustomPotThemeFilePath);
            // msVM.SetCustomThemeFilePath("Stove", settings.CustomStoveThemeFilePath);
            // msVM.SetCustomThemeFilePath("Bubble", settings.CustomBubbleThemeFilePath);
            // Устанавливаем ключи тем (это вызовет применение тем в ModelSettingsVM)
            msVM.SelectedPotThemeKey = settings.PotThemeKey;
            msVM.SelectedStoveThemeKey = settings.StoveThemeKey;
            msVM.SelectedBubbleThemeKey = settings.BubbleThemeKey;

            Debug.WriteLine("[MainViewModel] ApplySettingsFromService: Настройки применены к ViewModel.");
             // Обновляем связанные параметры симуляции, если они зависят от загруженных настроек
             UpdateLiquidParameters(this.SelectedVolume, this.SelectedLiquidType);
             // UpdateHeatingState(this.FlameLevel, this.ProcessSpeed);
        }

        // --- Метод для обновления объекта AppSettings из текущих ViewModel ---
        private void UpdateAppSettingsFromViewModels(AppSettings settings)
        {
            Debug.WriteLine("[MainViewModel] UpdateAppSettingsFromViewModels: Обновление объекта AppSettings.");
            // ControlPanel (MainViewModel)
            settings.ProcessSpeed = this.ProcessSpeed;
            settings.FlameLevel = this.FlameLevel;
            settings.SelectedVolume = this.SelectedVolume;
            settings.SelectedLiquidType = this.SelectedLiquidType;

            // GeneralSettingsViewModel
            settings.SelectedLanguage = SettingsVM.GeneralSettings.SelectedLanguage;
            settings.ShowDataPanelButton = SettingsVM.GeneralSettings.ShowDataPanelButton;
            // ... и так далее для всех свойств GeneralSettings ...
            settings.DataPanePlacementString = SettingsVM.GeneralSettings.DataPanePlacement.ToString();
            settings.DataPanelButtonSymbolString = SettingsVM.GeneralSettings.DataPanelButtonSymbol.ToString();


            // ThemeSettingsViewModel
            settings.SelectedThemeKey = SettingsVM.ThemeSettings.SelectedThemeKey;
            settings.SelectedAccentPaletteName = SettingsVM.ThemeSettings.SelectedAccentPalette?.Name;

            // ModelSettingsViewModel
            settings.PotThemeKey = SettingsVM.ModelSettings.SelectedPotThemeKey;
            // settings.CustomPotThemeFilePath = SettingsVM.ModelSettings.GetCustomThemeFilePath("Pot");
            // ... и так далее для Stove и Bubble ...
            Debug.WriteLine("[MainViewModel] UpdateAppSettingsFromViewModels: Объект AppSettings обновлен.");
        }


        // --- Реализация Команд Сохранения/Загрузки ---
        private async Task ExecuteSaveSettingsDialogAsync()
        {
            Debug.WriteLine("[MainViewModel] ExecuteSaveSettingsDialogAsync: Вызвана команда сохранения через диалог.");
            // Сначала убедимся, что CurrentSettings в сервисе актуальны
            UpdateAppSettingsFromViewModels(_settingsService.CurrentSettings);
            // Затем вызываем метод сервиса для сохранения через диалог
            bool saved = await _settingsService.SaveSettingsToFileDialogAsync();
            if (saved) { Debug.WriteLine("[MainViewModel] Настройки сохранены через диалог."); /* Показать уведомление? */ }
            else { Debug.WriteLine("[MainViewModel] Сохранение через диалог отменено или не удалось."); }
        }

        private async Task ExecuteLoadSettingsDialogAsync()
        {
            Debug.WriteLine("[MainViewModel] ExecuteLoadSettingsDialogAsync: Вызвана команда загрузки через диалог.");
            bool loaded = await _settingsService.LoadSettingsFromFileDialogAsync();
            if (loaded)
            {
                Debug.WriteLine("[MainViewModel] Настройки загружены из файла через диалог. Применяем...");
                ApplySettingsFromService(_settingsService.CurrentSettings); // Применяем загруженные настройки
                // TODO: Уведомить пользователя об успехе
            }
            else { Debug.WriteLine("[MainViewModel] Загрузка через диалог отменена или не удалась."); }
        }

        private async Task ExecuteApplyAndSaveCurrentSettingsAsync()
        {
            Debug.WriteLine("[MainViewModel] ExecuteApplyAndSaveCurrentSettingsAsync: Применение и сохранение текущих настроек.");
            // 1. Обновляем объект AppSettings в сервисе из текущих ViewModel
            UpdateAppSettingsFromViewModels(_settingsService.CurrentSettings);
            // 2. Сохраняем этот объект в файл по умолчанию
            await _settingsService.SaveDefaultSettingsAsync();
            // 3. (Опционально) Применяем настройки снова, если нужно (хотя они уже должны быть в ViewModel)
            // ApplySettingsFromService(_settingsService.CurrentSettings);
            Debug.WriteLine("[MainViewModel] Текущие настройки применены и сохранены в файл по умолчанию.");
            // TODO: Уведомить пользователя
        }

    #endregion

    /// <summary>
    ///     Метод для обновления параметров жидкости при выборе из ComboBox
    /// </summary>
    private void UpdateLiquidParameters(string? selectedVolume, string? selectedLiquidType)
    {
        // --- Обновление объема и конвертация в м³ ---
        if (selectedVolume != null && double.TryParse(selectedVolume.Split(' ')[0], NumberStyles.Any,
                CultureInfo.InvariantCulture, out var volValueLiters))
            LiquidVolume = volValueLiters / 1000.0; // Конвертируем литры в м³
        else
            LiquidVolume = 0.001; // 1 литр по умолчанию
        Debug.WriteLine($"CURRENT VOLUME TRIED TO PARSE {selectedVolume?.Split(' ')[0]}");

        // --- Обновление плотности и уд. теплоемкости в зависимости от типа жидкости ---
        switch (selectedLiquidType)
        {
            case "Вода":
                LiquidDensity = 998; // кг/м³ при ~20°C
                SpecificHeatCapacity = 4186; // Дж/(кг·°C)
                BoilingPointTemperature = 100.00;
                break;
            case "Керосин":
                LiquidDensity = 800;
                SpecificHeatCapacity = 2090;
                BoilingPointTemperature = 160.11;
                break;
            case "Масло (раст.)": // Подсолнечное как пример
                LiquidDensity = 920;
                SpecificHeatCapacity = 1900; // Примерно
                BoilingPointTemperature = 227.82;
                break;
            case "Парафин": // Твердый при комн. темп, но для симуляции нагрева жидкого
                LiquidDensity = 770; // Жидкий
                SpecificHeatCapacity = 2100; // Жидкий
                BoilingPointTemperature = 324.20;
                break;
            case "Спирт": // Этанол
                LiquidDensity = 789;
                SpecificHeatCapacity = 2440;            
                BoilingPointTemperature = 78.37;
                break;
            case "Ртуть (жид.)":
                LiquidDensity = 13534;
                SpecificHeatCapacity = 139;
                BoilingPointTemperature = 357.13;
                break;
            default:
                LiquidDensity = 0; // По умолчанию вода
                SpecificHeatCapacity = 0;
                BoilingPointTemperature = 0;
                break;
        }

        // Обновляем CurrentAverageTemperature, чтобы начать с InitialTemperature
        LiquidMass = LiquidVolume * LiquidDensity;
        CurrentAverageTemperature = InitialTemperature;
        HeatTransferred = SpecificHeatCapacity * LiquidMass * (CurrentAverageTemperature - InitialTemperature);

        // Сбрасываем состояние симуляции при смене параметров
        // Сначала полностью останавливаем нагрев
        var wasHeating = IsHeatingActive;
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


    #region Функции маниауляции с данными на панели

    // Метод для начала процесса нагрева (когда FlameLevel > 0)
    private void StartHeating()
    {
        Debug.WriteLine("[MainViewModel] StartHeating: Начало процесса нагрева.");
        IsHeatingActive = true;
        _accumulatedSimulatedTime = TimeSpan.Zero; // Сбрасываем симулированное время
        ElapsedTime = _accumulatedSimulatedTime; // Обновляем UI
        _lastTickRealTime = DateTime.Now; // Запоминаем время старта/возобновления

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
            Debug.WriteLine(
                $"[MainViewModel] EnsureTimerStarted: Таймер запущен/создан с интервалом {_baseTimerInterval.TotalMilliseconds} мс.");
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

        var currentRealTime = DateTime.Now;
        // Реальное время, прошедшее с последнего тика (или с момента _lastTickRealTime)
        var realTimeDelta = currentRealTime - _lastTickRealTime;
        _lastTickRealTime = currentRealTime; // Обновляем время последнего тика

        // Симулированное время, прошедшее за этот реальный интервал
        var simulatedMillisecondsThisTick = realTimeDelta.TotalMilliseconds * ProcessSpeed;
        _accumulatedSimulatedTime += TimeSpan.FromMilliseconds(simulatedMillisecondsThisTick);
        ElapsedTime = _accumulatedSimulatedTime;
        // --- Логика симуляции нагрева ---

        if (IsHeatingActive) HeatTransferred += PowerRating * simulatedMillisecondsThisTick / 1000.0;
        else if (HeatTransferred >= 0) HeatTransferred -= 0.07 * simulatedMillisecondsThisTick;

        if ((BoilingPointTemperature - (InitialTemperature + HeatTransferred / (LiquidMass * SpecificHeatCapacity))) > 0.1d)
            CurrentAverageTemperature = InitialTemperature + HeatTransferred / (LiquidMass * SpecificHeatCapacity);
        else CurrentAverageTemperature = BoilingPointTemperature;
        // ... (расчет температуры и т.д., используя simulatedMillisecondsThisTick или realTimeDelta * ProcessSpeed) ...
    }

    #endregion
}