using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Accounts;

public class ListBankStatementsUseCase(IBankStatementRepository repository, UserContext userContext)
{
    public Task<IReadOnlyList<BankStatement>> ExecuteAsync(string? accountId = null, CancellationToken ct = default)
    {
        if (userContext.SelectedUser is null)
            return Task.FromResult<IReadOnlyList<BankStatement>>([]);
        return repository.ListBankStatementsAsync(accountId, ct);
    }
}
