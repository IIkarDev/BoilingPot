// Services/IPluginLoaderService.cs

using Avalonia.Controls;
using Avalonia.Styling;

// Для типа Control

// Пространство имен для сервисов основного приложения
namespace BoilingPot.Services
{
    // Интерфейс определяет контракт для сервиса, который будет
    // отвечать за загрузку View (и опционально ViewModel) из внешних DLL.
    public interface IPluginLoaderService
    {
        // Метод для загрузки View.
        // Принимает путь к DLL и полное имя типа View внутри этой DLL.
        // Возвращает экземпляр загруженного Control или null в случае ошибки.
        IStyle? LoadStyleFromFile(string dllPath);

        // (Опционально) Метод для загрузки ViewModel, если это нужно делать отдельно.
        // В нашей реализации PluginLoaderService мы будем загружать ViewModel
        // внутри LoadViewFromDll и устанавливать его как DataContext.
        // object? LoadViewModelFromDll(string dllPath, string viewModelTypeName);
    }
}