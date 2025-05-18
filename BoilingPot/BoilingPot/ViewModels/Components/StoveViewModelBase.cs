// ViewModels/Components/PotViewModelBase.cs

using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers; // Для [Reactive]

namespace BoilingPot.ViewModels.Components
{
    // Базовый класс для ViewModel кастрюль.
    // Наследуется от нашего общего ViewModelBase и реализует IPotViewModel.
    public class StoveViewModelBase : IStoveViewModel
    {
        // Используем [Reactive] для свойств интерфейса
        [Reactive] public int FlameStrength { get; set; } = 0;

        public StoveViewModelBase()
        {
        }
    }
}