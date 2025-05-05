// ViewModels/SettingsViewModel.cs
using BoilingPot.ViewModels.SettingsViewModels; // Для VM секций
using ReactiveUI; // Базовый класс и Interaction
using ReactiveUI.Fody.Helpers; // Для [Reactive]
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq; // Для Unit
using System.Windows.Input; // Для ICommand (хотя RxUI команды уже ICommand)
using FluentAvalonia.UI.Controls; // Для NavigationViewItem
using Microsoft.Extensions.DependencyInjection; // Для IServiceProvider

namespace BoilingPot.ViewModels
{
    // ViewModel для окна/панели настроек в целом.
    // Управляет навигацией между секциями (General, Themes, Models).
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        [Reactive] public bool IsShowingSettings { get; set; }

        // --- Экземпляры ViewModel для каждой секции (получаем через DI) ---
        public GeneralSettingsViewModel GeneralSettings { get; }
        public ThemeSettingsViewModel ThemeSettings { get; }
        public ModelSettingsViewModel ModelSettings { get; }

        // --- Управление навигацией ---
        // Выбранный элемент в NavigationView (объект NavigationViewItem)
        [Reactive] public NavigationViewItem SelectedNavItem { get; set; }

        // Текущий ViewModel, который должен отображаться в ContentControl
        [Reactive] public ViewModelBase CurrentSettingSectionViewModel { get; private set; }

        // --- Взаимодействие (Interaction) для запроса закрытия ---
        // ViewModel не должен сам закрывать окно/панель, он должен попросить родителя.
        public Interaction<Unit, Unit> CloseSettingsInteraction { get; }

        // --- Команда для кнопки закрытия ---
        public ReactiveCommand<Unit, Unit> CloseSettingsCommand { get; }

        // --- Конструктор ---
        public SettingsViewModel(IServiceProvider serviceProvider) // Получаем зависимости через DI
        {
            _serviceProvider = serviceProvider;
             Debug.WriteLine("[SettingsViewModel] Конструктор RxUI: Начало");

            // Создаем Interaction
            CloseSettingsInteraction = new Interaction<Unit, Unit>();

            // Получаем экземпляры ViewModel для секций из DI контейнера
            GeneralSettings = _serviceProvider.GetRequiredService<GeneralSettingsViewModel>();
            ThemeSettings = _serviceProvider.GetRequiredService<ThemeSettingsViewModel>();
            ModelSettings = _serviceProvider.GetRequiredService<ModelSettingsViewModel>();
            Debug.WriteLine("[SettingsViewModel] Конструктор RxUI: ViewModel секций получены.");

            // Устанавливаем начальную секцию
            CurrentSettingSectionViewModel = GeneralSettings;
            Debug.WriteLine("[SettingsViewModel] Конструктор RxUI: Начальная секция установлена (General).");

            // --- Инициализация Команд ---
            CloseSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                 Debug.WriteLine("[SettingsViewModel] CloseSettingsCommand: Команда вызвана.");
                 // Отправляем запрос на закрытие через Interaction.
                 // .Handle() возвращает IObservable<Unit>, подписываемся, чтобы он выполнился.
                 IsShowingSettings = false;
                 // await CloseSettingsInteraction.Handle(Unit.Default);
                 Debug.WriteLine("[SettingsViewModel] CloseSettingsCommand: Interaction обработан.");
                 // Здесь можно добавить логику сохранения настроек ПЕРЕД вызовом Handle, если нужно
            });

            // --- Реакция на смену выбранного элемента навигации ---
            // Подписываемся на изменения свойства SelectedNavItem
            this.WhenAnyValue(x => x.SelectedNavItem)
                .Where(item => item is NavigationViewItem) // Убедимся, что это нужный тип
                .Cast<NavigationViewItem>() // Приводим тип
                .Where(item => item.Tag is string) // Убедимся, что Tag - строка
                .Select(item => item.Tag as string) // Берем Tag
                .Subscribe(tag => // Выполняем действие при получении нового Tag
                {
                    Debug.WriteLine($"[SettingsViewModel] Выбран NavItem с Tag: {tag}");
                    switch (tag)
                    {
                        case "General": CurrentSettingSectionViewModel = GeneralSettings; break;
                        case "Themes": CurrentSettingSectionViewModel = ThemeSettings; break;
                        case "Models": CurrentSettingSectionViewModel = ModelSettings; break;
                        default: CurrentSettingSectionViewModel = GeneralSettings; break; // По умолчанию
                    }
                    Debug.WriteLine($"[SettingsViewModel] CurrentSettingSectionViewModel установлен в: {CurrentSettingSectionViewModel?.GetType().Name}");
                });

             Debug.WriteLine("[SettingsViewModel] Конструктор RxUI: Завершение");
        }
    }
}