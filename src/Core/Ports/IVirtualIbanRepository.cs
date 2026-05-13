using EcoBank.Core.Domain.Accounts;

namespace EcoBank.Core.Ports;

public interface IVirtualIbanRepository
{
    Task<IReadOnlyList<VirtualIban>> GetVirtualIbansAsync(string accountId, CancellationToken ct = default);
    Task<VirtualIban> CreateVirtualIbanAsync(string accountId, string? label, CancellationToken ct = default);
}
