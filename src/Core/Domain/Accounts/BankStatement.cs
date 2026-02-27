namespace EcoBank.Core.Domain.Accounts;

public record BankStatement(
    string BankStatementId,
    string? AccountId,
    DateTimeOffset? PeriodStart,
    DateTimeOffset? PeriodEnd,
    string? Label,
    string? DocumentUrl);
