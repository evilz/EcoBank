using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Operations;

namespace EcoBank.App.ViewModels.Home;

public partial class HomeViewModel : ViewModelBase
{
    private readonly UserContext _userContext;
    private readonly GetAccountsUseCase _getAccounts;
    private readonly GetOperationsUseCase _getOperations;

    [ObservableProperty] private decimal _totalBalance;
    [ObservableProperty] private string _currency = "EUR";
    [ObservableProperty] private bool _isBalanceVisible = true;

    // Solde à venir : calculé depuis les opérations Pending (stub, à remplacer par API)
    [ObservableProperty] private decimal _upcomingBalance;

    public ObservableCollection<Account> Accounts { get; } = [];
    public ObservableCollection<Operation> RecentOperations { get; } = [];

    public Account? FirstAccount => Accounts.FirstOrDefault();

    public string FirstAccountLabel => FirstAccount?.Label ?? "Compte de Dépôt";
    public string FirstAccountNumber => FirstAccount?.Iban ?? "—";
    public decimal FirstAccountBalance => FirstAccount?.Balance ?? 0m;
    public string FirstAccountCurrency => FirstAccount?.Currency ?? "EUR";

    public string WelcomeMessage =>
        _userContext.SelectedUser is { } u
            ? $"Bonjour {u.FirstName ?? u.AppUserId}"
            : "Bonjour";

    public HomeViewModel(UserContext userContext, GetAccountsUseCase getAccounts, GetOperationsUseCase getOperations)
    {
        _userContext = userContext;
        _getAccounts = getAccounts;
        _getOperations = getOperations;
    }

    [RelayCommand]
    private void ToggleBalance() => IsBalanceVisible = !IsBalanceVisible;

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        try
        {
            var accounts = await _getAccounts.ExecuteAsync(ct);
            Accounts.Clear();
            foreach (var a in accounts) Accounts.Add(a);
            TotalBalance = accounts.Sum(a => a.Balance);
            Currency = accounts.FirstOrDefault()?.Currency ?? "EUR";

            OnPropertyChanged(nameof(FirstAccount));
            OnPropertyChanged(nameof(FirstAccountLabel));
            OnPropertyChanged(nameof(FirstAccountNumber));
            OnPropertyChanged(nameof(FirstAccountBalance));
            OnPropertyChanged(nameof(FirstAccountCurrency));

            var firstAccountId = accounts.FirstOrDefault()?.AccountId;

            var ops = await _getOperations.ExecuteAsync(
                accountId: firstAccountId,
                pageSize: 3,
                ct: ct);
            RecentOperations.Clear();
            foreach (var o in ops) RecentOperations.Add(o);

            // Calcul du solde à venir : solde courant + opérations Pending
            // On charge une page large pour ne pas manquer les Pending hors des 3 dernières opérations.
            // TODO: remplacer par champ API Account.UpcomingBalance quand disponible
            if (firstAccountId is not null)
            {
                var allPendingOps = await _getOperations.ExecuteAsync(
                    accountId: firstAccountId,
                    pageSize: 100,
                    ct: ct);
                UpcomingBalance = FirstAccountBalance
                    + allPendingOps
                        .Where(o => o.Status == OperationStatus.Pending)
                        .Sum(o => o.Amount);
            }
            else
            {
                UpcomingBalance = FirstAccountBalance;
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger le tableau de bord.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
