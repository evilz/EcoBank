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
    public string SelectedCardMaskedPan => SelectedCard?.MaskedPan.NullIfEmpty() ?? "Non disponible";
    public string SelectedCardHolderName => SelectedCard?.HolderName.NullIfEmpty() ?? "Non renseigné";
    public string SelectedCardTypeLabel => FormatCardType(SelectedCard?.Type);
    public string SelectedCardStatusLabel => FormatCardStatus(SelectedCard?.Status);
    public string SelectedCardMonthlyLimitLabel => FormatLimit(SelectedCard?.MonthlyLimit, SelectedCard?.Currency);
    public string SelectedCardDailyLimitLabel => FormatLimit(SelectedCard?.DailyLimit, SelectedCard?.Currency);

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

    partial void OnSelectedCardChanged(Card? value)
    {
        OnPropertyChanged(nameof(SelectedCardMaskedPan));
        OnPropertyChanged(nameof(SelectedCardHolderName));
        OnPropertyChanged(nameof(SelectedCardTypeLabel));
        OnPropertyChanged(nameof(SelectedCardStatusLabel));
        OnPropertyChanged(nameof(SelectedCardMonthlyLimitLabel));
        OnPropertyChanged(nameof(SelectedCardDailyLimitLabel));
    }

    private static string FormatCardType(CardType? type) => type switch
    {
        CardType.Physical => "Physique",
        CardType.Virtual => "Virtuelle",
        CardType.Unknown => "Carte",
        _ => "Carte"
    };

    private static string FormatCardStatus(CardStatus? status) => status switch
    {
        CardStatus.Active => "Active",
        CardStatus.Blocked => "Bloquée",
        CardStatus.Cancelled => "Annulée",
        CardStatus.Unknown => "Statut inconnu",
        _ => "Statut inconnu"
    };

    private static string FormatLimit(decimal? amount, string? currency)
    {
        if (amount is null)
        {
            return "Non renseigné";
        }

        var symbol = string.Equals(currency, "EUR", StringComparison.OrdinalIgnoreCase)
            ? "€"
            : currency ?? "";
        return $"{amount.Value:N2} {symbol}".Trim();
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
