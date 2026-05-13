namespace EcoBank.Core.Domain.Accounts;

public record VirtualIban(
    string VirtualIbanId,
    string AccountId,
    string Iban,
    string? Label,
    AccountStatus Status);
