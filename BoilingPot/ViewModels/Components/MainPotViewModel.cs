using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BoilingPot.ViewModels.Components;

public partial class MainPotViewModel : ViewModelBase, IPotViewModel
{
    [ObservableProperty] private string? _potVolumeText; 
    
    [ObservableProperty] private string? _liquidTypeText;
    
    [ObservableProperty] private SolidColorBrush _liquidColor = new SolidColorBrush(Colors.Transparent);

    public MainPotViewModel()
    {
        PotVolumeText = "6.0 литров";
        LiquidTypeText = "Вода";
        LiquidColor = new SolidColorBrush(Colors.Aqua);
    }
}