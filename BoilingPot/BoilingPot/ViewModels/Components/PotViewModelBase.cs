// ViewModels/Components/PotViewModelBase.cs
using Avalonia; // Для Point, Size, Rect
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace BoilingPot.ViewModels.Components
{
    public partial class PotViewModelBase : ViewModelBase, IPotViewModel
    {
        [Reactive] public double Left { get; set; }
        [Reactive] public double Top { get; set; }

        // --- Новые свойства ---
        [Reactive] public double CapWidth { get; set; } = 150;  // Ширина Canvas крышки (из темы)
        [Reactive] public double CapHeight { get; set; } = 40;   // Высота Canvas крышки (из темы)

        // Размеры родительского Canvas, в котором находится PotPresenter
        // Эти значения должны устанавливаться извне, например, из ViewModel,
        // которая знает размеры области симуляции.
        [Reactive] public double ParentCanvasWidth { get; set; } = 840; // Пример
        [Reactive] public double ParentCanvasHeight { get; set; } = 600; // Пример

        // Координаты "магнитов"
        // Для кастрюли: верхний центр кастрюли (относительно PotPresenter)
        [Reactive] public Point PotSnapTarget { get; set; } = new Point(346, 40); // (Canvas.Left="25" Canvas.Top="35" у крышки в теме)

        // Для полки: середина полки (координаты полки должны быть известны)
        // Эти координаты должны быть ОТНОСИТЕЛЬНО родительского Canvas,
        // в котором лежит и PotPresenter, и ShelfView.
        [Reactive] public Point ShelfSnapTarget { get; set; } = new Point(300, 50); // Пример

        [Reactive] public string? PotVolumeText { get; set; } = "N/A";
        [Reactive] public string? LiquidTypeText { get; set; }
        [Reactive] public IBrush LiquidColor { get; set; } = Brushes.Transparent;
        [Reactive] public IBrush LiquidBorder { get; set; } = Brushes.Transparent;

        public PotViewModelBase()
        {
            // Устанавливаем начальное положение крышки на кастрюле
            Left = PotSnapTarget.X;
            Top = PotSnapTarget.Y;
        }
    }
}