// Views/AboutView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using BoilingPot.ViewModels; // Пространство имен ViewModel
using ReactiveUI; // Для WhenActivated
using System.Reactive.Disposables; // Для CompositeDisposable

namespace BoilingPot.Views
{
    // Представление для панели "О программе".
    // Наследуется от ReactiveUserControl<TViewModel>.
    public partial class AboutView : ReactiveUserControl<AboutViewModel> // DataContext - MainViewModel
    {
        public AboutView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                // Логика активации/деактивации AboutView.
                // В данном случае, вероятно, ничего сложного здесь не нужно.

                System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");
                Disposable.Create(() => System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН."))
                    .DisposeWith(disposables);
            });
        }

        // Старые методы OnDataContextChanged, OnDetachedFromVisualTree не нужны
    }
}