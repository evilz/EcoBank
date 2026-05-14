using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
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
    private readonly ShellNavigationContext _shellNav;

    [ObservableProperty] private decimal _totalBalance;
    [ObservableProperty] private string _currency = "EUR";
    [ObservableProperty] private bool _isBalanceVisible = true;

    // Solde à venir : calculé depuis les opérations Pending (stub, à remplacer par API)
    [ObservableProperty] private decimal _upcomingBalance;

    public ObservableCollection<Account> Accounts { get; } = [];
    public ObservableCollection<Operation> RecentOperations { get; } = [];
    public ObservableCollection<string> DashboardAlerts { get; } = [];

    public bool HasAccounts => Accounts.Any();
    public bool HasNoAccounts => !HasAccounts;
    public decimal DisplayTotalBalance => TotalBalance;
    public string DisplayCurrency => Currency;
    public string EmptyAccountsMessage => string.IsNullOrWhiteSpace(ErrorMessage)
        ? "Aucun compte disponible"
        : ErrorMessage;

    public Account? FirstAccount => Accounts.FirstOrDefault();

    public string FirstAccountLabel => FirstAccount?.Label ?? "Compte de Dépôt";
    public string FirstAccountNumber => FirstAccount?.Iban ?? "—";
    public decimal FirstAccountBalance => FirstAccount?.Balance ?? 0m;
    public string FirstAccountCurrency => FirstAccount?.Currency ?? "EUR";

    public string WelcomeMessage =>
        _userContext.SelectedUser is { } u
            ? string.IsNullOrWhiteSpace(u.FirstName) ? "Bonjour" : $"Bonjour {u.FirstName}"
            : "Bonjour";

    public string GreetingName =>
        _userContext.SelectedUser is { } u
            ? (string.IsNullOrWhiteSpace(u.FirstName) ? string.Empty : u.FirstName)
            : "";

    public HomeViewModel(
        UserContext userContext,
        GetAccountsUseCase getAccounts,
        GetOperationsUseCase getOperations,
        ShellNavigationContext shellNav)
    {
        _userContext = userContext;
        _getAccounts = getAccounts;
        _getOperations = getOperations;
        _shellNav = shellNav;
    }

    [RelayCommand]
    private void ToggleBalance() => IsBalanceVisible = !IsBalanceVisible;

    [RelayCommand]
    private void GoToTransfer() => _shellNav.GoToPayments();

    [RelayCommand]
    private void GoToPayment() => _shellNav.GoToPayments();

    [RelayCommand]
    private void GoToAccounts() => _shellNav.GoToAccounts();

    [RelayCommand]
    private void GoToNotifications() => _shellNav.GoToProfile();

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

            OnPropertyChanged(nameof(HasAccounts));
            OnPropertyChanged(nameof(HasNoAccounts));
            OnPropertyChanged(nameof(DisplayTotalBalance));
            OnPropertyChanged(nameof(DisplayCurrency));
            OnPropertyChanged(nameof(EmptyAccountsMessage));
            OnPropertyChanged(nameof(FirstAccount));
            OnPropertyChanged(nameof(FirstAccountLabel));
            OnPropertyChanged(nameof(FirstAccountNumber));
            OnPropertyChanged(nameof(FirstAccountBalance));
            OnPropertyChanged(nameof(FirstAccountCurrency));

            var firstAccountId = accounts.FirstOrDefault()?.AccountId;
            DashboardAlerts.Clear();
            if (accounts.Count == 0)
            {
                DashboardAlerts.Add("Aucun compte disponible pour cet utilisateur.");
            }
            else
            {
                DashboardAlerts.Add("Comptes et paiements synchronisés avec Xpollens.");
            }

            var ops = await _getOperations.ExecuteAsync(
                accountId: firstAccountId,
                pageSize: 3,
                ct: ct);
            RecentOperations.Clear();
            foreach (var o in ops) RecentOperations.Add(o);

            // Calcul du solde à venir : solde courant + opérations Pending.
            // L'API compte ne fournit pas toujours un solde à venir dédié.
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

            OnPropertyChanged(nameof(DashboardAlerts));
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger le tableau de bord.";
            OnPropertyChanged(nameof(EmptyAccountsMessage));
        }
        finally
        {
            IsBusy = false;
        }
    }

}
