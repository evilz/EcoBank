namespace EcoBank.Core.Domain.Accounts;

public record Account(
    string AccountId,
    string? Label,
    AccountType Type,
    decimal Balance,
    string Currency,
    string? Iban,
    AccountStatus Status);

public enum AccountType { Current, Savings, Unknown }
public enum AccountStatus { Active, Closed, Blocked, Unknown }
