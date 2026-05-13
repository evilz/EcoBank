using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Operations;

namespace EcoBank.App.ViewModels.Operations;

public partial class OperationsViewModel : ViewModelBase
{
    private readonly GetAccountsUseCase _getAccounts;
    private readonly GetOperationsUseCase _getOperations;
    private readonly ShellNavigationContext _shellNav;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private OperationType? _filterType;
    [ObservableProperty] private DateTimeOffset? _filterFrom;
    [ObservableProperty] private DateTimeOffset? _filterTo;
    [ObservableProperty] private Operation? _selectedOperation;

    public ObservableCollection<Operation> Operations { get; } = [];
    public bool HasOperations => Operations.Any();
    public bool HasNoOperations => !HasOperations;

    public OperationsViewModel(GetAccountsUseCase getAccounts, GetOperationsUseCase getOperations, ShellNavigationContext shellNav)
    {
        _getAccounts = getAccounts;
        _getOperations = getOperations;
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
            var accountId = accounts.FirstOrDefault()?.AccountId;
            if (string.IsNullOrEmpty(accountId))
            {
                Operations.Clear();
                OnPropertyChanged(nameof(HasOperations));
                OnPropertyChanged(nameof(HasNoOperations));
                return;
            }
            var ops = await _getOperations.ExecuteAsync(
                accountId: accountId,
                type: FilterType,
                from: FilterFrom,
                to: FilterTo,
                search: SearchQuery.NullIfEmpty(),
                ct: ct);
            Operations.Clear();
            foreach (var o in ops) Operations.Add(o);
            OnPropertyChanged(nameof(HasOperations));
            OnPropertyChanged(nameof(HasNoOperations));
        }
        catch (Exception) { ErrorMessage = "Impossible de charger les opérations."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void GoBack() => _shellNav.GoToHome();
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
