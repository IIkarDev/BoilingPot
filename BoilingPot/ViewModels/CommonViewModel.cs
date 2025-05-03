//
// // ViewModels/CommonViewModel.cs
// using BoilingPot.ViewModels.SettingsViewModels;
// using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider
// using System;
//
// namespace BoilingPot.ViewModels
// {
//     public partial class CommonViewModel : ViewModelBase
//     {
//         // Ссылка на ViewModel настроек моделей, который содержит нужный PotViewModelInstance
//         public ModelSettingsViewModel ModelVm { get; }
//
//         // Конструктор получает ModelSettingsViewModel через DI
//         public CommonViewModel(IServiceProvider serviceProvider)
//         {
//             ModelVm = serviceProvider.GetRequiredService<ModelSettingsViewModel>();
//             System.Diagnostics.Debug.WriteLine(">>> CommonViewModel СОЗДАН");
//         }
//
//         // Метод для обновления PotVolumeText больше не нужен здесь,
//         // так как View привязан напрямую к PotViewModelInstance в ModelVm.
//         // Обновлять PotVolumeText нужно в самом PotViewModelInstance.
//         // public void UpdatePotVolume(string? volume) { ... }
//
//         // ... остальной код CommonViewModel, если есть ...
//
//         public void UpdatePotVolume(string? volume)
//         {
//             // Обновляем свойство в MainPotViewModel
//             ModelVm.PotViewModelInstance.PotVolumeText = volume ?? "N/A";
//         }
//     }
// }
// ViewModels/CommonViewModel.cs
using BoilingPot.ViewModels.SettingsViewModels;
using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider
using System;
using System.Diagnostics; // <<< Добавляем using для Debug !!!

namespace BoilingPot.ViewModels
{
    // Этот ViewModel отвечает за отображение основного экрана симуляции,
    // который включает в себя кастрюлю (через PotPresenter) и плиту.
    public partial class CommonViewModel : ViewModelBase
    {
        // Свойство (только для чтения), хранящее ссылку на ViewModel настроек моделей.
        // Это позволяет CommonView получить доступ к PotViewModelInstance, который
        // находится внутри ModelSettingsViewModel, для привязки DataContext у PotPresenter.
        public ModelSettingsViewModel ModelVm { get; }

        // Конструктор CommonViewModel.
        // IServiceProvider внедряется сюда DI контейнером.
        public CommonViewModel(IServiceProvider serviceProvider)
        {
            Debug.WriteLine($"[{this.GetType().Name}] Конструктор: Начало создания.");

            // Запрашиваем у DI контейнера ЭКЗЕМПЛЯР ModelSettingsViewModel.
            // Так как ModelSettingsViewModel, скорее всего, зарегистрирован как Transient,
            // здесь будет создан НОВЫЙ экземпляр ModelSettingsViewModel (если он не был создан ранее
            // в рамках того же запроса). Если он Singleton, будет возвращен единственный экземпляр.
            // ВАЖНО: Убедитесь, что ModelSettingsViewModel зарегистрирован в App.axaml.cs!
            ModelVm = serviceProvider.GetRequiredService<ModelSettingsViewModel>();

            // Проверяем, успешно ли получен ModelSettingsViewModel
            if (ModelVm != null)
            {
                Debug.WriteLine($"[{this.GetType().Name}] Конструктор: ModelSettingsViewModel успешно получен (тип: {ModelVm.GetType().Name}).");

                // Проверяем вложенный PotViewModelInstance для полноты картины
                if (ModelVm.PotViewModelInstance != null)
                {
                     Debug.WriteLine($"[{this.GetType().Name}] Конструктор: ModelVm.PotViewModelInstance НЕ null (тип: {ModelVm.PotViewModelInstance.GetType().Name}).");
                }
                else
                {
                     Debug.WriteLine($"!!! ПРЕДУПРЕЖДЕНИЕ: [{this.GetType().Name}] Конструктор: ModelVm.PotViewModelInstance РАВЕН null!");
                     // Это может произойти, если в конструкторе ModelSettingsViewModel возникла ошибка
                     // до присваивания PotViewModelInstance.
                }
            }
            else
            {
                 Debug.WriteLine($"!!! КРИТИЧЕСКАЯ ОШИБКА: [{this.GetType().Name}] Конструктор: Не удалось получить ModelSettingsViewModel из DI контейнера!");
                 // Это указывает на проблему с регистрацией ModelSettingsViewModel в App.axaml.cs
                 // или на ошибку в его конструкторе.
            }

            Debug.WriteLine($"[{this.GetType().Name}] Конструктор: Завершение создания.");
        }

        // Этот метод больше не нужен в CommonViewModel, если View (PotPresenter)
        // напрямую привязан к PotViewModelInstance внутри ModelVm.
        // Обновление текста должно происходить в ModelVm.PotViewModelInstance.
        // Оставляю его закомментированным на случай, если он используется где-то еще.
        
        public void UpdatePotVolume(string? volume)
        {
            Debug.WriteLine($"[{this.GetType().Name}] UpdatePotVolume: Попытка установить PotVolumeText = '{volume ?? "null"}'");
            if (ModelVm?.PotViewModelInstance != null)
            {
                ModelVm.PotViewModelInstance.PotVolumeText = volume ?? "N/A";
                 Debug.WriteLine($"[{this.GetType().Name}] UpdatePotVolume: Установлено значение ModelVm.PotViewModelInstance.PotVolumeText.");
            }
            else
            {
                Debug.WriteLine($"!!! ПРЕДУПРЕЖДЕНИЕ: [{this.GetType().Name}] UpdatePotVolume: ModelVm или PotViewModelInstance равен null. Значение не установлено.");
            }
        }
        
        // Вызываем инициализацию ModelVm ПОСЛЕ его создания.
        // Важно: Это нужно делать там, где гарантировано, что View (CommonView)
        // уже создан и его DataContext установлен (чтобы подписка в View сработала).
        // Вызов прямо здесь может быть все еще слишком ранним.
        // Лучше вызывать из CodeBehind View после установки DataContext.
        // await ModelVm.InitializeAsync(); // НЕ ЗДЕСЬ
        
        // ... остальной код CommonViewModel, если есть ...
        // Например, свойства и команды, специфичные для экрана симуляции,
        // но не связанные напрямую с внешним видом кастрюли.
    }
}