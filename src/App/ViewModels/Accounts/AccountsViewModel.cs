using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.UseCases.Accounts;

namespace EcoBank.App.ViewModels.Accounts;

public partial class AccountsViewModel : ViewModelBase
{
    private readonly GetAccountsUseCase _getAccounts;
    private readonly GetVirtualIbansUseCase _getVirtualIbans;

    [ObservableProperty] private Account? _selectedAccount;

    public ObservableCollection<Account> Accounts { get; } = [];
    public ObservableCollection<VirtualIban> VirtualIbans { get; } = [];

    public bool HasVirtualIbans => VirtualIbans.Any();
    public string SelectedAccountLimitLabel => SelectedAccount is null
        ? "Sélectionnez un compte"
        : "Limites et soldes complémentaires disponibles selon le contrat Xpollens.";

    public AccountsViewModel(GetAccountsUseCase getAccounts, GetVirtualIbansUseCase getVirtualIbans)
    {
        _getAccounts = getAccounts;
        _getVirtualIbans = getVirtualIbans;
    }

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
            SelectedAccount ??= Accounts.FirstOrDefault();
        }
        catch (Exception) { ErrorMessage = "Impossible de charger les comptes."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void SelectAccount(Account account) => SelectedAccount = account;

    partial void OnSelectedAccountChanged(Account? value)
    {
        OnPropertyChanged(nameof(SelectedAccountLimitLabel));
        _ = LoadVirtualIbansAsync(value?.AccountId);
    }

    private async Task LoadVirtualIbansAsync(string? accountId)
    {
        try
        {
            VirtualIbans.Clear();
            foreach (var virtualIban in await _getVirtualIbans.ExecuteAsync(accountId))
            {
                VirtualIbans.Add(virtualIban);
            }
            OnPropertyChanged(nameof(HasVirtualIbans));
        }
        catch
        {
            VirtualIbans.Clear();
            OnPropertyChanged(nameof(HasVirtualIbans));
        }
    }
}
