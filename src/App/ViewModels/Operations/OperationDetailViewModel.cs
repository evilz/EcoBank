using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.Ports;

namespace EcoBank.App.ViewModels.Operations;

public partial class OperationDetailViewModel(IOperationRepository operationRepository) : ViewModelBase
{
    [ObservableProperty] private Operation? _operation;

    [RelayCommand]
    private async Task LoadAsync(string operationId, CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        try { Operation = await operationRepository.GetOperationAsync(operationId, ct); }
        catch (Exception) { ErrorMessage = "Impossible de charger l'opération."; }
        finally { IsBusy = false; }
    }
}
