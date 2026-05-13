using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.UseCases.Cards;

namespace EcoBank.App.ViewModels.Cards;

public partial class CardsViewModel : ViewModelBase
{
    private readonly GetCardsUseCase _getCards;

    [ObservableProperty] private Card? _selectedCard;
    [ObservableProperty] private bool _isDetailVisible;
    public ObservableCollection<Card> Cards { get; } = [];
    public bool HasCards => Cards.Any();
    public bool HasNoCards => !HasCards;

    public CardsViewModel(GetCardsUseCase getCards) => _getCards = getCards;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        try
        {
            var cards = await _getCards.ExecuteAsync(ct);
            Cards.Clear();
            foreach (var c in cards) Cards.Add(c);
            SelectedCard ??= Cards.FirstOrDefault();
            OnPropertyChanged(nameof(HasCards));
            OnPropertyChanged(nameof(HasNoCards));
        }
        catch (Exception) { ErrorMessage = "Impossible de charger les cartes."; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ManageCard() { /* TODO: navigate to card management */ }

    [RelayCommand]
    private void ManageAlerts() { /* TODO: navigate to card alerts */ }

    [RelayCommand]
    private void ViewCardDetails() => IsDetailVisible = true;

    [RelayCommand]
    private void ShowCardList() => IsDetailVisible = false;
}
