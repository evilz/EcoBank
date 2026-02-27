using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Accounts;

public class GetAccountsUseCase(IAccountRepository accountRepository, UserContext userContext)
{
    public Task<IReadOnlyList<Account>> ExecuteAsync(CancellationToken ct = default)
    {
        var appUserId = userContext.SelectedUser?.AppUserId;
        if (string.IsNullOrEmpty(appUserId))
            return Task.FromResult<IReadOnlyList<Account>>([]);
        return accountRepository.GetAccountsAsync(appUserId, ct);
    }
}
