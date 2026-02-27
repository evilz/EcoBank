using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Operations;

namespace EcoBank.App.ViewModels.Operations;

public partial class OperationsViewModel : ViewModelBase
{
    private readonly GetAccountsUseCase _getAccounts;
    private readonly GetOperationsUseCase _getOperations;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private OperationType? _filterType;
    [ObservableProperty] private DateTimeOffset? _filterFrom;
    [ObservableProperty] private DateTimeOffset? _filterTo;
    [ObservableProperty] private Operation? _selectedOperation;

    public ObservableCollection<Operation> Operations { get; } = [];

    public OperationsViewModel(GetAccountsUseCase getAccounts, GetOperationsUseCase getOperations)
    {
        _getAccounts = getAccounts;
        _getOperations = getOperations;
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
        }
        catch (Exception) { ErrorMessage = "Impossible de charger les opérations."; }
        finally { IsBusy = false; }
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
