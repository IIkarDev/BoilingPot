using Avalonia.Media;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BoilingPot.ViewModels.Components;

public partial class AltPotViewModel : ViewModelBase, IPotViewModel
{
    [ObservableProperty] private string? _potVolumeText; 
    
    [ObservableProperty] private string? _liquidTypeText;
    
    [ObservableProperty] private SolidColorBrush _liquidColor;

    public AltPotViewModel()
    {
        PotVolumeText = "6.0 литров";
        LiquidTypeText = "Вода";
        LiquidColor = new SolidColorBrush(Colors.Blue);
    }
}