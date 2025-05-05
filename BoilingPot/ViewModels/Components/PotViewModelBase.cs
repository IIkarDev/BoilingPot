// ViewModels/Components/PotViewModelBase.cs

using System.ComponentModel;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers; // Для [Reactive]

namespace BoilingPot.ViewModels.Components
{
    // Базовый класс для ViewModel кастрюль.
    // Наследуется от нашего общего ViewModelBase и реализует IPotViewModel.
    public class PotViewModelBase: IPotViewModel
    {
        // Используем [Reactive] для свойств интерфейса
        [Reactive] public string? PotVolumeText { get; set; } = "N/A";
        [Reactive] public string? LiquidTypeText { get; set; }
        [Reactive] public IBrush LiquidColor { get; set; } = Brushes.Transparent;
        
    }
}