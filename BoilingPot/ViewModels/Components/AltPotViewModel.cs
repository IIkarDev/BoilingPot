// ViewModels/Components/AltPotViewModel.cs
using Avalonia.Media;
using System.Diagnostics;

namespace BoilingPot.ViewModels.Components
{
    // Конкретная реализация ViewModel для альтернативной темы кастрюли.
    public partial class AltPotViewModel : PotViewModelBase // Наследуемся от PotViewModelBase
    {
        public AltPotViewModel()
        {
            PotVolumeText = "3.5 литра (Alt)";
            LiquidTypeText = "Керосин";
            LiquidColor = Brushes.LightGoldenrodYellow;
            Debug.WriteLine(">>> AltPotViewModel СОЗДАН (RxUI)");
        }
    }
}