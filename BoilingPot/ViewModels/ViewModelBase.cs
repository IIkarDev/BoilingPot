// ViewModels/ViewModelBase.cs
using ReactiveUI; // Базовый класс RxUI

namespace BoilingPot.ViewModels
{
    // Базовый класс для ВСЕХ ViewModel в проекте.
    // Наследуется от ReactiveObject, который предоставляет реализацию
    // INotifyPropertyChanged и методы для работы с реактивными свойствами/командами.
    public abstract class ViewModelBase : ReactiveObject
    {
        // Используйте атрибут [Reactive] из ReactiveUI.Fody
        // для автоматической генерации свойств с уведомлением об изменении.
        // [Reactive] public string MyProperty { get; set; }

        // Если не используете Fody, реализуйте свойства так:
        // private string _myManualProperty;
        // public string MyManualProperty
        // {
        //     get => _myManualProperty;
        //     set => this.RaiseAndSetIfChanged(ref _myManualProperty, value);
        // }

        protected ViewModelBase()
        {
            // Общая логика конструктора, если нужна
        }
    }
}