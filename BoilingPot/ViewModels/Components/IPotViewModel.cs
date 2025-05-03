// <WORKDIR>/ViewModels/Components/IPotViewModel.cs

using Avalonia.Media;

// Добавьте, если будете включать LiquidColor

namespace BoilingPot.ViewModels.Components
{
    public interface IPotViewModel
    {
        string? PotVolumeText { get; set; }
        // При желании добавьте другие общие свойства:
        string? LiquidTypeText { get; set; }
        SolidColorBrush LiquidColor { get; set; }
    }
}