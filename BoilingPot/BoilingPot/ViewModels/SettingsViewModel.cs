// ViewModels/SettingsViewModel.cs

using BoilingPot.ViewModels.SettingsViewModels; // Для VM секций
using FluentAvalonia.UI.Controls;           // Для NavigationViewItem
using ReactiveUI;                           // Базовый класс и Interaction
using ReactiveUI.Fody.Helpers;              // Для [Reactive]
using System;
using System.Diagnostics;
using System.Reactive;                      // Для Unit
using System.Reactive.Linq;                 // Для WhenAnyValue и операторов Rx
using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider

namespace BoilingPot.ViewModels
{
    /// <summary>
    ///     ViewModel для окна/панели настроек.
    ///     Управляет навигацией между различными секциями настроек:
    ///     Общие (General), Темы (Themes), Модели (Models).
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        #region Поля

        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Свойства ViewModel для секций

        /// <summary>
        /// ViewModel для секции "Общие настройки".
        /// </summary>
        public GeneralSettingsViewModel GeneralSettings { get; }

        /// <summary>
        /// ViewModel для секции "Настройки Темы".
        /// </summary>
        public ThemeSettingsViewModel ThemeSettings { get; }

        /// <summary>
        /// ViewModel для секции "Настройки Моделей".
        /// </summary>
        public ModelSettingsViewModel ModelSettings { get; }

        #endregion

        #region Свойства для Навигации

        /// <summary>
        /// Выбранный элемент в NavigationView (обычно объект NavigationViewItem).
        /// Привязывается к свойству SelectedItem у NavigationView в XAML.
        /// </summary>
        [Reactive] public object? SelectedNavItem { get; set; }

        /// <summary>
        /// Текущий ViewModel, который должен отображаться в основной области контента
        /// панели настроек (например, в ContentControl).
        /// </summary>
        [Reactive] public ViewModelBase CurrentSettingSectionViewModel { get; private set; }

        /// <summary>
        /// Флаг, управляющий видимостью всей панели/окна настроек.
        /// Устанавливается извне (например, из MainViewModel).
        /// </summary>
        [Reactive] public bool IsShowingSettings { get; set; }

        #endregion

        #region Interactions (Взаимодействия с View)

        /// <summary>
        /// Interaction для запроса закрытия панели/окна настроек.
        /// ViewModel вызывает Handle() у этого Interaction, а View (или родительский VM)
        /// подписывается на него и выполняет фактическое закрытие/скрытие.
        /// TInput: Unit (не передаем данные в View).
        /// TOutput: Unit (не ожидаем результата от View).
        /// </summary>
        public Interaction<Unit, Unit> CloseSettingsInteraction { get; }

        #endregion

        #region Команды

        /// <summary>
        /// Команда для закрытия панели/окна настроек.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CloseSettingsCommand { get; }

        #endregion

        #region Конструктор

        public SettingsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Начало");

            // Инициализация Interaction для запроса закрытия
            CloseSettingsInteraction = new Interaction<Unit, Unit>();

            // Получаем экземпляры ViewModel для каждой секции из DI контейнера.
            // Они должны быть зарегистрированы в App.axaml.cs (например, как Transient).
            GeneralSettings = _serviceProvider.GetRequiredService<GeneralSettingsViewModel>();
            ThemeSettings = _serviceProvider.GetRequiredService<ThemeSettingsViewModel>();
            ModelSettings = _serviceProvider.GetRequiredService<ModelSettingsViewModel>();
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: ViewModel секций настроек получены из DI.");

            // Устанавливаем начальную секцию настроек (например, "Общие")
            CurrentSettingSectionViewModel = GeneralSettings;
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Начальная секция настроек установлена на 'General'.");

            // --- Инициализация Команды Закрытия ---
            CloseSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.WriteLine($"[{GetType().Name}] CloseSettingsCommand: Команда вызвана.");
                // Сначала устанавливаем флаг видимости в false (если это нужно сделать из этого VM)
                IsShowingSettings = false;
                // Затем отправляем запрос на закрытие через Interaction.
                // View или родительский ViewModel должен обработать этот Interaction.
                await CloseSettingsInteraction.Handle(Unit.Default);
                Debug.WriteLine($"[{GetType().Name}] CloseSettingsCommand: Interaction 'CloseSettingsInteraction' обработан.");
                // Здесь можно добавить логику сохранения всех настроек ПЕРЕД вызовом Handle,
                // если это не делается автоматически при изменении каждого свойства.
            });
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: CloseSettingsCommand инициализирована.");

            // --- Реакция на смену выбранного элемента в NavigationView ---
            // Подписываемся на изменения свойства SelectedNavItem.
            this.WhenAnyValue(x => x.SelectedNavItem)
                // Фильтруем: интересуют только реальные NavigationViewItem
                .Where(itemObject => itemObject is NavigationViewItem)
                // Приводим к типу NavigationViewItem
                .Cast<NavigationViewItem>()
                // Фильтруем: интересуют только те, у которых Tag - это строка
                .Where(navViewItem => navViewItem.Tag is string)
                // Извлекаем значение Tag как строку
                .Select(navViewItem => navViewItem.Tag as string)
                // Подписываемся на поток значений Tag
                .Subscribe(tagValue =>
                {
                    Debug.WriteLine($"[{GetType().Name}] Выбран NavItem с Tag: '{tagValue}'");
                    // В зависимости от Tag, устанавливаем соответствующий ViewModel секции
                    // в CurrentSettingSectionViewModel, что вызовет смену View в ContentControl.
                    CurrentSettingSectionViewModel = tagValue switch
                    {
                        "General" => GeneralSettings,
                        "Themes" => ThemeSettings,
                        "Models" => ModelSettings,
                        _ => GeneralSettings // Секция по умолчанию, если Tag неизвестен
                    };
                    Debug.WriteLine($"[{GetType().Name}] CurrentSettingSectionViewModel установлен в: {CurrentSettingSectionViewModel?.GetType().Name}");
                });

            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Подписка на SelectedNavItem настроена.");
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Завершение");
        }

        #endregion
    }
}