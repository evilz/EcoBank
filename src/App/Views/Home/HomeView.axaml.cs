using Avalonia.Controls;
using EcoBank.App.ViewModels.Home;

namespace EcoBank.App.Views.Home;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is HomeViewModel vm)
                vm.RefreshCommand.Execute(null);
        };
    }
}
