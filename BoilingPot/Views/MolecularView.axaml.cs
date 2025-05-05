// Views/MolecularView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using BoilingPot.ViewModels; // Пространство имен MolecularViewModel
using ReactiveUI; // Для WhenActivated
using System.Reactive.Disposables; // Для CompositeDisposable
using System.Diagnostics; // Для Debug
using System.Reactive.Linq; // Для Where, Select

namespace BoilingPot.Views
{
    // Представление для экрана молекулярной симуляции.
    // Наследуется от ReactiveUserControl<TViewModel>.
    public partial class MolecularView : ReactiveUserControl<MolecularViewModel>
    {
        public MolecularView()
        {
            InitializeComponent();
            Debug.WriteLine($"[{this.GetType().Name}] View создан.");

            // WhenActivated вызывается при активации View в визуальном дереве
            // (т.е. когда View становится видимым на экране).
            // Здесь мы настраиваем подписки или вызываем методы инициализации.
            this.WhenActivated(disposables =>
            {
                Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");

                // Проверяем, что ViewModel доступен и имеет метод InitializeAsync
                // (ReactiveUserControl гарантирует, что ViewModel не null в WhenActivated)
                if (ViewModel != null)
                {
                    // Вызываем асинхронный метод инициализации ViewModel
                    // (например, чтобы сгенерировать пузырьки и запустить таймер)
                    // Вызов без await, т.к. мы в лямбде активации
                     // TODO: Получить начальные значения скорости/нагрева из настроек (например, из Singleton сервиса)
                     // Пока используем заглушки 1.0
                     _ = ViewModel.InitializeAsync(1.0, 1.0); // Запускаем инициализацию

                     Debug.WriteLine($"[{this.GetType().Name}] WhenActivated: Вызван ViewModel.InitializeAsync().");
                }
                else
                {
                     Debug.WriteLine($"[{this.GetType().Name}] WhenActivated: ViewModel РАВЕН null!");
                }


                // Код очистки (отписки) выполняется автоматически при деактивации View
                 // (Когда View становится невидимым или удаляется из дерева)
                 Disposable.Create(() => System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН.")).DisposeWith(disposables);
            });
        }

        // Старый обработчик DataContextChanged не нужен с ReactiveUserControl
        // private void OnDataContextChanged(object? sender, EventArgs e) { ... }
        // Метод OnDetachedFromVisualTree тоже не нужен для отписки от WhenActivated
        // protected override void OnDetachedFromVisualTree(...) { ... }
    }
}