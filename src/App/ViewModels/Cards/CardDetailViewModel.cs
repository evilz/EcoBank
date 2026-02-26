using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.UseCases.Cards;

namespace EcoBank.App.ViewModels.Cards;

public partial class CardDetailViewModel(
    ToggleCardLockUseCase toggleLock) : ViewModelBase
{
    [ObservableProperty] private Card? _card;

    [RelayCommand]
    private async Task ToggleLockAsync(CancellationToken ct)
    {
        if (Card is null) return;
        IsBusy = true;
        ClearError();
        try
        {
            var shouldLock = Card.Status == CardStatus.Active;
            await toggleLock.ExecuteAsync(Card.CardId, shouldLock, ct);
            // TODO: reload card after toggle
        }
        catch (Exception) { ErrorMessage = "Impossible de modifier le statut de la carte."; }
        finally { IsBusy = false; }
    }
}
