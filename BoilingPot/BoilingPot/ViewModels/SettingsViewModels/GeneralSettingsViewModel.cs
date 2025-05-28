// ViewModels/SettingsViewModels/GeneralSettingsViewModel.cs

using Avalonia.Layout;          // Для HorizontalAlignment, VerticalAlignment
using FluentAvalonia.UI.Controls; // Для Symbol, SplitViewPanePlacement (если используется во View)
using ReactiveUI;               // Базовый класс RxUI
using ReactiveUI.Fody.Helpers;  // Для [Reactive]
using System;
using System.Collections.Generic;
using System.Diagnostics;       // Для Debug
using System.Reactive.Linq;
using Avalonia.Controls; // Для WhenAnyValue и операторов Rx

// Пространство имен для ViewModel секций настроек
namespace BoilingPot.ViewModels.SettingsViewModels
{
    /// <summary>
    ///     ViewModel для секции "Общие Настройки" приложения.
    /// </summary>
    public partial class GeneralSettingsViewModel : ViewModelBase
    {
        #region Свойства Настроек

        /// <summary>
        /// Выбранный язык интерфейса.
        /// </summary>
        [Reactive] public string SelectedLanguage { get; set; } = "Русский";

        /// <summary>
        /// Флаг, указывающий, отображать ли кнопку вызова панели данных.
        /// </summary>
        [Reactive] public bool ShowDataPanelButton { get; set; } = true;

        /// <summary>
        /// Выбранное положение кнопки вызова панели данных (из ComboBox).
        /// </summary>
        [Reactive] public string SelectedDataPanelButtonPosition { get; set; } = "Верхний правый угол";

        /// <summary>
        /// Горизонтальное выравнивание кнопки панели данных (вычисляется).
        /// </summary>
        [Reactive] public HorizontalAlignment DataPanelButtonHorAlignment { get; set; } = HorizontalAlignment.Right;

        /// <summary>
        /// Вертикальное выравнивание кнопки панели данных (вычисляется).
        /// </summary>
        [Reactive] public VerticalAlignment DataPanelButtonVerAlignment { get; set; } = VerticalAlignment.Top;

        /// <summary>
        /// Символ иконки для кнопки панели данных (вычисляется).
        /// </summary>
        [Reactive] public Symbol DataPanelButtonSymbol { get; set; } = Symbol.ChevronLeft;

        /// <summary>
        /// Расположение панели данных (слева/справа) (вычисляется).
        /// </summary>
        [Reactive] public SplitViewPanePlacement DataPanePlacement { get; set; } = SplitViewPanePlacement.Right;


        /// <summary>
        /// Флаг, указывающий, размещена ли панель данных слева (управляет навигационным элементом).
        /// </summary>
        [Reactive] public bool IsDataPanelOnLeft { get; set; } // Если true, кнопка DataPanel в ControlPanel не нужна


        // --- Флаги видимости для элементов навигации в ControlPanel ---
        [Reactive] public bool ShowHomeNavItem { get; set; } = true;
        [Reactive] public bool ShowLoadNavItem { get; set; } = true; // Добавлено (вместо Load)
        [Reactive] public bool ShowSaveNavItem { get; set; } = true; // Добавлено (вместо Save)
        [Reactive] public bool ShowSettingsNavItem { get; set; } = true;
        [Reactive] public bool ShowAboutNavItem { get; set; } = true;
        [Reactive] public bool ShowExitNavItem { get; set; } = true;
        // Убраны ShowLoadNavItem и ShowSaveNavItem, так как их заменили ShowCommonNavItem и ShowMolecularNavItem
        // Если "Загрузить" и "Сохранить" - это отдельные функции, их нужно будет вернуть

        #endregion

        #region Опции для ComboBox

        /// <summary>
        /// Список доступных языков интерфейса.
        /// </summary>
        public List<string> LanguageOptions { get; } = new List<string> { "Русский", "English" };

        /// <summary>
        /// Список доступных вариантов положения кнопки.
        /// </summary>
        public List<string> PositionOptions { get; } = new List<string>
        {
            "Верхний правый угол",
            "Нижний правый угол",
            "Верхний левый угол",
            "Нижний левый угол"
        };

        #endregion

        #region Конструктор

        public GeneralSettingsViewModel()
        {
            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Начало");

            // --- Реакция на изменение выбранного положения кнопки панели данных ---
            this.WhenAnyValue(x => x.SelectedDataPanelButtonPosition)
                .Subscribe(selectedPosition =>
                {
                    Debug.WriteLine($"[{GetType().Name}] SelectedDataPanelButtonPosition изменился на: {selectedPosition}");
                    // Обновляем вычисляемые свойства на основе выбранной строки
                    switch (selectedPosition)
                    {
                        case "Верхний правый угол":
                            DataPanelButtonHorAlignment = HorizontalAlignment.Right;
                            DataPanelButtonVerAlignment = VerticalAlignment.Top;
                            // DataPanePlacement остается Right, символ ChevronLeft
                            if (IsDataPanelOnLeft) DataPanelButtonSymbol = Symbol.ChevronLeft; // Если панель слева, кнопка "открывает" вправо
                            else DataPanelButtonSymbol = Symbol.ChevronLeft;
                            break;
                        case "Нижний правый угол":
                            DataPanelButtonHorAlignment = HorizontalAlignment.Right;
                            DataPanelButtonVerAlignment = VerticalAlignment.Bottom;
                            if (IsDataPanelOnLeft) DataPanelButtonSymbol = Symbol.ChevronLeft;
                            else DataPanelButtonSymbol = Symbol.ChevronLeft;
                            break;
                        case "Верхний левый угол":
                            DataPanelButtonHorAlignment = HorizontalAlignment.Left;
                            DataPanelButtonVerAlignment = VerticalAlignment.Top;
                            // DataPanePlacement остается Left, символ ChevronRight
                            if (IsDataPanelOnLeft) DataPanelButtonSymbol = Symbol.ChevronRight;
                            else DataPanelButtonSymbol = Symbol.ChevronRight;
                            break;
                        case "Нижний левый угол":
                            DataPanelButtonHorAlignment = HorizontalAlignment.Left;
                            DataPanelButtonVerAlignment = VerticalAlignment.Bottom;
                            if (IsDataPanelOnLeft) DataPanelButtonSymbol = Symbol.ChevronRight;
                            else DataPanelButtonSymbol = Symbol.ChevronRight;
                            break;
                        default: // По умолчанию, как "Верхний правый угол"
                            DataPanelButtonHorAlignment = HorizontalAlignment.Right;
                            DataPanelButtonVerAlignment = VerticalAlignment.Top;
                            DataPanelButtonSymbol = Symbol.ChevronLeft;
                            break;
                    }
                    Debug.WriteLine($"[{GetType().Name}] Обновленные параметры кнопки: Hor={DataPanelButtonHorAlignment}, Ver={DataPanelButtonVerAlignment}, Symbol={DataPanelButtonSymbol}");
                });

            // --- Реакция на изменение флага "Разместить панель данных слева" ---
            this.WhenAnyValue(x => x.IsDataPanelOnLeft)
                .Subscribe(isOnLeft =>
                {
                    Debug.WriteLine($"[{GetType().Name}] IsDataPanelOnLeft изменился на: {isOnLeft}");
                    // Обновляем расположение панели и видимость/символ кнопки
                    DataPanePlacement = isOnLeft ? SplitViewPanePlacement.Left : SplitViewPanePlacement.Right;
                    // Кнопка навигации для DataPanel в ControlPanelViewModel отображается, если панель слева
                    // Кнопка-гамбургер на самой DataPanelView отображается, если панель справа
                    ShowDataPanelButton = !isOnLeft; // Кнопка-гамбургер видима, если панель справа

                    // Обновляем символ кнопки в зависимости от нового положения панели
                    if (isOnLeft)
                    {
                        // Если панель теперь слева, кнопка-гамбургер на DataPanelView (если она есть) должна "открывать" вправо
                        // Но у нас кнопка-гамбургер связана с IsDataPanelOnLeft
                        // Логика кнопки в ControlPanelView должна поменяться
                        // DataPanelButtonSymbol = Symbol.ChevronRight; // Это для кнопки-гамбургера, которая теперь скрыта
                    }
                    else
                    {
                        // Если панель справа, кнопка-гамбургер "открывает" влево
                        DataPanelButtonSymbol = Symbol.ChevronLeft;
                    }
                    Debug.WriteLine($"[{GetType().Name}] DataPanePlacement: {DataPanePlacement}, ShowDataPanelButton (гамбургер): {ShowDataPanelButton}, DataPanelButtonSymbol: {DataPanelButtonSymbol}");
                });

            // --- Другие подписки (пример) ---
            this.WhenAnyValue(x => x.SelectedLanguage)
                .Skip(1) // Пропускаем начальное значение при первой подписке
                .Subscribe(lang => Debug.WriteLine($"[{GetType().Name}] Выбран язык: {lang}"));

            // TODO: Загрузить сохраненные настройки при инициализации
            // Например, из сервиса настроек.

            Debug.WriteLine($"[{GetType().Name}] Конструктор RxUI: Завершение");
        }
        #endregion
    }
}