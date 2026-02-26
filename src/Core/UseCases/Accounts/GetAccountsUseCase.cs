using EcoBank.Core.Application;
using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Accounts;

public class GetAccountsUseCase(IAccountRepository accountRepository, UserContext userContext)
{
    public Task<IReadOnlyList<Account>> ExecuteAsync(CancellationToken ct = default)
    {
        var userId = userContext.SelectedUser?.AppUserId
            ?? throw new InvalidOperationException("Aucun utilisateur sélectionné.");
        return accountRepository.GetAccountsAsync(userId, ct);
    }
}
