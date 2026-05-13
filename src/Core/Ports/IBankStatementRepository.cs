using EcoBank.Core.Domain.Accounts;

namespace EcoBank.Core.Ports;

public interface IBankStatementRepository
{
    Task<IReadOnlyList<BankStatement>> ListBankStatementsAsync(string? accountId, CancellationToken ct = default);
    Task<BankStatement?> GetBankStatementAsync(string bankStatementId, CancellationToken ct = default);
}
