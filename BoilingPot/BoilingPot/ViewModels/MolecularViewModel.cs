// ViewModels/MolecularViewModel.cs

// --- Подключение необходимых пространств имен ---
using Avalonia.Media;
using Avalonia.Threading;
using BoilingPot.Services;
using BoilingPot.ViewModels.Components;
using BoilingPot.ViewModels.SettingsViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic; // Для List<Point> в BubbleViewModel
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BoilingPot.Models;

namespace BoilingPot.ViewModels
{
    public partial class MolecularViewModel : ViewModelBase
    {
        // --- Зависимости ---
        private readonly IServiceProvider _serviceProvider;
        private readonly ModelSettingsViewModel _modelSettings;

        [Reactive] public double CurrentProcessSpeed { get; set; }
        [Reactive] public int CurrentFlameLevel { get; set; }
        [Reactive] public double CurrentTemperature { get; set; }
        [Reactive] public double CurrentBoilingTemperature { get; set; }


        // --- Симуляция ---
        private DispatcherTimer? _simulationTimer;
        private Random _random = new Random();
        public ObservableCollection<BubbleViewModelBase> Bubbles { get; } = new ObservableCollection<BubbleViewModelBase>();
        [Reactive] public bool IsSimulationRunning { get; private set; }

        // --- Параметры "аквариума" для пузырьков ---
        public double AquariumX { get; } = 0;
        public double AquariumY { get; } = 0;
        public double AquariumWidth { get; } = 275;
        public double AquariumHeight { get; } = 360;
        
        public IBrush BetaColorBrush { get; } = Brushes.Transparent;

        // --- Визуализация пламени ---
        [Reactive] public double FlameVisualHeight { get; private set; }

        // --- НОВОЕ: Свойства для управления траекторией ---
        // Эти значения определяют "полосы" движения
        private double _edgeLaneWidthRatio = 0.25; // 25% ширины для боковых потоков вверх
        private double _centerLaneWidthRatio = 0.4; // 40% ширины для центрального потока вниз
        // Остальное - буферные зоны

        // Конструктор (остается почти таким же, как в предыдущей версии)
        public MolecularViewModel(
            IServiceProvider serviceProvider,
            ModelSettingsViewModel modelSettings)
        {
            _serviceProvider = serviceProvider;
            _modelSettings = modelSettings;
            Debug.WriteLine($"[{this.GetType().Name}] Конструктор: Начало.");
            

            // Подписки на FlameLevel, ProcessSpeed, изменения объема/жидкости
            this.WhenAnyValue(vm => vm.CurrentFlameLevel)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(flameLevel => FlameVisualHeight = flameLevel * 10.0);

            this.WhenAnyValue(vm => vm.CurrentProcessSpeed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(processSpeed =>
                {
                    if (processSpeed > 0 && !IsSimulationRunning && Bubbles.Any()) StartSimulation();
                    // Логика паузы будет в Tick
                });

            Observable.Merge(
                    _modelSettings.PotViewModelInstance.WhenAnyValue(vm => vm.PotVolumeText),
                    _modelSettings.PotViewModelInstance.WhenAnyValue(vm => vm.LiquidTypeText)
                )
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => GenerateBubbles());

            Debug.WriteLine($"[{this.GetType().Name}] Конструктор: Завершение.");
        }

        public async Task InitializeAsync()
        {
            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Начало.");
            GenerateBubbles();
            if (CurrentProcessSpeed > 0) StartSimulation();
            else IsSimulationRunning = false;
            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Завершение.");
            await Task.CompletedTask;
        }

        private void StartSimulation()
        {
            // ... (код StartSimulation как раньше, проверяет ProcessSpeed) ...
            Debug.WriteLine($"[{this.GetType().Name}] StartSimulation: Попытка запуска.");
            if (IsSimulationRunning && (_simulationTimer?.IsEnabled ?? false)) return;
            if (CurrentProcessSpeed <= 0)
            {
                Debug.WriteLine(
                    $"[{this.GetType().Name}] StartSimulation: ProcessSpeed ({CurrentProcessSpeed}) <= 0, таймер не запущен.");
                IsSimulationRunning = false;
                _simulationTimer?.Stop();
                return;
            }

            _simulationTimer ??= new DispatcherTimer();
            _simulationTimer.Interval = TimeSpan.FromMilliseconds(50); // ~20 FPS, можно уменьшить для скорости
            _simulationTimer.Tick -= SimulationTimer_Tick;
            _simulationTimer.Tick += SimulationTimer_Tick;
            _simulationTimer.Start();
            IsSimulationRunning = true;
            Debug.WriteLine($"[{this.GetType().Name}] StartSimulation: Таймер ЗАПУЩЕН.");
        }

        public void StopSimulation()
        {
            // ... (код StopSimulation как раньше) ...
            Debug.WriteLine($"[{this.GetType().Name}] StopSimulation: Попытка остановки.");
            _simulationTimer?.Stop();
            IsSimulationRunning = false;
            Debug.WriteLine($"[{this.GetType().Name}] StopSimulation: Таймер ОСТАНОВЛЕН. IsSimulationRunning = false.");
        }

        public void GenerateBubbles()
        {
            Bubbles.Clear();
            Debug.WriteLine($"[{this.GetType().Name}] GenerateBubbles: Начало генерации 40 пузырьков.");
            IBrush bubbleColor = _modelSettings.PotViewModelInstance.LiquidColor ?? Brushes.Transparent;
            int numberOfBubbles = 1400;
            double baseBubbleSize = 20;

            Debug.WriteLine($"[{this.GetType().Name}] GenerateBubbles: Количество={numberOfBubbles}, БазовыйРазмер={baseBubbleSize}");

            for (int i = 0; i < numberOfBubbles; i++)
            {
                var bubble = _serviceProvider.GetService<BubbleViewModelBase>() ?? new BubbleViewModelBase();
                bubble.Size = baseBubbleSize * (0.7 + _random.NextDouble() * 0.6);
                bubble.ColorBrush = bubbleColor;
                
                bubble.AquariumWidth = AquariumWidth;
                bubble.AquariumHeight = AquariumHeight;

                // --- Инициализируем начальную позицию СЛУЧАЙНО по всему аквариуму ---
                bubble.X = AquariumX + _random.NextDouble() * (AquariumWidth - bubble.Size);
                bubble.Y = AquariumY + _random.NextDouble() * (AquariumHeight - bubble.Size);

                // --- Создаем и присваиваем объект физики каждому пузырьку ---
                // Передаем начальную X для определения начального бокового смещения
                bubble.PhysicsLogic = new BubblePhysics(AquariumWidth, AquariumHeight, bubble.X);

                // Устанавливаем начальные параметры симуляции для физики этого пузырька
                bubble.PhysicsLogic.UpdateSimulationParameters(
                    CurrentProcessSpeed, // Текущая скорость
                    ((CurrentTemperature - 20) / CurrentBoilingTemperature) * 1.5 // Нормализованный нагрев
                );

                Bubbles.Add(bubble);
            }
            Debug.WriteLine($"[{this.GetType().Name}] GenerateBubbles: Сгенерировано {Bubbles.Count} пузырьков.");
        }

        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            double currentSpeed = CurrentProcessSpeed;
            double currentHeat = ((CurrentTemperature - 20) / CurrentBoilingTemperature) * 1.5;
            Debug.WriteLine($"currentHeat {currentHeat}");

            if (currentSpeed <= 0) return;

            bool firstBubbleLoggedThisTick = false;
            foreach (var bubble in Bubbles)
            {
                if (bubble.PhysicsLogic == null) continue;

                // Обновляем параметры физики для пузырька актуальными значениями
                bubble.PhysicsLogic.UpdateSimulationParameters(currentSpeed, currentHeat);

                // Вызываем метод обновления позиции из его собственного объекта физики
                bubble.PhysicsLogic.UpdateBubblePosition(bubble); // Передаем сам bubble

                if (!firstBubbleLoggedThisTick) // Логируем только первый пузырек за тик
                {
                    // Debug.WriteLine($"[{this.GetType().Name}] Tick: Bubble[0] Pos:({bubble.X:F1},{bubble.Y:F1})");
                    firstBubbleLoggedThisTick = true;
                }
            }
        }
    }
}