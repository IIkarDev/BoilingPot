// Views/MolecularView.axaml.cs
// ... (using как раньше) ...

using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using BoilingPot.ViewModels;
using ReactiveUI;

namespace BoilingPot.Views
{
    public partial class MolecularView : ReactiveUserControl<MolecularViewModel>
    {
        public MolecularView()
        {
            InitializeComponent();
            Debug.WriteLine($"[{this.GetType().Name}] View создан.");

            this.WhenActivated(disposables =>
            {
                Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН.");

                // --- ЖДЕМ, пока ViewModel будет установлен ---
                this.WhenAnyValue(x => x.ViewModel) // Наблюдаем за свойством ViewModel (тип MolecularViewModel)
                    .Where(vm => vm != null)        // Фильтруем, пока ViewModel не станет не-null
                    .Take(1)                        // Берем только первое не-null значение
                    .ObserveOn(RxApp.MainThreadScheduler) // Переключаемся на UI поток
                    // !!! ЯВНО УКАЗЫВАЕМ ТИП ПАРАМЕТРА ЛЯМБДЫ !!!
                    .Subscribe((MolecularViewModel? actualViewModel) => // <--- ИЗМЕНЕНИЕ ЗДЕСЬ
                    {
                        // Проверяем еще раз на null на всякий случай, хотя Where и Take должны это обеспечить
                        if (actualViewModel != null)
                        {
                            Debug.WriteLine($"[{this.GetType().Name}] ViewModel ({actualViewModel.GetType().Name}) установлен и не null. Вызов InitializeAsync.");
                            _ = actualViewModel.InitializeAsync(); // Запускаем асинхронную инициализацию
                        }
                        else
                        {
                            Debug.WriteLine($"!!! [{this.GetType().Name}] Subscribe: actualViewModel равен null, InitializeAsync не вызван.");
                        }
                    })
                    .DisposeWith(disposables); // Отписываемся при деактивации

                Disposable.Create(() =>
                {
                    Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН. Попытка остановить симуляцию.");
                    ViewModel?.StopSimulation(); // Останавливаем симуляцию
                }).DisposeWith(disposables);
            });
        }
    }
}