using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Domain.Payments;
using EcoBank.Core.UseCases.Accounts;
using EcoBank.Core.UseCases.Payments;

namespace EcoBank.App.ViewModels.Contact;

public partial class ContactViewModel : ViewModelBase
{
    private readonly UserContext _userContext;
    private readonly GetAccountsUseCase _getAccounts;
    private readonly GetBeneficiariesUseCase _getBeneficiaries;
    private readonly GetMandatesUseCase _getMandates;
    private readonly CreateSepaTransferUseCase _createSepaTransfer;

    [ObservableProperty]
    private string _message = "Sélectionnez un compte et un bénéficiaire pour préparer un paiement.";

    [ObservableProperty] private Account? _selectedAccount;
    [ObservableProperty] private Beneficiary? _selectedBeneficiary;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string _paymentLabel = "";
    [ObservableProperty] private PaymentModeOption? _selectedPaymentMode;
    [ObservableProperty] private PaymentResult? _lastPayment;

    public ObservableCollection<Account> Accounts { get; } = [];
    public ObservableCollection<Beneficiary> Beneficiaries { get; } = [];
    public ObservableCollection<Mandate> Mandates { get; } = [];
    public ObservableCollection<PaymentModeOption> PaymentModes { get; } =
    [
        new() { Label = "Virement SEPA standard", Mode = PaymentExecutionMode.Standard },
        new() { Label = "Virement instantané", Mode = PaymentExecutionMode.Instant },
    ];

    public bool CanSubmitPayment =>
        SelectedAccount is not null
        && SelectedBeneficiary is not null
        && Amount > 0
        && SelectedPaymentMode is not null;

    public string PaymentValidationMessage =>
        SelectedAccount is null ? "Sélectionnez un compte source." :
        SelectedBeneficiary is null ? "Sélectionnez un bénéficiaire." :
        Amount <= 0 ? "Le montant doit être supérieur à 0." :
        SelectedPaymentMode is null ? "Sélectionnez un mode d’exécution." :
        "";

    public string LastPaymentLabel => LastPayment is null
        ? "Aucun paiement soumis"
        : $"{LastPayment.PaymentId} - {FormatStatus(LastPayment.Status)}";

    public string UserDisplayName =>
        _userContext.SelectedUser is { } u
            ? $"{u.FirstName} {u.LastName}".Trim().NullIfEmpty() ?? u.AppUserId
            : "Utilisateur";

    public ContactViewModel(
        UserContext userContext,
        GetAccountsUseCase getAccounts,
        GetBeneficiariesUseCase getBeneficiaries,
        GetMandatesUseCase getMandates,
        CreateSepaTransferUseCase createSepaTransfer)
    {
        _userContext = userContext;
        _getAccounts = getAccounts;
        _getBeneficiaries = getBeneficiaries;
        _getMandates = getMandates;
        _createSepaTransfer = createSepaTransfer;
        _selectedPaymentMode = PaymentModes.FirstOrDefault();
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ClearError();
        try
        {
            Accounts.Clear();
            foreach (var account in await _getAccounts.ExecuteAsync(ct))
            {
                Accounts.Add(account);
            }
            SelectedAccount ??= Accounts.FirstOrDefault();

            Beneficiaries.Clear();
            foreach (var beneficiary in await _getBeneficiaries.ExecuteAsync(ct))
            {
                Beneficiaries.Add(beneficiary);
            }
            SelectedBeneficiary ??= Beneficiaries.FirstOrDefault();

            Mandates.Clear();
            foreach (var mandate in await _getMandates.ExecuteAsync(ct))
            {
                Mandates.Add(mandate);
            }

            Message = Beneficiaries.Count == 0
                ? "Aucun bénéficiaire disponible pour cet utilisateur."
                : "Virement SEPA ou instantané disponible.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger les paiements.";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanSubmitPayment));
        }
    }

    [RelayCommand(CanExecute = nameof(CanSubmitPayment))]
    private async Task SubmitPaymentAsync(CancellationToken ct)
    {
        if (!CanSubmitPayment || SelectedAccount is null || SelectedBeneficiary is null)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var order = new PaymentOrder(
                SelectedAccount.AccountId,
                SelectedBeneficiary.BeneficiaryId,
                Amount,
                SelectedAccount.Currency,
                PaymentLabel.NullIfEmpty() ?? "Virement EcoBank",
                SelectedPaymentMode?.Mode ?? PaymentExecutionMode.Standard);
            LastPayment = await _createSepaTransfer.ExecuteAsync(order, ct);
            Message = LastPayment.Status == PaymentStatus.Completed
                ? "Paiement exécuté."
                : "Paiement transmis à Xpollens.";
            OnPropertyChanged(nameof(LastPaymentLabel));
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de créer le paiement.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedAccountChanged(Account? value) => PaymentInputChanged();
    partial void OnSelectedBeneficiaryChanged(Beneficiary? value) => PaymentInputChanged();
    partial void OnAmountChanged(decimal value) => PaymentInputChanged();
    partial void OnSelectedPaymentModeChanged(PaymentModeOption? value) => PaymentInputChanged();

    private void PaymentInputChanged()
    {
        OnPropertyChanged(nameof(CanSubmitPayment));
        OnPropertyChanged(nameof(PaymentValidationMessage));
        SubmitPaymentCommand.NotifyCanExecuteChanged();
    }

    private static string FormatStatus(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "En cours",
        PaymentStatus.RequiresAuthentication => "Authentification requise",
        PaymentStatus.Completed => "Terminé",
        PaymentStatus.Rejected => "Rejeté",
        PaymentStatus.Failed => "Échec",
        _ => "Statut inconnu"
    };
}

public sealed class PaymentModeOption
{
    public string Label { get; init; } = "";
    public PaymentExecutionMode Mode { get; init; }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
