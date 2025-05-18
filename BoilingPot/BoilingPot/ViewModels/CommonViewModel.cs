using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using BoilingPot.ViewModels.Components;
using BoilingPot.ViewModels.SettingsViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace BoilingPot.ViewModels;

public class CommonViewModel : ViewModelBase
{

    private const double SimCanvasWidth = 840;
    private const double SimCanvasHeight = 400;

    // Параметры полки (соответствуют ShelfView в XAML)
    private const double ShelfX = 600;
    private const double ShelfY = 64;
    private const double ShelfWidth = 200; // Ширина из ShelfView.axaml
    public ModelSettingsViewModel ModelVm { get; set; }
    
    private readonly IServiceProvider _serviceProvider;
    // Команда для обновления текста объема кастрюли (пример)
    // Может быть привязана к кнопке на этом экране

    // Конструктор CommonViewModel.
    // IServiceProvider и ModelSettingsViewModel внедряются через DI.
    public CommonViewModel(IServiceProvider serviceProvider, ModelSettingsViewModel modelSettingsViewModel)
    {
            _serviceProvider = serviceProvider;

            ModelVm = modelSettingsViewModel;
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Начало создания.");

            // Проверяем PotViewModelInstance
            if (ModelVm?.PotViewModelInstance != null)
                Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: ModelVm.PotViewModelInstance НЕ null.");
            else
                Debug.WriteLine(
                    $"!!! ПРЕДУПРЕЖДЕНИЕ: [{GetType().Name}] Конструктор RxUI: ModelVm или PotViewModelInstance равен null!");

            // Инициализация команд
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Завершение создания.");


            if (ModelVm.PotViewModelInstance != null)
            {
                Debug.WriteLine($"[{this.GetType().Name}] Конструктор: Инициализация параметров PotViewModelInstance.");
                // Устанавливаем размеры родительского Canvas
                ModelVm.PotViewModelInstance.ParentCanvasWidth = SimCanvasWidth;
                ModelVm.PotViewModelInstance.ParentCanvasHeight = SimCanvasHeight;

                // Рассчитываем и устанавливаем ShelfSnapTarget
                // Центрируем крышку на полке
                double capWidth = ModelVm.PotViewModelInstance.CapWidth; // Берем из VM кастрюли
                // double capHeight = ModelVm.PotViewModelInstance.CapHeight; // Если нужно для вертикального центрирования

                ModelVm.PotViewModelInstance.ShelfSnapTarget = new Point(
                    ShelfX + (ShelfWidth - capWidth) / 2,
                    ShelfY // Помещаем на верхний край полки
                           // или ShelfY + (ShelfHeight - capHeight) / 2 для центрирования по высоте
                );
                Debug.WriteLine($"[{this.GetType().Name}] Конструктор: ShelfSnapTarget установлен в {ModelVm.PotViewModelInstance.ShelfSnapTarget}");

                // PotSnapTarget уже установлен в конструкторе PotViewModelBase
                 Debug.WriteLine($"[{this.GetType().Name}] Конструктор: PotSnapTarget из VM: {ModelVm.PotViewModelInstance.PotSnapTarget}");
            }
            else { Debug.WriteLine($"!!! ПРЕДУПРЕЖДЕНИЕ: [{this.GetType().Name}] Конструктор: ModelVm.PotViewModelInstance РАВЕН null!"); }

            Debug.WriteLine($"[{this.GetType().Name}] Конструктор: Завершение создания.");
        }


    // TODO: Добавить свойства и команды для управления симуляцией (скорость, нагрев, запуск/остановка)
    // Эти свойства могут быть привязаны к элементам UI на экране симуляции (CommonView).
    // Возможно, эти свойства (ProcessSpeed, FlameLevel и т.п.) лучше хранить в этом VM,
    // а ModelSettingsViewModel будет просто показывать/менять их значения.


    public Task UpdatePotVolumeText(string volume)
    {
        Debug.WriteLine(
            $"[{GetType().Name}] UpdatePotVolumeText: Попытка установить PotVolumeText = '{volume ?? "null"}'");
    
            ModelVm.PotViewModelInstance = _serviceProvider.GetRequiredService<PotViewModelBase>();
            // Обновляем свойство в PotViewModelInstance
            ModelVm.PotViewModelInstance.PotVolumeText = volume ?? "N/A";
            Debug.WriteLine(
                $"[{GetType().Name}] UpdatePotVolumeText: Установлено значение ModelVm.PotViewModelInstance.PotVolumeText.");
            return Task.CompletedTask;
    }
    public Task UpdateLiquidType(string liquidType)
    {
        Debug.WriteLine(
            $"[{GetType().Name}] UpdatePotVolumeText: Попытка установить PotVolumeText = '{liquidType ?? "null"}'");
    
        ModelVm.PotViewModelInstance = _serviceProvider.GetRequiredService<PotViewModelBase>();
        // Обновляем свойство в PotViewModelInstance
        ModelVm.PotViewModelInstance.LiquidColor = liquidType switch
        {
            "Вода" => Brushes.Aqua, // Или Brushes.CornflowerBlue
            "Масло (раст.)" => Brushes.Olive,
            "Парафин" => Brushes.AntiqueWhite,
            "Керосин" => Brushes.LightGoldenrodYellow,
            "Спирт" => Brushes.Thistle,
            "Ртуть (жид.)" => Brushes.Silver,
            _ => Brushes.Transparent// Цвет по умолчанию или для неизвестного типа
        };
        ModelVm.PotViewModelInstance.LiquidBorder = Brushes.Gray;
        
        Debug.WriteLine(
            $"[{GetType().Name}] UpdatePotVolumeText: Установлено значение ModelVm.PotViewModelInstance.PotVolumeText.");
        return Task.CompletedTask;
    }
}