// ViewModels/HomeViewModel.cs

using System;

// Для Unit

namespace BoilingPot.ViewModels;

// ViewModel для стартового экрана.
public class HomeViewModel : ViewModelBase
{
    public HomeViewModel(IServiceProvider serviceProvider) // Получаем MainViewModel через DI
    {
    }
}