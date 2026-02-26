using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.UseCases.Accounts;

namespace EcoBank.App.ViewModels.Accounts;

public partial class AccountsViewModel : ViewModelBase
{
    private readonly GetAccountsUseCase _getAccounts;

    [ObservableProperty] private Account? _selectedAccount;

    public ObservableCollection<Account> Accounts { get; } = [];

    public AccountsViewModel(GetAccountsUseCase getAccounts) => _getAccounts = getAccounts;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        try
        {
            var accounts = await _getAccounts.ExecuteAsync(ct);
            Accounts.Clear();
            foreach (var a in accounts) Accounts.Add(a);
        }
        catch (Exception) { ErrorMessage = "Impossible de charger les comptes."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void SelectAccount(Account account) => SelectedAccount = account;
}
