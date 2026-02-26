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
    public ObservableCollection<Account> Accounts { get; } = [];
    public ObservableCollection<Operation> RecentOperations { get; } = [];

    public string WelcomeMessage =>
        _userContext.SelectedUser is { } u
            ? $"Bonjour, {u.FirstName ?? u.AppUserId} !"
            : "Bonjour !";

    public HomeViewModel(UserContext userContext, GetAccountsUseCase getAccounts, GetOperationsUseCase getOperations)
    {
        _userContext = userContext;
        _getAccounts = getAccounts;
        _getOperations = getOperations;
    }

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

            var ops = await _getOperations.ExecuteAsync(pageSize: 5, ct: ct);
            RecentOperations.Clear();
            foreach (var o in ops) RecentOperations.Add(o);
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
