using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BoilingPot.ViewModels; // Убедитесь, что пространство имен MolecularViewModel здесь доступно

namespace BoilingPot.Views;

public partial class MolecularView : UserControl
{
    public MolecularView()
    {
        InitializeComponent();

        // Вызываем инициализацию при загрузке представления
        // this.AttachedToVisualTree += OnAttachedToVisualTree;
    }

    // private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    // {
    //     // Проверяем, что DataContext имеет тип MolecularViewModel
    //     if (DataContext is MolecularViewModel viewModel)
    //     {
    //         // Теперь вызываем метод у MolecularViewModel
    //         // Убедитесь, что этот метод существует в MolecularViewModel!
    //         viewModel.InitializeMolecularView();
    //     }
    //     // Удаляем обработчик после первого вызова, если инициализация нужна только один раз
    //     // this.AttachedToVisualTree -= OnAttachedToVisualTree;
    // }
    //
    // // Если вы хотите отписаться при отсоединении от дерева:
    // // protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    // // {
    // //     base.OnDetachedFromVisualTree(e);
    // //     this.AttachedToVisualTree -= OnAttachedToVisualTree;
    // // }
}