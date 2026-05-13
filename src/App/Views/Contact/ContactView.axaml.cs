using Avalonia.Controls;
using EcoBank.App.ViewModels.Contact;

namespace EcoBank.App.Views.Contact;

public partial class ContactView : UserControl
{
    public ContactView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is ContactViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}

