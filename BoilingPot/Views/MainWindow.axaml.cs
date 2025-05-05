// Пример: Views/MainWindow.axaml.cs
using Avalonia.ReactiveUI; // Используем ReactiveWindow
using BoilingPot.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace BoilingPot.Views
{
    // Наследуемся от ReactiveWindow<TViewModel>
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                // Настройка подписок и привязок для окна
                System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");
                Disposable.Create(() => System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН."))
                    .DisposeWith(disposables);
            });
        }
    }
}