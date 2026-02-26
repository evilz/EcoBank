using Avalonia.Controls;
using EcoBank.App.ViewModels.Cards;

namespace EcoBank.App.Views.Cards;

public partial class CardsView : UserControl
{
    public CardsView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is CardsViewModel vm)
                vm.LoadCommand.Execute(null);
        };
    }
}
