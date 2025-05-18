// ViewModels/SettingsViewModels/GeneralSettingsViewModel.cs

using ReactiveUI; // Базовый класс RxUI
using ReactiveUI.Fody.Helpers; // Для [Reactive]
using System;
using System.Collections.Generic;
using System.Diagnostics; // Для Debug
using System.Reactive; // Для Unit
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls; // Для WhenAnyValue

// Пространство имен для ViewModel секций настроек
namespace BoilingPot.ViewModels.SettingsViewModels
{
    // ViewModel для секции "Общие Настройки".
    public partial class GeneralSettingsViewModel : ViewModelBase
    {
        // --- Свойства для общих настроек ---

        // Язык (StringEqualsConverter используется в View для RadioButton)
        [Reactive] public string SelectedLanguage { get; set; } = "Русский"; // Начальное значение

        // Панель данных
        [Reactive] public bool ShowDataPanelButton { get; set; } = true; // Флаг отображения кнопки панели
        [Reactive] public HorizontalAlignment DataPanelButtonHorAlignment { get; set; } = HorizontalAlignment.Right; // Положение кнопки
        [Reactive] public VerticalAlignment DataPanelButtonVerAlignment { get; set; } = VerticalAlignment.Top; // Положение кнопки
        [Reactive] public SplitViewPanePlacement DataPanePlacement{ get; set; } = SplitViewPanePlacement.Right;
        [Reactive] public Symbol DataPanelButtonSymbol { get; set; } = Symbol.ChevronLeft;
        [Reactive] public string SelectedDataPanelButtonPosition { get; set; } = "Верхний правый угол"; // Положение кнопки
        [Reactive] public bool IsDataPanelOnLeft { get; set; } // Положение панели

        // Отображение элементов навигации в ControlPanel (NavigationView)
        [Reactive] public bool ShowHomeNavItem { get; set; } = true;
        [Reactive] public bool ShowLoadNavItem { get; set; } = true;
        [Reactive] public bool ShowSaveNavItem { get; set; } = true;
        [Reactive] public bool ShowSettingsNavItem { get; set; } = true; // Навигация на эту секцию
        [Reactive] public bool ShowAboutNavItem { get; set; } = true;
        [Reactive] public bool ShowExitNavItem { get; set; } = true;

        // Списки опций для ComboBox
        public List<string> LanguageOptions { get; } = new List<string> { "Русский", "English" };
        public List<string> PositionOptions { get; } = new List<string> { "Верхний правый угол", "Нижний правый угол", "Верхний левый угол", "Нижний левый угол" };


        // --- Команды (если нужны для специфичных действий) ---
        // public ReactiveCommand<Unit, Unit> ApplyGeneralSettingsCommand { get; }

        // --- Конструктор ---
        public GeneralSettingsViewModel()
        {
             Debug.WriteLine("[GeneralSettingsVM] Конструктор RxUI: Начало");
             
             this.WhenAnyValue(x => x.SelectedDataPanelButtonPosition)
                 .Subscribe(selectedDataPanelButtonPosition => // Выполняем действие при получении нового Tag
                 {
                     switch (selectedDataPanelButtonPosition)
                     {
                         case "Верхний правый угол": 
                             DataPanelButtonHorAlignment = HorizontalAlignment.Right;
                             DataPanelButtonVerAlignment = VerticalAlignment.Top;
                             DataPanePlacement = SplitViewPanePlacement.Right;
                             DataPanelButtonSymbol = Symbol.ChevronLeft; break;
                         case "Нижний правый угол": 
                             DataPanelButtonHorAlignment = HorizontalAlignment.Right;
                             DataPanelButtonVerAlignment = VerticalAlignment.Bottom;
                             DataPanePlacement = SplitViewPanePlacement.Right;
                             DataPanelButtonSymbol = Symbol.ChevronLeft; break;
                         case "Верхний левый угол": 
                             DataPanelButtonHorAlignment = HorizontalAlignment.Left;
                             DataPanelButtonVerAlignment = VerticalAlignment.Top;
                             DataPanePlacement = SplitViewPanePlacement.Left;
                             DataPanelButtonSymbol = Symbol.ChevronRight; break;
                         case "Нижний левый угол": 
                             DataPanelButtonHorAlignment = HorizontalAlignment.Left;
                             DataPanelButtonVerAlignment = VerticalAlignment.Bottom; 
                             DataPanePlacement = SplitViewPanePlacement.Left;
                             DataPanelButtonSymbol = Symbol.ChevronRight; break;
                        default:
                             DataPanelButtonHorAlignment = HorizontalAlignment.Right;
                             DataPanelButtonVerAlignment = VerticalAlignment.Top; 
                             DataPanePlacement = SplitViewPanePlacement.Right;
                             DataPanelButtonSymbol = Symbol.ChevronLeft; break;
                     }
                 });
            // --- Реакция на изменение свойств (пример) ---
            // Подписываемся на изменение языка (когда UI обновит SelectedLanguage)
            this.WhenAnyValue(x => x.SelectedLanguage)
                .Skip(1) // Пропускаем начальное значение
                .Subscribe(lang => Debug.WriteLine($"[GeneralSettingsVM] Выбран язык: {lang}"));

            // Подписываемся на изменение флага ShowDataPanelButton
            this.WhenAnyValue(x => x.ShowDataPanelButton)
                 .Subscribe(isVisible => Debug.WriteLine($"[GeneralSettingsVM] Кнопка панели данных видима: {isVisible}"));

            // Подписываемся на изменение положения панели
             this.WhenAnyValue(x => x.IsDataPanelOnLeft)
                  .Subscribe(isOnLeft =>
                  {
                      DataPanePlacement = isOnLeft ? SplitViewPanePlacement.Left : SplitViewPanePlacement.Right;
                      ShowDataPanelButton = !isOnLeft;
                      Debug.WriteLine($"[GeneralSettingsVM] Панель данных слева: {isOnLeft}");
                  });


            // TODO: Добавить логику загрузки/сохранения настроек при инициализации или при вызове команды "Сохранить" в SettingsViewModel
            // TODO: Возможно, некоторые настройки (как язык) должны быть применены сразу, другие - при сохранении.

             Debug.WriteLine("[GeneralSettingsVM] Конструктор RxUI: Завершение");
        }
    }
}