// Services/ThemeLoaderService.cs

using Avalonia.Markup.Xaml; // Для AvaloniaRuntimeXamlLoader.Load
using Avalonia.Styling; // Для IStyle, Styles, ControlTheme
using System;
using System.Diagnostics; // Для Debug
using System.IO;       // Для File.Exists, File.ReadAllText
using System.Linq;     // Для .OfType<T>() и .FirstOrDefault()

// Пространство имен для сервисов
namespace BoilingPot.Services
{
    // Реализация сервиса IThemeLoaderService.
    // Отвечает за чтение и парсинг .axaml файлов со стилями.
    public class ThemeLoaderService : IThemeLoaderService
    {
        // Реализация метода загрузки стиля из файла на диске.
        public IStyle? LoadStyleFromFile(string filePath)
        {
            Debug.WriteLine($"--- ThemeLoaderService: Начинаем загрузку стиля из файла: {filePath} ---");

            // Проверяем существование файла
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("!!! ThemeLoaderService: ОШИБКА: Файл стиля не найден.");
                return null;
            }

            try
            {
                // 1. Читаем содержимое файла как текст
                string xamlContent = File.ReadAllText(filePath);
                Debug.WriteLine($"--- ThemeLoaderService: Файл '{Path.GetFileName(filePath)}' успешно прочитан. Длина: {xamlContent.Length}");

                // 2. Парсим XAML строку во время выполнения.
                // AvaloniaRuntimeXamlLoader.Load преобразует XAML в объекты Avalonia.
                var loadedObject = AvaloniaRuntimeXamlLoader.Load(xamlContent);
                Debug.WriteLine($"--- ThemeLoaderService: XAML распарсен, тип объекта: {loadedObject?.GetType().FullName ?? "null"}");

                // 3. Проверяем, что распарсенный объект является стилем (Styles или IStyle)
                if (loadedObject is Styles styles) // Если корневой элемент в файле был <Styles>
                {
                    Debug.WriteLine("--- ThemeLoaderService: Распарсен как <Styles>.");
                    // Ищем ControlTheme внутри коллекции стилей (если нужен именно он)
                    var controlTheme = styles.OfType<ControlTheme>().FirstOrDefault();
                    if (controlTheme != null)
                    {
                         Debug.WriteLine("--- ThemeLoaderService: Найден ControlTheme внутри <Styles>.");
                        return controlTheme; // Возвращаем найденный ControlTheme
                    }
                    // Если ControlTheme не найден, берем первый попавшийся стиль (если он есть)
                    var firstStyle = styles.FirstOrDefault();
                    if (firstStyle != null) Debug.WriteLine("--- ThemeLoaderService: ControlTheme не найден, взят первый стиль из <Styles>.");
                    return firstStyle; // Возвращаем первый стиль из коллекции

                }
                else if (loadedObject is IStyle style) // Если корневой элемент был <Style> или <ControlTheme>
                {
                    Debug.WriteLine($"--- ThemeLoaderService: Распарсен как корневой стиль/тема: {style.GetType().Name}");
                    return style; // Возвращаем сам корневой стиль
                }
                else
                {
                    Debug.WriteLine("!!! ThemeLoaderService: ОШИБКА: Распарсенный объект не является Styles или IStyle.");
                    return null; // Объект не является ожидаемым типом стиля
                }
            }
            catch (Exception ex)
            {
                // Ловим любые ошибки, которые могут возникнуть при чтении файла или парсинге XAML
                Debug.WriteLine($"!!! ThemeLoaderService: КРИТИЧЕСКАЯ ОШИБКА при загрузке/парсинге файла стиля '{filePath}': {ex}");
                // В реальном приложении здесь может быть более детальная обработка ошибок
                return null;
            }
        }

        // Возможно, добавить реализацию LoadStyleFromResource(Uri resourceUri)
        // используя Avalonia.Platform.AssetLoader.Open.
    }
}