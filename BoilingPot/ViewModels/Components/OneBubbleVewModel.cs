using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BoilingPot.ViewModels.Components;

public partial class BubbleViewModel : ObservableObject // Используем CommunityToolkit.Mvvm
{
    [ObservableProperty] private IBrush _colorBrush = Brushes.CornflowerBlue; // Цвет

    // Доп. свойства для управления анимацией (если нужно)
    [ObservableProperty] private TimeSpan _animationDuration = TimeSpan.FromSeconds(5); // Длительность анимации

    [ObservableProperty] private double _horizontalShift; // Для бокового смещения

    [ObservableProperty] private double _size; // Диаметр пузырька

    [ObservableProperty] private double _x; // Для Canvas.Left

    [ObservableProperty] private double _y; // Для Canvas.Top
}

// В вашем основном ViewModel (например, MainWindowViewModel.cs)
public partial class MainWindowViewModel : ViewModelBase
{
    // ... (ваши существующие свойства: ProcessSpeed, FlameLevel, VolumeOptions, LiquidTypes и т.д.)

    // Коллекция для хранения пузырьков
    public ObservableCollection<BubbleViewModel> Bubbles { get; } = new();

    // Вызывается при активации молекулярного вида
    public void InitializeMolecularView()
    {
        Bubbles.Clear(); // Очищаем старые

        // --- ЛОГИКА ГЕНЕРАЦИИ ПУЗЫРЬКОВ ---
        // Здесь вам нужно создать N пузырьков на основе:
        // - SelectedVolume (влияет на количество и/или размер)
        // - SelectedLiquidType (влияет на цвет)
        // - FlameLevel, ProcessSpeed (могут влиять на начальные параметры анимации)

        // Пример: создадим несколько пузырьков для демонстрации
        var random = new Random();
        double potWidth = 300; // Ширина области для пузырьков (условно)
        double potHeight = 400; // Высота области

        // Определяем цвет на основе выбранной жидкости
    //     var bubbleColor = GetLiquidColor(SelectedLiquidType);
    //
    //     // Определяем базовый размер на основе объема
    //     var baseSize = CalculateBubbleSize(SelectedVolume);
    //
    //     // Определяем базовую длительность анимации
    //     // Чем выше нагрев и скорость, тем *меньше* Duration (быстрее)
    //     // Нужна формула! Пример:
    //     var baseDurationSeconds = 10.0 / (FlameLevel + ProcessSpeed + 1.0); // Очень упрощенно!
    //     if (baseDurationSeconds < 1) baseDurationSeconds = 1; // Минимальная длительность
    //
    //     for (var i = 0; i < 50; i++) // Создаем 50 пузырьков (количество может зависеть от объема)
    //     {
    //         var bubble = new BubbleViewModel
    //         {
    //             X = random.NextDouble() * (potWidth - baseSize),
    //             Y = random.NextDouble() * (potHeight - baseSize),
    //             Size = baseSize * (0.8 + random.NextDouble() * 0.4), // Небольшой разброс размера
    //             ColorBrush = bubbleColor,
    //             // Длительность может немного варьироваться
    //             
    //             AnimationDuration = TimeSpan.FromSeconds(baseDurationSeconds * (0.9 + random.NextDouble() * 0.2)),
    //             // Горизонтальное смещение для имитации потоков
    //             HorizontalShift = (random.NextDouble() - 0.5) * potWidth * 0.2 // Сдвиг на +/- 10% ширины
    //         };
    //         Bubbles.Add(bubble);
    //     }
    // }
    //
    // // Вспомогательные методы (реализуйте их)
    // private IBrush GetLiquidColor(string? liquidType)
    // {
    //     return liquidType switch
    //     {
    //         "Вода" => Brushes.CornflowerBlue,
    //         "Керосин" => Brushes.LightGoldenrodYellow,
    //         "Масло" => Brushes.Olive,
    //         "Парафин" => Brushes.LightGray,
    //         "Спирт" => Brushes.Thistle,
    //         "Ртуть (жид)" => Brushes.Silver,
    //         _ => Brushes.Gray // Цвет по умолчанию
    //     };
    // }
    //
    // private double CalculateBubbleSize(string? volume)
    // {
    //     // Простая логика: парсим число из строки "N литров"
    //     if (volume != null && double.TryParse(volume.Split(' ')[0], NumberStyles.Any, CultureInfo.InvariantCulture,
    //             out var volValue))
    //         // Пример: размер от 10 до 30 в зависимости от объема (от 1 до 10 л)
    //         return 10 + (volValue - 1.0) * (20.0 / 9.0);
    //     return 15; // Размер по умолчанию
    }
}