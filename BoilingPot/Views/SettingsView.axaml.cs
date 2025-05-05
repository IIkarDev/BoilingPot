// Views/SettingsView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using BoilingPot.ViewModels; // Пространство имен MainViewModel и SettingsViewModel
using ReactiveUI; // Для WhenActivated, Interaction
using System.Reactive.Disposables; // Для CompositeDisposable
using System.Reactive; // Для Unit
using System.Diagnostics; // Для Debug

namespace BoilingPot.Views
{
    // Представление для панели настроек.
    // Наследуется от ReactiveUserControl<TViewModel>.
    public partial class SettingsView : ReactiveUserControl<SettingsViewModel> // DataContext - SettingsViewModel
    {
        public SettingsView()
        {
            InitializeComponent();
            Debug.WriteLine($"[{this.GetType().Name}] View создан.");

            this.WhenActivated(disposables =>
            {
                Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");

                // --- Обработка Interaction запроса закрытия от ViewModel ---
                // Подписываемся на Interaction, который ViewModel отправляет,
                // когда нужно закрыть панель.
                // RegisterHandler выполняется в потоке, где был вызван Handle()
                // (обычно UI поток).
                ViewModel.CloseSettingsInteraction
                    .RegisterHandler(async interaction =>
                    {
                        Debug.WriteLine($"[{this.GetType().Name}] Получен Handle для CloseSettingsInteraction.");
                        // Логика закрытия View (например, скрыть его)
                        // Поскольку этот View отображается через IsVisible,
                        // его родитель (MainWindow) должен отреагировать на изменение
                        // свойства IsShowingSettings в MainViewModel.
                        // Здесь мы просто можем вывести сообщение и сообщить, что обработали.
                         // TODO: Реализовать логику скрытия/закрытия View в родительском ViewModel.
                         // Возможно, нужно вызвать команду в родительском VM через Interaction?
                         // Или просто сообщить, что обработали, и родитель отреагирует на IsShowingSettings.
                        interaction.SetOutput(Unit.Default); // Сообщаем, что запрос обработан
                         Debug.WriteLine($"[{this.GetType().Name}] Обработка CloseSettingsInteraction завершена.");
                    })
                    .DisposeWith(disposables); // Отписываемся при деактивации View

                 Debug.WriteLine($"[{this.GetType().Name}] WhenActivated: Обработчик Interaction для закрытия настроен.");

                Disposable.Create(() => System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН.")).DisposeWith(disposables);
            });
        }
        // Старые методы OnDataContextChanged, OnDetachedFromVisualTree не нужны
    }
}