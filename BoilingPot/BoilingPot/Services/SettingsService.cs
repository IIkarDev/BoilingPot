// Services/SettingsService.cs
using Avalonia; // Для Application.Current
using Avalonia.Controls.ApplicationLifetimes; // Для IClassicDesktopStyleApplicationLifetime
using Avalonia.Platform.Storage; // Для FilePicker
using BoilingPot.Models;
using System;
using System.Collections.Generic; // Для FilePickerFileType
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BoilingPot.ViewModels;
using BoilingPot.ViewModels.SettingsViewModels; // Для JSON конвертеров, если они здесь

namespace BoilingPot.Services
{
    public class SettingsService : ISettingsService
    {
        private const string DefaultSettingsFileName = "boiling_pot_app_settings.json"; // Файл по умолчанию
        private readonly string _defaultSettingsFilePath;

        // CurrentSettings теперь имеет приватный сеттер, меняется только через загрузку
        public AppSettings CurrentSettings { get; private set; }

        // Конструктор для JSON конвертеров (если они нужны при десериализации)
        private readonly JsonSerializerOptions _jsonOptions;

        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataPath, "BoilingPotApp");
            Directory.CreateDirectory(appSpecificFolder);
            _defaultSettingsFilePath = Path.Combine(appSpecificFolder, DefaultSettingsFileName);

            Debug.WriteLine($"[{this.GetType().Name}] Путь к файлу настроек по умолчанию: {_defaultSettingsFilePath}");

            // Настраиваем JSON опции с конвертерами
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = {
                    // new HorizontalAlignmentConverter(), // Предполагая, что они у тебя есть
                    // new VerticalAlignmentConverter(),
                    // new SymbolJsonConverter()
                }
            };

            CurrentSettings = new AppSettings(); // Инициализируем дефолтными значениями
            _ = LoadDefaultSettingsAsync(); // Загружаем настройки по умолчанию при старте
        }

        // --- Работа с файлом настроек ПО УМОЛЧАНИЮ (в AppData) ---
        public async Task LoadDefaultSettingsAsync()
        {
            Debug.WriteLine($"[{this.GetType().Name}] Попытка загрузки настроек ПО УМОЛЧАНИЮ из: {_defaultSettingsFilePath}");
            if (File.Exists(_defaultSettingsFilePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(_defaultSettingsFilePath);
                    var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                    if (loadedSettings != null)
                    {
                        CurrentSettings = loadedSettings;
                        Debug.WriteLine($"[{this.GetType().Name}] Настройки ПО УМОЛЧАНИЮ успешно загружены.");
                        // Здесь можно вызвать событие или обновить ViewModel, если это нужно СРАЗУ
                        // NotifySettingsLoaded();
                    }
                    else { Debug.WriteLine($"!!! [{this.GetType().Name}] ОШИБКА: Не удалось десериализовать настройки ПО УМОЛЧАНИЮ."); }
                }
                catch (Exception ex) { Debug.WriteLine($"!!! [{this.GetType().Name}] КРИТИЧЕСКАЯ ОШИБКА при загрузке настроек ПО УМОЛЧАНИЮ: {ex.Message}"); }
            }
            else
            {
                Debug.WriteLine($"[{this.GetType().Name}] Файл настроек ПО УМОЛЧАНИЮ не найден. Используются значения по умолчанию и будет создан новый файл.");
                await SaveDefaultSettingsAsync(); // Создаем файл с дефолтными настройками
            }
        }

        public async Task SaveDefaultSettingsAsync()
        {
            Debug.WriteLine($"[{this.GetType().Name}] Попытка сохранения настроек ПО УМОЛЧАНИЮ в: {_defaultSettingsFilePath}");
            try
            {
                string json = JsonSerializer.Serialize(CurrentSettings, _jsonOptions);
                await File.WriteAllTextAsync(_defaultSettingsFilePath, json);
                Debug.WriteLine($"[{this.GetType().Name}] Настройки ПО УМОЛЧАНИЮ успешно сохранены.");
            }
            catch (Exception ex) { Debug.WriteLine($"!!! [{this.GetType().Name}] КРИТИЧЕСКАЯ ОШИБКА при сохранении настроек ПО УМОЛЧАНИЮ: {ex.Message}"); }
        }

        // --- Работа с файлами через ДИАЛОГИ ---
        public async Task<bool> LoadSettingsFromFileDialogAsync()
        {
            Debug.WriteLine($"[{this.GetType().Name}] LoadSettingsFromFileDialogAsync: Открытие диалога выбора файла...");
            var topLevel = AppExtensions.GetTopLevel(Application.Current);
            if (topLevel == null)
            {
                Debug.WriteLine("!!! TopLevel не найден для диалога загрузки.");
                return false;
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Загрузить файл настроек",
                AllowMultiple = false,
                FileTypeFilter = new[] { JsonFileType }
            });

            if (files.Count >= 1)
            {
                string filePath = files[0].Path.LocalPath;
                Debug.WriteLine($"[{this.GetType().Name}] Выбран файл для загрузки: {filePath}");
                try
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                    if (loadedSettings != null)
                    {
                        CurrentSettings = loadedSettings; // Обновляем текущие настройки
                        Debug.WriteLine($"[{this.GetType().Name}] Настройки успешно загружены из файла: {Path.GetFileName(filePath)}");
                        // Важно: После загрузки нужно обновить ViewModel!
                        // Это можно сделать через событие, на которое подпишется MainViewModel,
                        // или MainViewModel сам вызовет метод применения настроек.
                        // NotifySettingsLoaded();
                        return true;
                    }
                    else { Debug.WriteLine($"!!! [{this.GetType().Name}] ОШИБКА: Не удалось десериализовать настройки из файла: {Path.GetFileName(filePath)}"); }
                }
                catch (Exception ex) { Debug.WriteLine($"!!! [{this.GetType().Name}] КРИТИЧЕСКАЯ ОШИБКА при загрузке настроек из файла '{filePath}': {ex.Message}"); }
            }
            else { Debug.WriteLine($"[{this.GetType().Name}] Выбор файла для загрузки отменен или не удалось получить путь."); }
            return false;
        }

        public async Task<bool> SaveSettingsToFileDialogAsync()
        {
            Debug.WriteLine($"[{this.GetType().Name}] SaveSettingsToFileDialogAsync: Открытие диалога сохранения файла...");
            var topLevel = AppExtensions.GetTopLevel(Application.Current);
            if (topLevel == null)
            {
                 Debug.WriteLine("!!! TopLevel не найден для диалога сохранения.");
                 return false;
            }

            // Предлагаем имя файла по умолчанию
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить настройки как...",
                SuggestedFileName = "BoilingPot_Settings",
                DefaultExtension = "json",
                FileTypeChoices = new[] { JsonFileType }
            });

            if (file != null)
            {
                string filePath = file.Path.LocalPath;
                Debug.WriteLine($"[{this.GetType().Name}] Файл для сохранения выбран: {filePath}");
                try
                {
                    string json = JsonSerializer.Serialize(CurrentSettings, _jsonOptions);
                    await File.WriteAllTextAsync(filePath, json);
                    Debug.WriteLine($"[{this.GetType().Name}] Настройки успешно сохранены в файл: {Path.GetFileName(filePath)}");
                    return true;
                }
                catch (Exception ex) { Debug.WriteLine($"!!! [{this.GetType().Name}] КРИТИЧЕСКАЯ ОШИБКА при сохранении настроек в файл '{filePath}': {ex.Message}"); }
            }
            else { Debug.WriteLine($"[{this.GetType().Name}] Сохранение файла отменено или не удалось получить путь."); }
            return false;
        }

        // Вспомогательный фильтр для диалогов
        private static FilePickerFileType JsonFileType { get; } = new("JSON Files")
        {
            Patterns = new[] { "*.json" }
        };

        // (Опционально) Событие для уведомления об изменении/загрузке настроек
        // public event Action? SettingsChanged;
        // private void NotifySettingsLoaded() => SettingsChanged?.Invoke();
    }

    // JSON Конвертеры (если они не в отдельном файле BoilingPot.Converters)
    // Если они в отдельном файле, эти классы здесь не нужны.
    // public class HorizontalAlignmentConverter : JsonConverter<HorizontalAlignment> { ... }
    // public class VerticalAlignmentConverter : JsonConverter<VerticalAlignment> { ... }
    // public class SymbolJsonConverter : JsonConverter<Symbol> { ... }
}