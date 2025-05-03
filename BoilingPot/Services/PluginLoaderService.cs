using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq; // Для FirstOrDefault

namespace BoilingPot.Services
{
    public class PluginLoaderService : IPluginLoaderService
    {
        public IStyle? LoadStyleFromFile(string filePath)
        {
            Debug.WriteLine($"--- Загрузка стиля из файла: {filePath} ---");
            if (!File.Exists(filePath))
            {
                Debug.WriteLine("!!! ОШИБКА: Файл стиля не найден.");
                return null;
            }

            try
            {
                string xamlContent = File.ReadAllText(filePath);
                // Парсим XAML строку с помощью RuntimeXamlLoader
                var loadedObject = AvaloniaRuntimeXamlLoader.Load(xamlContent);

                if (loadedObject is Styles styles) // Если корневой элемент <Styles>
                {
                    // Ищем ControlTheme внутри (можно уточнить поиск по TargetType)
                    var controlTheme = styles.OfType<ControlTheme>().FirstOrDefault();
                    if (controlTheme != null)
                    {
                         Debug.WriteLine($"Найден ControlTheme в <Styles>.");
                        return controlTheme;
                    }
                    // Или ищем первый попавшийся стиль, если ControlTheme не найден
                    var firstStyle = styles.FirstOrDefault();
                     if(firstStyle != null) Debug.WriteLine($"Взяли первый стиль из <Styles>.");
                    return firstStyle;

                }
                else if (loadedObject is IStyle style) // Если корневой элемент <Style> или <ControlTheme>
                {
                    Debug.WriteLine($"Загружен корневой стиль/тема: {style.GetType().Name}");
                    return style;
                }
                else
                {
                     Debug.WriteLine("!!! ОШИБКА: Загруженный XAML не является Styles или IStyle.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! КРИТИЧЕСКАЯ ОШИБКА при загрузке/парсинге файла стиля: {ex}");
                return null;
            }
        }
    }
}