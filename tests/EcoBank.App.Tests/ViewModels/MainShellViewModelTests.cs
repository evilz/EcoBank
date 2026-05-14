using EcoBank.App.Services;
using EcoBank.App.ViewModels.Shell;
using EcoBank.Core.Application;
using Xunit;

namespace EcoBank.App.Tests.ViewModels;

public sealed class MainShellViewModelTests
{
    [Fact]
    public void Shell_exposes_client_banking_tabs_in_expected_order()
    {
        var vm = new MainShellViewModel(
            new UserContext(),
            new ShellNavigationContext(),
            homeVm: null!,
            accountsVm: null!,
            paymentsVm: null!,
            cardsVm: null!,
            profileVm: null!);

        var labels = vm.Tabs.Select(tab => tab.Label).ToArray();

        Assert.Equal(["Accueil", "Comptes", "Virements", "Cartes", "Profil"], labels);
    }
}
