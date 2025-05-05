// ViewLocator.cs
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BoilingPot.ViewModels; // Пространство имен VM
using ReactiveUI; // Для IViewFor
using System;
using Splat; // Для Locator.Current

namespace BoilingPot
{
    // ViewLocator адаптирован для работы с ReactiveUI и Splat DI Resolver.
    public class ViewLocator : IDataTemplate
    {
        // Match остается прежним, подходит для всех наших ViewModel.
        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }

        // Build теперь будет пытаться получить View из DI контейнера через Splat.
        public Control? Build(object? data)
        {
            if (data is null) return null;

            // Получаем тип ViewModel
            var viewModelType = data.GetType();

            // Пытаемся найти зарегистрированный View для этого ViewModel через Splat.
            // ReactiveUI обычно регистрирует View как IViewFor<TViewModel>.
            // Locator.Current обратится к нашему Microsoft DI контейнеру благодаря UseMicrosoftDependencyResolver().
            var view = Locator.Current.GetService<IViewFor>(viewModelType.AssemblyQualifiedName);
            // Альтернатива: Искать по интерфейсу с ViewModel в качестве Generic параметра
            // var viewInterfaceType = typeof(IViewFor<>).MakeGenericType(viewModelType);
            // var view = Locator.Current.GetService(viewInterfaceType) as Control;

            if (view is Control controlView) // Если View найден через DI (Splat)
            {
                // Устанавливаем ViewModel для View (RxUI это делает часто автоматически, но для надежности можно)
                 if (view.ViewModel == null)
                 {
                      view.ViewModel = data;
                 }
                System.Diagnostics.Debug.WriteLine($"[ViewLocator] View '{controlView.GetType().Name}' найден через Splat для '{viewModelType.Name}'.");
                return controlView;
            }
            else // Если View не зарегистрирован в DI или не найден Splat'ом, пробуем старый метод
            {
                System.Diagnostics.Debug.WriteLine($"[ViewLocator] View для '{viewModelType.Name}' не найден через Splat, пробуем по имени...");
                var viewTypeName = viewModelType.FullName!.Replace("ViewModel", "View");
                var type = Type.GetType(viewTypeName);

                if (type != null)
                {
                    // Создаем View через Activator.CreateInstance
                    var control = (Control)Activator.CreateInstance(type)!;
                    // Попытаемся установить ViewModel вручную, если View реализует IViewFor
                     if (control is IViewFor ivf && ivf.ViewModel == null)
                     {
                          ivf.ViewModel = data;
                     }
                    System.Diagnostics.Debug.WriteLine($"[ViewLocator] View '{type.Name}' создан через Activator.");
                    return control;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"!!! ОШИБКА: [ViewLocator] View с именем '{viewTypeName}' не найден!");
                    return new TextBlock { Text = "View Not Found: " + viewTypeName };
                }
            }
        }
    }
}