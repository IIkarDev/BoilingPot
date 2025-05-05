// Views/SettingsViews/GeneralSettingsView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using BoilingPot.ViewModels.SettingsViewModels; // Пространство имен ViewModel
using ReactiveUI; // Для WhenActivated
using System.Reactive.Disposables; // Для CompositeDisposable
using System.Diagnostics; // Для Debug

namespace BoilingPot.Views.SettingsViews
{
    // Представление для секции общих настроек.
    // Наследуется от ReactiveUserControl<TViewModel>.
    public partial class GeneralSettingsView : ReactiveUserControl<GeneralSettingsViewModel> // DataContext - GeneralSettingsViewModel
    {
        public GeneralSettingsView()
        {
            InitializeComponent();
            Debug.WriteLine($"[{this.GetType().Name}] View создан.");

            this.WhenActivated(disposables =>
            {
                Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");
                // Здесь можно настроить подписки, специфичные для этой секции.
                // Например, подписка на Observable, который отправляет уведомления о применении настроек.

                Disposable.Create(() => System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН.")).DisposeWith(disposables);
            });
        }
    }
}