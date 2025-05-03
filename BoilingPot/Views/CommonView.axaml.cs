// Views/CommonView.axaml.cs
using Avalonia.Controls;
using Avalonia.Styling; // Для IStyle
using BoilingPot.ViewModels; // Для CommonViewModel
using BoilingPot.ViewModels.SettingsViewModels; // Для ModelSettingsViewModel
using BoilingPot.Views.Components; // Для PotPresenter
using System;

namespace BoilingPot.Views
{
    public partial class CommonView : UserControl
    {
        private IStyle? _lastAppliedPotStyle = null; // Храним ссылку на последний примененный стиль

        public CommonView()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private async void OnDataContextChanged(object? sender, EventArgs e)
        {
            // ... (код отписки/подписки на ApplyStyleRequested) ...

            // !!! ВЫЗЫВАЕМ ИНИЦИАЛИЗАЦИЮ ЗДЕСЬ !!!
            if (this.DataContext is CommonViewModel vm && vm.ModelVm != null)
            {
                System.Diagnostics.Debug.WriteLine($"[{this.GetType().Name}] OnDataContextChanged: Вызов ModelVm.InitializeAsync().");
                await vm.ModelVm.InitializeAsync(); // Асинхронный вызов
            }
        }
        // private void OnDataContextChanged(object? sender, EventArgs e)
        // {
        //     if (this.DataContext is CommonViewModel vm && vm.ModelVm != null)
        //     {
        //         // Отписываемся от старого, если был
        //          if (vm.ModelVm != null) vm.ModelVm.ApplyStyleRequested -= ApplyStyleToPresenter;
        //         // Подписываемся на событие нового ViewModel
        //         vm.ModelVm.ApplyStyleRequested += ApplyStyleToPresenter;
        //     }
        //      // TODO: Реализовать отписку от старого vm.ModelVm при смене DataContext самого CommonView
        // }

        private void ApplyStyleToPresenter(IStyle? style, string targetName)
        {
            // Находим нужный Presenter по имени (убедитесь, что имя задано в XAML)
            var presenter = this.FindControl<Control>(targetName + "Presenter"); // Например, "PotPresenter"
            if (presenter == null)
            {
                System.Diagnostics.Debug.WriteLine($"Презентер '{targetName}Presenter' не найден в CommonView.");
                return;
            }

            // --- Логика очистки старого и добавления нового стиля ---
            // Удаляем предыдущий динамически добавленный стиль, если он был
            if (_lastAppliedPotStyle != null && presenter.Styles.Contains(_lastAppliedPotStyle))
            {
                 presenter.Styles.Remove(_lastAppliedPotStyle);
                 System.Diagnostics.Debug.WriteLine($"Старый стиль удален из {presenter.Name}");
            }

            // Добавляем новый стиль, если он не null
            if (style != null)
            {
                presenter.Styles.Add(style);
                _lastAppliedPotStyle = style; // Запоминаем новый стиль
                System.Diagnostics.Debug.WriteLine($"Новый стиль применен к {presenter.Name}");
            }
            else
            {
                 _lastAppliedPotStyle = null; // Сброс, если пришел null (для очистки)
                 System.Diagnostics.Debug.WriteLine($"Стиль для {presenter.Name} очищен.");
                 // Можно вернуть стиль по умолчанию, если он был определен где-то еще
            }
        }

        // Не забыть отписаться
        protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
             if (this.DataContext is CommonViewModel vm && vm.ModelVm != null)
             {
                 vm.ModelVm.ApplyStyleRequested -= ApplyStyleToPresenter;
             }
            base.OnDetachedFromVisualTree(e);
        }
    }
}

// Не забудьте дать имя вашему PotPresenter в CommonView.axaml:
// <comp:PotPresenter x:Name="PotPresenter" ... />