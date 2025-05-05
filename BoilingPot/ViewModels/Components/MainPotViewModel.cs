// ViewModels/Components/MainPotViewModel.cs
using Avalonia.Media;
using System.Diagnostics;

namespace BoilingPot.ViewModels.Components
{
    // Конкретная реализация ViewModel для основной темы кастрюли.
    public partial class MainPotViewModel : PotViewModelBase // Наследуемся от PotViewModelBase
    {
        public MainPotViewModel()
        {
            // Устанавливаем значения для этой конкретной реализации
            PotVolumeText = "6.0 литров (Main)";
            LiquidTypeText = "Вода";
            LiquidColor = Brushes.Aqua;
            Debug.WriteLine(">>> MainPotViewModel СОЗДАН (RxUI)");
        }
    }
}