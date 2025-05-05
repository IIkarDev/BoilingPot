// ViewModels/Components/BubbleViewModel.cs
using Avalonia.Media;
using ReactiveUI; // Базовый класс RxUI
using ReactiveUI.Fody.Helpers; // Для [Reactive]
using System;
using System.Diagnostics;

namespace BoilingPot.ViewModels.Components
{
    // ViewModel для одного "пузырька" в молекулярной симуляции.
    // Содержит только данные о состоянии одного пузырька.
    public partial class BubbleViewModel : ViewModelBase
    {
        // Используем [Reactive] для всех свойств,
        // которые могут меняться во время симуляции и должны обновлять View.
        [Reactive] public double X { get; set; } // Позиция по X (для Canvas.Left)
        [Reactive] public double Y { get; set; } // Позиция по Y (для Canvas.Top)
        [Reactive] public double Size { get; set; } // Размер пузырька (диаметр)
        [Reactive] public IBrush ColorBrush { get; set; } = Brushes.CornflowerBlue; // Цвет заливки

        // Параметры для анимации (если XAML-анимация будет использоваться)
        // В нашем случае мы анимируем позицию X/Y в C# таймером,
        // но эти свойства могут использоваться для других видов анимации
        // или для передачи параметров в XAML-шаблон пузырька.
        [Reactive] public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromSeconds(5); // Пример
        [Reactive] public double HorizontalShift { get; set; } = 0; // Пример

        // Конструктор
        public BubbleViewModel()
        {
            Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: Начало.");
            // Установка значений по умолчанию или получение через DI (если нужен сервис в BubbleVM)
            Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: Завершение.");
        }
    }
}