using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.UseCases.Accounts;

namespace EcoBank.App.ViewModels.Accounts;

public partial class AccountsViewModel : ViewModelBase
{
    private readonly GetAccountsUseCase _getAccounts;
    private readonly GetVirtualIbansUseCase _getVirtualIbans;
    private readonly ListBankStatementsUseCase _listBankStatements;
    private readonly GetBankStatementUseCase _getBankStatement;
    private readonly ShellNavigationContext _shellNav;

    [ObservableProperty] private Account? _selectedAccount;
    [ObservableProperty] private BankStatement? _selectedBankStatement;
    [ObservableProperty] private bool _isStatementDetailVisible;

    public ObservableCollection<Account> Accounts { get; } = [];
    public ObservableCollection<VirtualIban> VirtualIbans { get; } = [];
    public ObservableCollection<BankStatement> BankStatements { get; } = [];

    public bool HasVirtualIbans => VirtualIbans.Any();
    public bool HasBankStatements => BankStatements.Any();
    public string SelectedAccountLimitLabel => SelectedAccount is null
        ? "Sélectionnez un compte"
        : "Limites et soldes complémentaires disponibles selon le contrat Xpollens.";

    public AccountsViewModel(
        GetAccountsUseCase getAccounts,
        GetVirtualIbansUseCase getVirtualIbans,
        ListBankStatementsUseCase listBankStatements,
        GetBankStatementUseCase getBankStatement,
        ShellNavigationContext shellNav)
    {
        _getAccounts = getAccounts;
        _getVirtualIbans = getVirtualIbans;
        _listBankStatements = listBankStatements;
        _getBankStatement = getBankStatement;
        _shellNav = shellNav;
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

            // Load bank statements for the selected account
            await LoadBankStatementsAsync(SelectedAccount?.AccountId, ct);
        }
        catch (Exception) { ErrorMessage = "Impossible de charger les comptes."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void GoBack() => _shellNav.GoToHome();

    [RelayCommand]
    private void SelectAccount(Account account) => SelectedAccount = account;

    [RelayCommand]
    private async Task OpenBankStatementAsync(BankStatement statement, CancellationToken ct)
    {
        if (statement is null) return;
        IsBusy = true;
        ClearError();
        try
        {
            var detail = await _getBankStatement.ExecuteAsync(statement.BankStatementId, ct);
            SelectedBankStatement = detail ?? statement;
            IsStatementDetailVisible = true;
        }
        catch (Exception) { ErrorMessage = "Impossible de charger le relevé."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void CloseStatementDetail() => IsStatementDetailVisible = false;

    partial void OnSelectedAccountChanged(Account? value)
    {
        OnPropertyChanged(nameof(SelectedAccountLimitLabel));
        _ = LoadVirtualIbansAsync(value?.AccountId);
        _ = LoadBankStatementsAsync(value?.AccountId);
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

    private async Task LoadBankStatementsAsync(string? accountId = null, CancellationToken ct = default)
    {
        try
        {
            BankStatements.Clear();
            foreach (var statement in await _listBankStatements.ExecuteAsync(accountId, ct))
            {
                BankStatements.Add(statement);
            }
            OnPropertyChanged(nameof(HasBankStatements));
        }
        catch
        {
            BankStatements.Clear();
            OnPropertyChanged(nameof(HasBankStatements));
        }
    }
}
