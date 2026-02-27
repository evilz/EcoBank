using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.Ports;
using EcoBank.Core.UseCases.Operations;

namespace EcoBank.App.ViewModels.Accounts;

public partial class AccountDetailViewModel(
    IAccountRepository accountRepository,
    GetOperationsUseCase getOperations) : ViewModelBase
{
    [ObservableProperty] private Account? _account;
    public ObservableCollection<Operation> Operations { get; } = [];

    [RelayCommand]
    private async Task LoadAsync(string accountId, CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        try
        {
            Account = await accountRepository.GetAccountAsync(accountId, ct);
            var ops = await getOperations.ExecuteAsync(accountId: accountId, ct: ct);
            Operations.Clear();
            foreach (var o in ops) Operations.Add(o);
        }
        catch (Exception) { ErrorMessage = "Impossible de charger le détail du compte."; }
        finally { IsBusy = false; }
    }
}
