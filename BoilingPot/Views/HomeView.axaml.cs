// Views/HomeView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using BoilingPot.ViewModels; // Пространство имен ViewModel
using ReactiveUI; // Для WhenActivated
using System.Reactive.Disposables; // Для CompositeDisposable

namespace BoilingPot.Views
{
    // Представление для стартового экрана.
    // Наследуется от ReactiveUserControl<TViewModel>.
    public partial class HomeView : ReactiveUserControl<HomeViewModel> // DataContext - HomeViewModel
    {
        public HomeView()
        {
            InitializeComponent();

            // WhenActivated вызывается, когда View становится активным в визуальном дереве
            // (например, при первом отображении или когда окно становится видимым).
            // Используется для настройки подписок, которые должны жить вместе с View.
            this.WhenActivated(disposables =>
            {
                // Подписки и логика, специфичная для HomeView при его активации.
                // В данном случае, возможно, ничего особого не нужно делать здесь,
                // т.к. вся логика навигации в MainViewModel.
                // Но если бы HomeViewModel имел Observables для UI, подписки были бы здесь.

                 // Пример: Подписка на Observable из HomeViewModel
                 // ViewModel.SomeObservable.Subscribe(...).DisposeWith(disposables);

                 // Код очистки (отписки) при деактивации View
                 // (Когда View становится невидимым или удаляется из дерева)
                 System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");
                 Disposable.Create(() => System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН.")).DisposeWith(disposables);
            });
        }

        // Старый обработчик DataContextChanged не нужен с ReactiveUserControl
        // private void OnDataContextChanged(object? sender, EventArgs e) { ... }
        // Метод OnDetachedFromVisualTree не нужен для отписки от WhenActivated
        // protected override void OnDetachedFromVisualTree(...) { ... }
    }
}