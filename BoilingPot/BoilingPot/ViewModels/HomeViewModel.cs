// ViewModels/HomeViewModel.cs
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using Avalonia.Automation.Peers;
using Microsoft.Extensions.DependencyInjection; // Для Unit

namespace BoilingPot.ViewModels
{
    // ViewModel для стартового экрана.
    public class HomeViewModel : ViewModelBase
    {

        public HomeViewModel(IServiceProvider serviceProvider) // Получаем MainViewModel через DI
        {
        }
    }
}