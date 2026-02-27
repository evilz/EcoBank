using Avalonia.Controls;
using EcoBank.App.ViewModels.Operations;

namespace EcoBank.App.Views.Operations;

public partial class OperationsView : UserControl
{
    public OperationsView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is OperationsViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
