// ViewModels/Components/IPotViewModel.cs
using Avalonia.Media;
using System.ComponentModel;
using ReactiveUI; // Для INotifyPropertyChanged

namespace BoilingPot.ViewModels.Components
{
    // Интерфейс для ViewModel кастрюли.
    // Явно наследуем INotifyPropertyChanged, т.к. ReactiveObject его реализует.
    public interface IPotViewModel
    {
        string? PotVolumeText { get; set; }
        string? LiquidTypeText { get; set; }
        IBrush LiquidColor { get; set; }
        // Добавьте другие свойства, если они используются в шаблонах тем
    }
}