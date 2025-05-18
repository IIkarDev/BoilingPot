// Views/Components/PotPresenter.axaml.cs
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using BoilingPot.ViewModels.Components;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Diagnostics;
using Avalonia.ReactiveUI;

namespace BoilingPot.Views.Components
{
    public partial class PotPresenter : ReactiveUserControl<PotViewModelBase>
    {
        private Point _startMousePosition;    // Позиция мыши при нажатии (относительно this = PotPresenter)
        private Point _startElementPosition;  // Позиция PotCap (Left, Top из VM) при нажатии
        private bool _isDragging;

        // Радиус "примагничивания"
        private const double SnapRadius = 96; // Увеличим немного для удобства

        private Canvas? _potCapElement;

        public PotPresenter()
        {
            InitializeComponent();
            Debug.WriteLine($"[{this.GetType().Name}] Конструктор PotPresenter завершен.");

            this.WhenActivated(disposables =>
            {
                Debug.WriteLine($"[{this.GetType().Name}] PotPresenter АКТИВИРОВАН.");
                // Подписка на ViewModel, если нужно реагировать на изменения из VM,
                // например, принудительно установить положение крышки.

                Disposable.Create(() =>
                {
                    Debug.WriteLine($"[{this.GetType().Name}] PotPresenter ДЕАКТИВИРОВАН.");
                    if (_potCapElement != null)
                    {
                        _potCapElement.PointerPressed -= OnPointerPressed;
                        _potCapElement.PointerMoved -= OnPointerMoved;
                        _potCapElement.PointerReleased -= OnPointerReleased;
                        Debug.WriteLine($"[{this.GetType().Name}] Отписались от событий крышки при деактивации.");
                    }
                }).DisposeWith(disposables);
            });
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            Debug.WriteLine($"[{this.GetType().Name}] OnApplyTemplate: Шаблон применен.");

            if (_potCapElement != null)
            {
                _potCapElement.PointerPressed -= OnPointerPressed;
                _potCapElement.PointerMoved -= OnPointerMoved;
                _potCapElement.PointerReleased -= OnPointerReleased;
                Debug.WriteLine($"[{this.GetType().Name}] OnApplyTemplate: Отписались от событий старой крышки.");
            }

            _potCapElement = e.NameScope.Find<Canvas>("PotCap");

            if (_potCapElement != null)
            {
                Debug.WriteLine($"[{this.GetType().Name}] OnApplyTemplate: Элемент 'PotCap' (тип: {_potCapElement.GetType().Name}) найден.");
                _potCapElement.PointerPressed += OnPointerPressed;
                _potCapElement.PointerMoved += OnPointerMoved;
                _potCapElement.PointerReleased += OnPointerReleased;
                Debug.WriteLine($"[{this.GetType().Name}] OnApplyTemplate: Подписались на события новой крышки.");
            }
            else { Debug.WriteLine($"!!! ОШИБКА: [{this.GetType().Name}] OnApplyTemplate: Элемент 'PotCap' НЕ НАЙДЕН!"); }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel is PotViewModelBase vm && _potCapElement != null && e.GetCurrentPoint(_potCapElement).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _startMousePosition = e.GetPosition(this.Parent as Visual); 
                _startElementPosition = new Point(vm.Left, vm.Top);
                
                e.Handled = true;
                Debug.WriteLine($"[{this.GetType().Name}] OnPointerPressed: Dragging начат. StartMouseInParent: {_startMousePosition}, StartElement: {_startElementPosition}");
            }
            else { Debug.WriteLine($"[{this.GetType().Name}] OnPointerPressed: Условие не выполнено. ViewModel is null: {ViewModel == null}, _potCapElement is null: {_potCapElement == null}"); }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && ViewModel is PotViewModelBase vm && _potCapElement != null)
            {
                var currentMousePositionInParent = e.GetPosition(this.Parent as Visual);
                var offset = currentMousePositionInParent - _startMousePosition;

                double newLeft = _startElementPosition.X + offset.X;
                double newTop = _startElementPosition.Y + offset.Y;

                // --- Ограничение выхода за границы родительского Canvas ---
                // Предполагаем, что PotPresenter сам лежит на Canvas, и его размеры совпадают с ParentCanvasWidth/Height
                // Иначе нужно передавать Bounds родительского Canvas в PotPresenter
                var parentWidth = vm.ParentCanvasWidth;   // Из ViewModel
                var parentHeight = vm.ParentCanvasHeight; // Из ViewModel
                var capWidth = vm.CapWidth;     // Из ViewModel
                var capHeight = vm.CapHeight;   // Из ViewModel

                // Ограничиваем Left
                if (newLeft < 0) newLeft = 0;
                if (newLeft + capWidth > parentWidth) newLeft = parentWidth - capWidth;

                // Ограничиваем Top
                if (newTop < 0) newTop = 0;
                if (newTop + capHeight > parentHeight) newTop = parentHeight - capHeight;

                vm.Left = newLeft;
                vm.Top = newTop;
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_isDragging) return; // Если не было перетаскивания, выходим

            _isDragging = false;
            if (ViewModel is PotViewModelBase vm && _potCapElement != null)
            {
                e.Pointer.Capture(null);
                e.Handled = true;

                // Текущая позиция крышки (ее левый верхний угол)
                var currentCapPosition = new Point(vm.Left, vm.Top);

                // --- Проверка примагничивания к Кастрюле ---
                // PotSnapTarget - это координаты, куда должен встать левый верхний угол PotCap,
                // чтобы крышка оказалась на кастрюле.
                var distanceToPot = Math.Sqrt(Math.Pow((currentCapPosition - vm.PotSnapTarget).X, 2) +
                                              Math.Pow((currentCapPosition - vm.PotSnapTarget).Y, 2));
                Debug.WriteLine($"[{this.GetType().Name}] OnPointerReleased: CapPos: {currentCapPosition}, PotSnap: {vm.PotSnapTarget}, DistToPot: {distanceToPot}");

                if (distanceToPot <= SnapRadius)
                {
                    vm.Left = vm.PotSnapTarget.X;
                    vm.Top = vm.PotSnapTarget.Y;
                    Debug.WriteLine($"[{this.GetType().Name}] OnPointerReleased: Прилипание к КАСТРЮЛЕ.");
                    return; // Выходим, если прилипли
                }

                // --- Проверка примагничивания к Полке ---
                // ShelfSnapTarget - это координаты, куда должен встать левый верхний угол PotCap,
                // чтобы крышка оказалась на полке.
                // Эти координаты должны быть установлены в ViewModel в системе координат
                // родительского Canvas для PotPresenter.
                var distanceToShelf = Math.Sqrt(Math.Pow((currentCapPosition - vm.ShelfSnapTarget).X, 2) +
                                                Math.Pow((currentCapPosition - vm.ShelfSnapTarget).Y, 2));
                 Debug.WriteLine($"[{this.GetType().Name}] OnPointerReleased: CapPos: {currentCapPosition}, ShelfSnap: {vm.ShelfSnapTarget}, DistToShelf: {distanceToShelf}");

                if (distanceToShelf <= SnapRadius)
                {
                    vm.Left = vm.ShelfSnapTarget.X;
                    vm.Top = vm.ShelfSnapTarget.Y;
                    Debug.WriteLine($"[{this.GetType().Name}] OnPointerReleased: Прилипание к ПОЛКЕ.");
                    return; // Выходим, если прилипли
                }

                // Если не прилипли ни к чему, оставляем крышку там, где отпустили
                // (позиция уже установлена в OnPointerMoved)
                 Debug.WriteLine($"[{this.GetType().Name}] OnPointerReleased: Без прилипания. Итоговые Left: {vm.Left}, Top: {vm.Top}");
            }
        }
    }
}