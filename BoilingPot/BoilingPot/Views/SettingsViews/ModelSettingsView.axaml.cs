// Views/SettingsViews/ModelSettingsView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using BoilingPot.ViewModels.SettingsViewModels; // Пространство имен ViewModel
using ReactiveUI; // Для WhenActivated
using System.Reactive.Disposables; // Для CompositeDisposable
using System.Diagnostics; // Для Debug

namespace BoilingPot.Views.SettingsViews
{
    // Представление для секции настроек моделей.
    // Наследуется от ReactiveUserControl<TViewModel>.
    public partial class ModelSettingsView : ReactiveUserControl<ModelSettingsViewModel> // DataContext - ModelSettingsViewModel
    {
        public ModelSettingsView()
        {
            InitializeComponent();
            Debug.WriteLine($"[{this.GetType().Name}] View создан.");

            this.WhenActivated(disposables =>
            {
                Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");
                // Здесь можно настроить подписки, специфичные для этой секции.
                // Например, подписка на Observable, который отправляет уведомления
                // о статусе загрузки кастомной темы.

                Disposable.Create(() => System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН.")).DisposeWith(disposables);
            });
        }
    }
}