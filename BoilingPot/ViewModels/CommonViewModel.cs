// ViewModels/CommonViewModel.cs
using BoilingPot.ViewModels.SettingsViewModels; // Для ModelSettingsViewModel
using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider
using ReactiveUI; // Базовый класс RxUI
using ReactiveUI.Fody.Helpers; // Для [Reactive]
using System;
using System.Diagnostics; // Для Debug
using System.Reactive; // Для Unit
using System.Reactive.Linq; // Для WhenAnyValue

namespace BoilingPot.ViewModels
{
    // ViewModel для основного экрана симуляции (где отображается кастрюля и плита).
    // Управляет общими данными и логикой, не связанной с конкретными моделями или настройками.
    public partial class CommonViewModel : ViewModelBase
    {
        // Ссылка на ViewModel настроек моделей.
        // Получается через DI. CommonView привязывает свой DataContext к этому VM,
        // чтобы иметь доступ к PotViewModelInstance, StoveViewModelInstance и т.д.
        public ModelSettingsViewModel ModelVm { get; }

        // Команда для обновления текста объема кастрюли (пример)
        // Может быть привязана к кнопке на этом экране
        public ReactiveCommand<string?, Unit> UpdatePotVolumeCommand { get; }


        // Конструктор CommonViewModel.
        // IServiceProvider и ModelSettingsViewModel внедряются через DI.
        public CommonViewModel(IServiceProvider serviceProvider, ModelSettingsViewModel modelSettingsViewModel)
        {
            // Получаем ModelSettingsViewModel из DI.
            // Так как он является Singleton в App.axaml.cs, мы получим тот же экземпляр,
            // который используется в SettingsViewModel.
            ModelVm = modelSettingsViewModel;
            Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: Начало создания.");

            // Проверяем PotViewModelInstance
            if (ModelVm?.PotViewModelInstance != null)
            {
                Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: ModelVm.PotViewModelInstance НЕ null.");
            }
            else
            {
                 Debug.WriteLine($"!!! ПРЕДУПРЕЖДЕНИЕ: [{this.GetType().Name}] Конструктор RxUI: ModelVm или PotViewModelInstance равен null!");
            }

             // Инициализация команд
             UpdatePotVolumeCommand = ReactiveCommand.Create<string?>(ExecuteUpdatePotVolume);

             Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: Завершение создания.");
        }

        // Метод для команды UpdatePotVolumeCommand
        private void ExecuteUpdatePotVolume(string? volume)
        {
             Debug.WriteLine($"[{this.GetType().Name}] ExecuteUpdatePotVolume: Попытка установить PotVolumeText = '{volume ?? "null"}'");
            if (ModelVm?.PotViewModelInstance != null)
            {
                // Обновляем свойство в PotViewModelInstance
                ModelVm.PotViewModelInstance.PotVolumeText = volume ?? "N/A";
                 Debug.WriteLine($"[{this.GetType().Name}] ExecuteUpdatePotVolume: Установлено значение ModelVm.PotViewModelInstance.PotVolumeText.");
            }
            else
            {
                Debug.WriteLine($"!!! ПРЕДУПРЕЖДЕНИЕ: [{this.GetType().Name}] ExecuteUpdatePotVolume: ModelVm или PotViewModelInstance равен null. Значение не установлено.");
            }
        }

        // TODO: Добавить свойства и команды для управления симуляцией (скорость, нагрев, запуск/остановка)
        // Эти свойства могут быть привязаны к элементам UI на экране симуляции (CommonView).
        // Возможно, эти свойства (ProcessSpeed, FlameLevel и т.п.) лучше хранить в этом VM,
        // а ModelSettingsViewModel будет просто показывать/менять их значения.

        // Пример свойств симуляции (можно перенести сюда из ControlPanelViewModel, если нужно)
        // [Reactive] public double ProcessSpeed { get; set; } = 1.0;
        // [Reactive] public double FlameLevel { get; set; } = 1.0;
    }
}