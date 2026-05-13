using Avalonia.Controls;
using EcoBank.App.ViewModels.Profile;

namespace EcoBank.App.Views.Profile;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is ProfileViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
