// ViewModels/MolecularViewModel.cs
using ReactiveUI; // Базовый класс RxUI
using ReactiveUI.Fody.Helpers; // Для [Reactive]
using System;
using System.Diagnostics; // Для Debug
using System.Reactive; // Для Unit
using System.Reactive.Linq; // Для Where, Select
using System.Collections.ObjectModel; // Для ObservableCollection
using Avalonia.Media; // Для IBrush
using Avalonia.Threading; // Для DispatcherTimer
using BoilingPot.ViewModels.Components; // Для BubbleViewModel
using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider
using System.Threading.Tasks; // Для Task, Task.Delay

namespace BoilingPot.ViewModels
{
    // ViewModel для экрана молекулярной симуляции.
    // Управляет коллекцией пузырьков и логикой их анимации (симуляции).
    public partial class MolecularViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private DispatcherTimer? _simulationTimer; // Таймер для симуляции движения
        private Random _random = new Random(); // Для рандомизации

        // Коллекция пузырьков для отображения в ItemsControl в View.
        // ObservableCollection уведомляет View об изменении коллекции (добавление/удаление).
        public ObservableCollection<BubbleViewModel> Bubbles { get; } = new ObservableCollection<BubbleViewModel>();

        // Свойства для управления симуляцией (могут быть привязаны к UI)
        // Можно получить эти значения из CommonViewModel или GlobalSettings/ModelSettings
        [Reactive] public double SimulationSpeed { get; set; } = 1.0; // Скорость симуляции
        [Reactive] public double HeatIntensity { get; set; } = 1.0; // Интенсивность нагрева

        // Флаг для управления состоянием симуляции (запущена/остановлена)
        [Reactive] public bool IsSimulationRunning { get; private set; }

        // Команды для управления симуляцией
        public ReactiveCommand<Unit, Unit> StartSimulationCommand { get; }
        public ReactiveCommand<Unit, Unit> StopSimulationCommand { get; }


        // Конструктор. Получает зависимости (IServiceProvider).
        public MolecularViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: Начало.");

            // Инициализация команд
            StartSimulationCommand = ReactiveCommand.Create(ExecuteStartSimulation,
                this.WhenAnyValue(x => x.IsSimulationRunning).Select(isRunning => !isRunning)); // Команда активна, если симуляция НЕ запущена
            StopSimulationCommand = ReactiveCommand.Create(ExecuteStopSimulation,
                this.WhenAnyValue(x => x.IsSimulationRunning).Select(isRunning => isRunning)); // Команда активна, если симуляция ЗАПУЩЕНА

            // Подписка на активацию/деактивацию ViewModel (когда View становится видимым/скрывается)
            // Используется для запуска/остановки таймера симуляции
            // ВАЖНО: Эта подписка должна быть настроена в CodeBehind View (MolecularView.axaml.cs)
            // через WhenActivated, а не здесь в ViewModel! ViewModel не должен знать о жизненном цикле View.

             Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: Завершение.");
        }

        // Метод инициализации симуляции (вызывается при переходе на этот экран)
        // Асинхронный, т.к. может включать загрузку данных/настроек
        public async Task InitializeAsync(double initialSpeed, double initialHeat)
        {
            Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Начало инициализации симуляции.");
            SimulationSpeed = initialSpeed;
            HeatIntensity = initialHeat;

            // Логика генерации пузырьков
            GenerateBubbles();

            // Запуск симуляции (таймера)
            ExecuteStartSimulation(); // Запускаем сразу после инициализации

             Debug.WriteLine($"[{this.GetType().Name}] InitializeAsync: Инициализация завершена.");
        }

        // Метод для генерации начального набора пузырьков
        private void GenerateBubbles()
        {
            Bubbles.Clear(); // Очищаем предыдущие пузырьки
            Debug.WriteLine($"[{this.GetType().Name}] GenerateBubbles: Генерация пузырьков...");

            // --- Ваша логика генерации пузырьков ---
            // Основывается на параметрах симуляции (SimulationSpeed, HeatIntensity)
            // и, возможно, общих параметрах (объем, тип жидкости) из других VM.
            // Можете получать эти параметры через DI в конструкторе,
            // или передавать их в InitializeAsync, или читать из Singleton сервиса настроек.

            // Пример: Создание 50 пузырьков
            double potWidth = 300; // Условные размеры области
            double potHeight = 400;

            for (int i = 0; i < 50; i++)
            {
                // Создаем экземпляр BubbleViewModel (DI здесь обычно не нужен, т.к. это простая модель данных)
                var bubble = new BubbleViewModel
                {
                    // Случайная начальная позиция и размер
                    X = _random.NextDouble() * potWidth,
                    Y = _random.NextDouble() * potHeight,
                    Size = _random.NextDouble() * 10 + 5, // Размер от 5 до 15
                    ColorBrush = Brushes.Aqua, // Цвет по умолчанию

                    // Параметры анимации (Duration, HorizontalShift) -
                    // могут зависеть от SimulationSpeed, HeatIntensity
                    // ...
                };
                 Bubbles.Add(bubble);
            }
             Debug.WriteLine($"[{this.GetType().Name}] GenerateBubbles: Создано {Bubbles.Count} пузырьков.");
        }

        // --- Реализация методов команд симуляции ---

        private void ExecuteStartSimulation()
        {
            Debug.WriteLine($"[{this.GetType().Name}] ExecuteStartSimulation: Команда вызвана.");
            if (!IsSimulationRunning)
            {
                // Создаем или настраиваем таймер
                _simulationTimer ??= new DispatcherTimer();
                _simulationTimer.Interval = TimeSpan.FromMilliseconds(100); // Интервал обновления симуляции
                _simulationTimer.Tick -= SimulationTimer_Tick; // Отписываемся от старых подписок
                _simulationTimer.Tick += SimulationTimer_Tick; // Подписываемся на событие тика

                _simulationTimer.Start(); // Запускаем таймер
                IsSimulationRunning = true; // Обновляем флаг
                Debug.WriteLine($"[{this.GetType().Name}] ExecuteStartSimulation: Симуляция ЗАПУЩЕНА. Таймер запущен.");
            }
             else { Debug.WriteLine($"[{this.GetType().Name}] ExecuteStartSimulation: Симуляция уже запущена."); }
        }

        private void ExecuteStopSimulation()
        {
             Debug.WriteLine($"[{this.GetType().Name}] ExecuteStopSimulation: Команда вызвана.");
            if (IsSimulationRunning)
            {
                _simulationTimer?.Stop(); // Останавливаем таймер
                _simulationTimer = null; // Можно обнулить, чтобы создать заново при следующем запуске
                IsSimulationRunning = false; // Обновляем флаг
                Debug.WriteLine($"[{this.GetType().Name}] ExecuteStopSimulation: Симуляция ОСТАНОВЛЕНА. Таймер остановлен.");
            }
             else { Debug.WriteLine($"[{this.GetType().Name}] ExecuteStopSimulation: Симуляция уже остановлена."); }
        }

        // --- Обработчик тика таймера (логика движения пузырьков) ---
        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            // Debug.WriteLine($"[{this.GetType().Name}] SimulationTimer_Tick: Тик таймера."); // Осторожно, может спамить

            // !!! ЭТО МЕСТО ДЛЯ ВАШЕЙ СИМУЛЯЦИОННОЙ ЛОГИКИ !!!
            // Перебираем все пузырьки в коллекции Bubbles
            foreach (var bubble in Bubbles)
            {
                // --- ОБНОВЛЕНИЕ ПОЗИЦИИ ПУЗЫРЬКА ---
                // ЭТО ОЧЕНЬ УПРОЩЕННЫЙ ПРИМЕР. ВАМ НУЖНО РЕАЛИЗОВАТЬ ВАШУ ЛОГИКУ КОНВЕКЦИИ ЗДЕСЬ!
                // Например:
                // - Рассчитать новую позицию (X, Y) на основе SimulationSpeed, HeatIntensity,
                //   направления потока, случайных флуктуаций и т.д.
                // - Возможно, учитывать "столкновения" с другими пузырьками или стенками.

                // Пример: Простое случайное смещение
                double step = SimulationSpeed * 2; // Шаг зависит от скорости симуляции
                bubble.X += (_random.NextDouble() - 0.5) * step; // Случайное смещение по X
                bubble.Y += (_random.NextDouble() - 0.5) * step; // Случайное смещение по Y

                // Ограничиваем движение границами области симуляции (например, стенками кастрюли)
                 // if (bubble.X < 0) bubble.X = 0;
                 // if (bubble.Y < 0) bubble.Y = 0;
                 // ... (проверки границ по всем сторонам) ...

                // Привязки в XAML (Canvas.Left="{Binding X}", Canvas.Top="{Binding Y}")
                // автоматически обновят положение эллипсов на Canvas,
                // так как свойства X и Y помечены [Reactive] (через ViewModelBase).
            }
        }
    }
}