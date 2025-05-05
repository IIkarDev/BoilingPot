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
        // Команды для кнопок на стартовом экране.
        // private readonly IServiceProvider _serviceProvider;
        // // ReactiveCommand<TParam, TResult>
        // // Unit используется, когда команда не принимает параметров или не возвращает результат.
        // public ReactiveCommand<Unit, IDisposable> StartCommand { get; }
        // public ReactiveCommand<Unit, IDisposable> LoadFileCommand { get; }
        // public ReactiveCommand<Unit, IDisposable> ShowSettingsCommand { get; }
        // public ReactiveCommand<Unit, IDisposable> ShowAboutCommand { get; }
        // public ReactiveCommand<Unit, IDisposable> ExitApplicationCommand { get; }
        //
        // // Ссылка на главный ViewModel для вызова его команд навигации/действий.
        // // Она может быть передана через конструктор или получена через Service Location (менее предпочтительно).
        // // Для простоты предположим, что команды этого VM будут вызывать команды MainViewModel.
        // // Более правильный подход - использовать Interaction или сервис сообщений.
        // private readonly MainViewModel _mainViewModel; // !!! Важно: MainViewModel должен быть Singleton !!!

        public HomeViewModel(IServiceProvider serviceProvider) // Получаем MainViewModel через DI
        {
            // _serviceProvider = serviceProvider;
            // _mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            // System.Diagnostics.Debug.WriteLine(">>> HomeViewModel СОЗДАН (RxUI)");
            //
            // // --- Инициализация команд ---
            // // Создаем команды, которые просто вызывают соответствующие команды в MainViewModel.
            // // CanExecute можно привязать к CanExecute у команд MainViewModel, если нужно.
            // StartCommand = ReactiveCommand.Create(() => _mainViewModel.GoToCommonCommand.Execute().Subscribe());
            // LoadFileCommand = ReactiveCommand.Create(() =>  _mainViewModel.LoadFileCommand.Execute().Subscribe());
            // ShowSettingsCommand = ReactiveCommand.Create(() => _mainViewModel.ShowSettingsCommand.Execute().Subscribe());
            // ShowAboutCommand = ReactiveCommand.Create(() => _mainViewModel.ShowAboutCommand.Execute().Subscribe());
            // ExitApplicationCommand = ReactiveCommand.Create(() => _mainViewModel.ExitApplicationCommand.Execute().Subscribe());

            // Вызов .Subscribe() после .Execute() важен для немедленного выполнения команды RxUI.
        }
    }
}