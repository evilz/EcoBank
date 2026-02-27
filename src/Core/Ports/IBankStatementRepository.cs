using EcoBank.Core.Domain.Accounts;

namespace EcoBank.Core.Ports;

public interface IBankStatementRepository
{
    Task<BankStatement?> GetBankStatementAsync(string bankStatementId, CancellationToken ct = default);
}
