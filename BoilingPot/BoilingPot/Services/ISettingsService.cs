// Services/ISettingsService.cs
using BoilingPot.Models; // Для AppSettings
using System.Threading.Tasks; // Для Task
using Avalonia.Platform.Storage; // Для IStorageFile (результат SaveFilePicker)

namespace BoilingPot.Services
{
    // Интерфейс для сервиса управления настройками приложения.
    public interface ISettingsService
    {
        // Текущие настройки приложения. ViewModel будут получать доступ к этому объекту.
        // Свойство get; чтобы другие не могли заменить сам объект AppSettings,
        // а только изменять его поля.
        AppSettings CurrentSettings { get; }

        // Загружает настройки из файла по УМОЛЧАНИЮ (скрыто от пользователя).
        // Вызывается при старте приложения.
        Task LoadDefaultSettingsAsync();

        // Сохраняет текущие настройки в файл по УМОЛЧАНИЮ (скрыто от пользователя).
        // Может вызываться автоматически или по команде "Применить".
        Task SaveDefaultSettingsAsync();

        // --- НОВЫЕ МЕТОДЫ для работы с файлами через диалоги ---

        // Загружает настройки из ВЫБРАННОГО пользователем файла.
        // Возвращает true, если загрузка успешна, иначе false.
        Task<bool> LoadSettingsFromFileDialogAsync();

        // Сохраняет текущие настройки в ВЫБРАННЫЙ пользователем файл.
        // Возвращает true, если сохранение успешно, иначе false.
        Task<bool> SaveSettingsToFileDialogAsync();

        // Применяет загруженные настройки к текущему состоянию ViewModel
        // (если это не происходит автоматически через привязки).
        // Этот метод может быть специфичен для каждого ViewModel или быть общим.
        // Пока оставим как напоминание, конкретная реализация зависит от того,
        // как ViewModel реагируют на изменения в CurrentSettings.
        // void ApplySettingsToViewModels();
    }
}