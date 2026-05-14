using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoBank.App.Services;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Domain.Security;
using EcoBank.Core.UseCases.Cards;
using EcoBank.Core.UseCases.Security;

namespace EcoBank.App.ViewModels.Cards;

public partial class CardsViewModel : ViewModelBase
{
    private readonly UserContext _userContext;
    private readonly GetCardsUseCase _getCards;
    private readonly ToggleCardLockUseCase _toggleCardLock;
    private readonly RequestStrongAuthenticationUseCase _requestStrongAuthentication;
    private readonly CreatePhysicalCardUseCase _createPhysicalCard;
    private readonly CreateVirtualCardUseCase _createVirtualCard;
    private readonly ShellNavigationContext _shellNav;

    [ObservableProperty] private Card? _selectedCard;
    [ObservableProperty] private bool _isDetailVisible;
    [ObservableProperty] private string _securityMessage = "Les données sensibles nécessitent une authentification forte.";
    [ObservableProperty] private string _cardCreationMessage = "";
    public ObservableCollection<Card> Cards { get; } = [];
    public bool HasCards => Cards.Any();
    public bool HasNoCards => !HasCards;
    public string SelectedCardMaskedPan => SelectedCard?.MaskedPan.NullIfEmpty() ?? "Non disponible";
    public string SelectedCardHolderName => SelectedCard?.HolderName.NullIfEmpty() ?? "Non renseigné";
    public string SelectedCardTypeLabel => FormatCardType(SelectedCard?.Type);
    public string SelectedCardStatusLabel => FormatCardStatus(SelectedCard?.Status);
    public string SelectedCardMonthlyLimitLabel => FormatLimit(SelectedCard?.MonthlyLimit, SelectedCard?.Currency);
    public string SelectedCardDailyLimitLabel => FormatLimit(SelectedCard?.DailyLimit, SelectedCard?.Currency);

    public CardsViewModel(
        UserContext userContext,
        GetCardsUseCase getCards,
        ToggleCardLockUseCase toggleCardLock,
        RequestStrongAuthenticationUseCase requestStrongAuthentication,
        CreatePhysicalCardUseCase createPhysicalCard,
        CreateVirtualCardUseCase createVirtualCard,
        ShellNavigationContext shellNav)
    {
        _userContext = userContext;
        _getCards = getCards;
        _toggleCardLock = toggleCardLock;
        _requestStrongAuthentication = requestStrongAuthentication;
        _createPhysicalCard = createPhysicalCard;
        _createVirtualCard = createVirtualCard;
        _shellNav = shellNav;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        CardCreationMessage = "";
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
    private void GoBack() => _shellNav.GoToHome();

    [RelayCommand]
    private async Task CreateCardAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        CardCreationMessage = "";
        try
        {
            var card = await _createPhysicalCard.ExecuteAsync(ct);
            Cards.Add(card);
            SelectedCard = card;
            OnPropertyChanged(nameof(HasCards));
            OnPropertyChanged(nameof(HasNoCards));
            CardCreationMessage = "Carte physique créée avec succès.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de créer la carte.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateVirtualCardAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        CardCreationMessage = "";
        try
        {
            var card = await _createVirtualCard.ExecuteAsync(ct);
            Cards.Add(card);
            SelectedCard = card;
            OnPropertyChanged(nameof(HasCards));
            OnPropertyChanged(nameof(HasNoCards));
            CardCreationMessage = "Carte virtuelle créée avec succès.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de créer la carte virtuelle.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ManageCardAsync(CancellationToken ct)
    {
        if (SelectedCard is null)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var shouldLock = SelectedCard.Status == CardStatus.Active;
            await _toggleCardLock.ExecuteAsync(SelectedCard.CardId, shouldLock, ct);
            SecurityMessage = shouldLock ? "Carte verrouillée." : "Carte déverrouillée.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de modifier le statut de la carte.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ManageAlerts() => SecurityMessage = "Les plafonds et alertes carte sont affichés dans le détail de la carte.";

    [RelayCommand]
    private async Task EnrollApplePayAsync(CancellationToken ct) =>
        await RequestWalletEnrollmentAsync("ApplePay", "Enrôlement Apple Pay lancé.", ct);

    [RelayCommand]
    private async Task EnrollXPayAsync(CancellationToken ct) =>
        await RequestWalletEnrollmentAsync("XPay", "Enrôlement XPay lancé.", ct);

    [RelayCommand]
    private void ViewCardDetails() => IsDetailVisible = true;

    [RelayCommand]
    private void ShowCardList() => IsDetailVisible = false;

    [RelayCommand]
    private async Task RequestPinDisplayAsync(CancellationToken ct)
    {
        if (SelectedCard is null || _userContext.SelectedUser is null)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var result = await _requestStrongAuthentication.ExecuteAsync(
                new StrongAuthenticationRequest(
                    _userContext.SelectedUser.AppUserId,
                    "DisplayPin",
                    SelectedCard.CardId,
                    null,
                    SelectedCard.Currency),
                ct);
            SecurityMessage = result.Status == StrongAuthenticationStatus.Approved
                ? "PIN autorisé par authentification forte."
                : "Demande d'authentification forte envoyée.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de demander l'affichage du PIN.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RequestWalletEnrollmentAsync(string walletProvider, string successMessage, CancellationToken ct)
    {
        if (SelectedCard is null || _userContext.SelectedUser is null)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var result = await _requestStrongAuthentication.ExecuteAsync(
                new StrongAuthenticationRequest(
                    _userContext.SelectedUser.AppUserId,
                    $"WalletEnrollment:{walletProvider}",
                    SelectedCard.CardId,
                    null,
                    SelectedCard.Currency),
                ct);
            SecurityMessage = result.Status == StrongAuthenticationStatus.Approved
                ? successMessage
                : $"Demande d'authentification forte envoyée pour {walletProvider}.";
        }
        catch (Exception)
        {
            ErrorMessage = $"Impossible de lancer l'enrôlement {walletProvider}.";
        }
        finally
        {
            IsBusy = false;
        }
    }

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
