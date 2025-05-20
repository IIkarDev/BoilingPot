// Views/SettingsViews/GeneralSettingsView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using BoilingPot.ViewModels.SettingsViewModels; // Пространство имен ViewModel
using ReactiveUI; // Для WhenActivated
using System.Reactive.Disposables; // Для CompositeDisposable
using System.Diagnostics; // Для Debug

namespace BoilingPot.Views.SettingsViews
{
    /// <summary>
    /// Представление для секции "Общие настройки".
    /// Наследуется от ReactiveUserControl<TViewModel> для интеграции с ReactiveUI.
    /// </summary>
    public partial class GeneralSettingsView : ReactiveUserControl<GeneralSettingsViewModel>
    {
        public GeneralSettingsView()
        {
            InitializeComponent(); // Загрузка XAML и инициализация элементов
            Debug.WriteLine($"[{GetType().Name}] View создан. HashCode: {GetHashCode()}");

            // WhenActivated - механизм ReactiveUI для выполнения кода при активации
            // (когда View добавляется в визуальное дерево) и его очистки при деактивации.
            this.WhenActivated(disposables =>
            {
                Debug.WriteLine($"[{GetType().Name}] View АКТИВИРОВАН. HashCode: {GetHashCode()}");

                // Здесь можно настроить привязки команд, которые не удалось сделать в XAML,
                // или подписаться на Observables из ViewModel.
                // Например:
                // this.BindCommand(ViewModel, vm => vm.SomeCommand, v => v.MyButtonInXaml)
                //     .DisposeWith(disposables);
                //
                // ViewModel.SomeObservable.Subscribe(value => { /* Do something */ })
                //     .DisposeWith(disposables);


                // Добавляем действие, которое выполнится при деактивации View
                // (например, отписка от событий, освобождение ресурсов).
                // DisposeWith(disposables) гарантирует, что это произойдет автоматически.
                Disposable.Create(() => Debug.WriteLine($"[{GetType().Name}] View ДЕАКТИВИРОВАН. HashCode: {GetHashCode()}"))
                    .DisposeWith(disposables);
            });
        }
    }
}