using Avalonia.Controls;
using EcoBank.App.ViewModels.Accounts;

namespace EcoBank.App.Views.Accounts;

public partial class AccountsView : UserControl
{
    public AccountsView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is AccountsViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
