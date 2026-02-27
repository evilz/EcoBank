using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Accounts;

public class GetAccountsUseCase(IAccountRepository accountRepository)
{
    public Task<IReadOnlyList<Account>> ExecuteAsync(CancellationToken ct = default)
        => accountRepository.GetAccountsAsync(ct);
}
