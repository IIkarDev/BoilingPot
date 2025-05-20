using Avalonia.Media;
using BoilingPot.Models;
using ReactiveUI.Fody.Helpers;

namespace BoilingPot.ViewModels.Components;

public interface IBubbleViewModel
{
    
    [Reactive] public double X { get; set; }
    [Reactive] public double Y { get; set; }
    [Reactive] public double Size { get; set; }// Размер по умолчанию
    [Reactive] public IBrush ColorBrush { get; set; }
        
    public double AquariumWidth { get; set; }  // Внутренняя ширина области для пузырьков
    public double AquariumHeight { get; set; }  // Внутренняя высота области для пузырьков

    public BubblePhysics? PhysicsLogic { get; set; } // <<< ВОТ ЭТО СВОЙСТВО

}