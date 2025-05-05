// Services/IThemeLoaderService.cs

using Avalonia.Styling; // Для IStyle
using System;

// Пространство имен для сервисов
namespace BoilingPot.Services
{
    // Интерфейс для сервиса, который умеет загружать стили (темы) из файлов (.axaml).
    public interface IThemeLoaderService
    {
        // Метод для загрузки стиля из файла .axaml по указанному пути.
        // Принимает полный путь к файлу на диске.
        // Возвращает загруженный стиль (IStyle) или null, если файл не найден,
        // не является валидным XAML стилем, или произошла ошибка при парсинге.
        IStyle? LoadStyleFromFile(string filePath);

        // Возможно, добавить методы для загрузки стилей из ресурсов приложения (avares://)
        // IStyle? LoadStyleFromResource(Uri resourceUri);
    }
}