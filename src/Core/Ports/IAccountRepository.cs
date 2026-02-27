using EcoBank.Core.Domain.Accounts;

namespace EcoBank.Core.Ports;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetAccountsAsync(string appUserId, CancellationToken ct = default);
    Task<Account?> GetAccountAsync(string accountId, CancellationToken ct = default);
}
