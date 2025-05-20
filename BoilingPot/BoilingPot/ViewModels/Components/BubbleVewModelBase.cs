// ViewModels/Components/BubbleViewModel.cs
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using BoilingPot.Models;

namespace BoilingPot.ViewModels.Components
{
    public partial class BubbleViewModelBase : ViewModelBase, IBubbleViewModel // ViewModelBase наследуется от ReactiveObject
    {
        [Reactive] public double X { get; set; }
        [Reactive] public double Y { get; set; }
        [Reactive] public double Size { get; set; } = 10; // Размер по умолчанию
        [Reactive] public IBrush ColorBrush { get; set; } = Brushes.Transparent;
        
        public double AquariumWidth { get; set; }  // Внутренняя ширина области для пузырьков
        public double AquariumHeight { get; set; }  // Внутренняя высота области для пузырьков

        public BubblePhysics? PhysicsLogic { get; set; } // <<< ВОТ ЭТО СВОЙСТВО

        public BubbleViewModelBase()
        {
            // Debug.WriteLine($"[{this.GetType().Name}] Конструктор RxUI: BubbleViewModel создан.");
        }
    }
}