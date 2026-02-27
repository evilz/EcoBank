using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Accounts;

public class GetBankStatementUseCase(IBankStatementRepository repository)
{
    public Task<BankStatement?> ExecuteAsync(string bankStatementId, CancellationToken ct = default)
        => repository.GetBankStatementAsync(bankStatementId, ct);
}
