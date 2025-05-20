// ViewModels/CommonViewModel.cs

// --- Подключение необходимых пространств имен ---

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using BoilingPot.ViewModels.SettingsViewModels;
// Для базовых типов, таких как IServiceProvider и Random (если бы он здесь использовался)
// Для вывода отладочной информации (Debug.WriteLine)
// Для асинхронных операций (Task, Task.CompletedTask)
// Для Point (структура для координат)
// Для Brushes и цветов (IBrush)
// Пространство имен, где лежат IPotViewModel и PotViewModelBase
// Пространство имен для ModelSettingsViewModel

// Для IServiceProvider и метода GetRequiredService
// using ReactiveUI; // Если бы использовался ReactiveCommand или [Reactive] напрямую в этом классе
// using ReactiveUI.Fody.Helpers; // Если бы использовался [Reactive]

// --- Основное пространство имен для ViewModel ---
namespace BoilingPot.ViewModels;

// CommonViewModel отвечает за логику основного экрана симуляции.
// Он агрегирует или получает доступ к другим ViewModel, таким как ModelSettingsViewModel,
// для управления отображением и состоянием компонентов симуляции (кастрюля, плита).
public class CommonViewModel : ViewModelBase // Наследование от ViewModelBase (предположительно, ReactiveObject)
{
    // --- Константы и Поля ---

    #region Константы Размеров и Позиций

    // Эти константы определяют размеры симуляционной области и начальные параметры
    // для позиционирования элементов, таких как полка.
    // Они используются для расчета точек "примагничивания" и ограничений.

    private const double SimCanvasWidth = 840; // Ширина основного Canvas для симуляции
    private const double SimCanvasHeight = 400; // Высота основного Canvas для симуляции

    // Параметры полки (координаты и размеры, как они заданы в XAML)
    private const double ShelfX = 600; // Координата X левого верхнего угла полки
    private const double ShelfY = 64; // Координата Y левого верхнего угла полки

    private const double ShelfWidth = 200; // Ширина полки (из ShelfView.axaml)
    // private const double ShelfHeight = 20; // Высота полки (если нужна для вертикального центрирования)

    #endregion

    #region Поля Зависимостей

    // Ссылка на IServiceProvider, полученная через DI.
    // Используется для получения других зарегистрированных сервисов или ViewModel.
    private readonly IServiceProvider _serviceProvider;

    #endregion

    // --- Свойства ---

    #region Публичные Свойства (Доступные для View)

    // Свойство для хранения ModelSettingsViewModel.
    // ModelSettingsViewModel содержит PotViewModelInstance и логику управления темами моделей.
    // CommonView будет привязывать DataContext своего PotPresenter к ModelVm.PotViewModelInstance.
    public ModelSettingsViewModel ModelVm { get; set; }

    #endregion

    // --- Конструктор ---

    // Конструктор CommonViewModel.
    // IServiceProvider и ModelSettingsViewModel внедряются сюда DI контейнером
    // при создании экземпляра CommonViewModel.
    public CommonViewModel(IServiceProvider serviceProvider, ModelSettingsViewModel modelSettingsViewModel)
    {
        // Сохраняем полученные зависимости в приватных полях.
        _serviceProvider = serviceProvider;
        ModelVm = modelSettingsViewModel; // ModelSettingsViewModel теперь внедряется напрямую

        Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Начало создания.");

        // Проверяем, что ModelSettingsViewModel и вложенный PotViewModelInstance успешно получены/созданы.
        // Это важно для отладки и понимания, что DI и инициализация ViewModel проходят корректно.
        if (ModelVm != null)
        {
            if (ModelVm.PotViewModelInstance != null)
            {
                Debug.WriteLine(
                    $"[{GetType().Name}] Конструктор RxUI: ModelVm.PotViewModelInstance НЕ null (тип: {ModelVm.PotViewModelInstance.GetType().Name}).");

                // --- Инициализация параметров PotViewModelInstance ---
                // Устанавливаем размеры родительского Canvas (области симуляции)
                // в PotViewModelInstance. Эти значения будут использоваться
                // в PotPresenter для ограничения движения крышки.
                Debug.WriteLine($"[{GetType().Name}] Конструктор: Инициализация параметров PotViewModelInstance.");
                ModelVm.PotViewModelInstance.ParentCanvasWidth = SimCanvasWidth;
                ModelVm.PotViewModelInstance.ParentCanvasHeight = SimCanvasHeight;

                // Рассчитываем и устанавливаем координаты точки "примагничивания" крышки к полке.
                // Эта точка - это куда должен встать левый верхний угол Canvas крышки,
                // чтобы крышка визуально оказалась на полке (например, по центру).
                var capWidth = ModelVm.PotViewModelInstance.CapWidth; // Получаем ширину крышки из ее ViewModel

                ModelVm.PotViewModelInstance.ShelfSnapTarget = new Point(
                    ShelfX + (ShelfWidth - capWidth) / 2, // Центрируем крышку по горизонтали на полке
                    ShelfY // Располагаем крышку по верхнему краю полки
                    // или ShelfY + (ShelfHeight - capHeight) / 2 для центрирования по высоте
                );
                Debug.WriteLine(
                    $"[{GetType().Name}] Конструктор: ShelfSnapTarget установлен в {ModelVm.PotViewModelInstance.ShelfSnapTarget}");

                // Координаты точки "примагничивания" крышки к кастрюле (PotSnapTarget)
                // предполагается, что уже установлены в конструкторе PotViewModelBase или при его создании.
                Debug.WriteLine(
                    $"[{GetType().Name}] Конструктор: PotSnapTarget из VM: {ModelVm.PotViewModelInstance.PotSnapTarget}");
            }
            else
            {
                Debug.WriteLine(
                    $"!!! ПРЕДУПРЕЖДЕНИЕ: [{GetType().Name}] Конструктор RxUI: PotViewModelInstance в ModelVm РАВЕН null!");
            }
        }
        else
        {
            // Эта ситуация не должна возникать, если DI настроен правильно
            // и ModelSettingsViewModel успешно создается.
            Debug.WriteLine($"!!! КРИТИЧЕСКАЯ ОШИБКА: [{GetType().Name}] Конструктор RxUI: ModelVm РАВЕН null!");
        }

        Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Завершение создания.");
    }

    // --- Публичные Методы (Могут быть вызваны из View через команды или напрямую) ---

    #region Методы Обновления Состояния Модели

    // Метод для обновления текста объема кастрюли.
    // Может вызываться, например, из ControlPanelView при изменении соответствующего ComboBox.
    public Task UpdatePotVolumeText(string volume)
    {
        Debug.WriteLine(
            $"[{GetType().Name}] UpdatePotVolumeText: Попытка установить PotVolumeText = '{volume ?? "null"}'");
        
        // Обновляем свойство PotVolumeText у ТЕКУЩЕГО PotViewModelInstance,
        // который хранится в ModelVm.
        if (ModelVm.PotViewModelInstance != null)
        {
            ModelVm.PotViewModelInstance.PotVolumeText = volume ?? "N/A";
            Debug.WriteLine(
                $"[{GetType().Name}] UpdatePotVolumeText: Установлено значение ModelVm.PotViewModelInstance.PotVolumeText.");
        }
        else
        {
            Debug.WriteLine(
                $"!!! ПРЕДУПРЕЖДЕНИЕ: [{GetType().Name}] UpdatePotVolumeText: ModelVm.PotViewModelInstance РАВЕН null. Значение не установлено.");
        }

        return Task.CompletedTask; // Для асинхронной сигнатуры команды (если будет команда)
    }

    // Метод для обновления типа жидкости и ее цвета в кастрюле.
    public Task UpdateLiquidType(string liquidType)
    {
        Debug.WriteLine(
            $"[{GetType().Name}] UpdateLiquidType: Попытка установить тип жидкости = '{liquidType ?? "null"}'");
        
        if (ModelVm.PotViewModelInstance != null)
        {
            ModelVm.PotViewModelInstance.LiquidTypeText = liquidType; // Устанавливаем текст типа жидкости

            // Устанавливаем цвет жидкости в зависимости от типа
            ModelVm.PotViewModelInstance.LiquidColor = liquidType switch
            {
                "Вода" => Brushes.Aqua, // Или Brushes.CornflowerBlue
                "Масло (раст.)" => Brushes.Olive,
                "Парафин" => Brushes.AntiqueWhite,
                "Керосин" => Brushes.LightGoldenrodYellow,
                "Спирт" => Brushes.Thistle,
                "Ртуть (жид.)" => Brushes.Silver,
                _ => Brushes.Transparent // Цвет по умолчанию или для неизвестного типа
            };
            ModelVm.PotViewModelInstance.LiquidBorder = Brushes.Gray; // Если есть свойство для рамки жидкости

            Debug.WriteLine(
                $"[{GetType().Name}] UpdateLiquidType: Установлен тип жидкости и цвет для ModelVm.PotViewModelInstance.");
        }
        else
        {
            Debug.WriteLine(
                $"!!! ПРЕДУПРЕЖДЕНИЕ: [{GetType().Name}] UpdateLiquidType: ModelVm.PotViewModelInstance РАВЕН null. Тип/цвет не установлен.");
        }

        return Task.CompletedTask;
    }

    #endregion

    // --- TODO ---
    // TODO: Добавить свойства и команды для управления симуляцией (скорость, нагрев, запуск/остановка),
    // если CommonViewModel будет отвечать за эту логику.
    // Эти свойства могут быть привязаны к элементам UI на экране симуляции (CommonView).
    // Возможно, параметры (ProcessSpeed, FlameLevel) лучше получать из ControlPanelViewModel
    // через общий сервис настроек или напрямую, если ControlPanelViewModel тоже Singleton.
}