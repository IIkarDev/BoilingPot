// Views/CommonView.axaml.cs
using Avalonia.ReactiveUI; // Для ReactiveUserControl
using Avalonia.Controls; // Для Control, FindControl
using Avalonia.Styling; // Для IStyle, Styles
using BoilingPot.ViewModels; // Пространство имен CommonViewModel
using BoilingPot.ViewModels.SettingsViewModels; // Для ModelSettingsViewModel
using BoilingPot.Views.Components; // Для PotPresenter
using ReactiveUI; // Для WhenActivated
using System.Reactive.Disposables; // Для CompositeDisposable
using System;
using System.Diagnostics; // Для Debug
using System.Reactive.Linq; // Для Where, Select
using System.Reactive;
using System.Threading.Tasks; // Для Unit
using Splat; // Для LogManager

namespace BoilingPot.Views
{
    // Представление для экрана симуляции (где отображается кастрюля и плита).
    // Наследуется от ReactiveUserControl<TViewModel>.
    public partial class CommonView : ReactiveUserControl<CommonViewModel> // DataContext - CommonViewModel
    {
        private IStyle? _lastAppliedPotStyle = null; // Храним ссылку на последний примененный стиль кастрюли
        private IStyle? _lastAppliedStoveStyle = null; // Храним ссылку на последний примененный стиль плиты
        private IStyle? _lastAppliedBubbleStyle = null; // Храним ссылку на последний примененный стиль пузырьков


        public CommonView()
        {
            InitializeComponent();
            Debug.WriteLine($"[{this.GetType().Name}] View создан. HashCode: {this.GetHashCode()}");

            // WhenActivated вызывается, когда View становится активным в визуальном дереве.
            // Здесь настраиваем подписки на Observable из ViewModel, которые должны
            // влиять на UI этого View.
        this.WhenActivated(disposables =>
        {
            Debug.WriteLine($"[{this.GetType().Name}] АКТИВИРОВАН. HashCode: {this.GetHashCode()}");
            

            // --- 2. Подписка на ModelVm и настройка обработчика Interaction ---
            ViewModel.WhenAnyValue(x => x.ModelVm) // Подписываемся на изменение ModelVm
                 .Where(modelVm => modelVm != null) // Убедимся, что ModelVm не null
                 .Subscribe(modelVm => // <<< Просто подписываемся на ModelVm
                 {
                     // Настраиваем обработчик Interaction, когда ModelVm доступен
                     Debug.WriteLine($"[{this.GetType().Name}] Настройка обработчика ApplyStyleInteraction для ModelVm типа {modelVm.GetType().Name}.");
                     
                      Debug.WriteLine($"[{this.GetType().Name}] WhenActivated: Обработчик Interaction для стилей настроен.");
                 })
                 .DisposeWith(disposables); // Автоматическая отписка от подписки на ModelVm

            // Лог деактивации
            Disposable.Create(() => Debug.WriteLine($"[{this.GetType().Name}] ДЕАКТИВИРОВАН. HashCode: {this.GetHashCode()}")).DisposeWith(disposables);
        });
    }

        // Метод для применения стиля к соответствующему Presenter.
        private async Task ApplyStyleToPresenter(IStyle? style, string targetName)
        {
             Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Запрос на применение стиля для '{targetName}'. Стиль IsNull={style == null}");

            // Находим нужный Presenter по имени
            var presenter = this.FindControl<Control>(targetName + "Presenter"); // Имя должно быть "PotPresenter", "StovePresenter", "BubblePresenter"
            if (presenter == null)
            {
                Debug.WriteLine($"!!! ОШИБКА: Презентер '{targetName}Presenter' не найден в {this.GetType().Name}.");
                return;
            }
            Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Презентер '{presenter.Name}' найден.");

            // Определяем, какой стиль нужно удалить и какой запомнить, в зависимости от цели
            IStyle? styleToRemove = null;
            switch (targetName)
            {
                case "Pot": styleToRemove = _lastAppliedPotStyle; break;
                case "Stove": styleToRemove = _lastAppliedStoveStyle; break;
                case "Bubble": styleToRemove = _lastAppliedBubbleStyle; break;
            }

            // Удаляем предыдущий ДИНАМИЧЕСКИ добавленный стиль, если он был
            if (styleToRemove != null && presenter.Styles.Contains(styleToRemove))
            {
                 presenter.Styles.Remove(styleToRemove);
                 Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Старый стиль удален из '{presenter.Name}'.");
            }
             else if (styleToRemove != null) { Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Старый стиль не найден в коллекции стилей '{presenter.Name}'."); }


            // Добавляем новый стиль, если он не null
            if (style != null)
            {
                // Проверяем, нет ли уже такого стиля (маловероятно, но на всякий случай)
                if (!presenter.Styles.Contains(style))
                {
                     presenter.Styles.Add(style);
                     Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Новый стиль ДОБАВЛЕН к '{presenter.Name}'.");

                     // Запоминаем новый стиль как последний примененный
                      switch (targetName)
                     {
                         case "Pot": _lastAppliedPotStyle = style; break;
                         case "Stove": _lastAppliedStoveStyle = style; break;
                         case "Bubble": _lastAppliedBubbleStyle = style; break;
                     }
                } else { Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Новый стиль УЖЕ существует у '{presenter.Name}'."); }
            }
            else // Если пришел null, значит, нужно очистить (вернуть к дефолту, если есть)
            {
                 Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Стиль для '{presenter.Name}' очищен (пришел null).");
                 // Сбрасываем ссылку на последний примененный стиль
                  switch (targetName)
                 {
                     case "Pot": _lastAppliedPotStyle = null; break;
                     case "Stove": _lastAppliedStoveStyle = null; break;
                     case "Bubble": _lastAppliedBubbleStyle = null; break;
                 }
                 // Здесь можно было бы применить стиль по умолчанию, если он определен где-то иначе
            }
             Debug.WriteLine($"[{this.GetType().Name}] ApplyStyleToPresenter: Завершено для '{targetName}'.");
        }

        // Старые методы DataContextChanged и OnDetachedFromVisualTree больше не нужны,
        // ReactiveUserControl и WhenActivated их заменяют для управления подписками.
    }
}