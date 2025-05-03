// ViewModels/Components/IPotViewModel.cs
using Avalonia.Media; // Для IBrush, если нужен цвет
using CommunityToolkit.Mvvm.ComponentModel; // Для ObservableObject или просто как маркер

namespace BoilingPot.ViewModels.Components
{
    // Используем ObservableObject, чтобы наследники могли легко реализовывать INPC
    public partial class PotViewModelBase : ViewModelBase, IPotViewModel
    {
        [ObservableProperty]
        private string? _potVolumeText = "N/A"; // Свойство для текста объема

        // Можно добавить другие общие свойства (цвет жидкости и т.д.)
        [ObservableProperty]
        private string? _liquidTypeText;

        [ObservableProperty]
        private SolidColorBrush _liquidColor = new SolidColorBrush(Colors.Transparent);
    }

}