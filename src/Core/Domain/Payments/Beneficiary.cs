namespace EcoBank.Core.Domain.Payments;

public record Beneficiary(
    string BeneficiaryId,
    string DisplayName,
    string? Iban,
    string? Bic,
    BeneficiaryStatus Status);

public enum BeneficiaryStatus
{
    Active,
    Pending,
    Blocked,
    Unknown
}
