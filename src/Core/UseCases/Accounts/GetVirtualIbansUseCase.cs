using EcoBank.Core.Domain.Accounts;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Accounts;

public sealed class GetVirtualIbansUseCase(IVirtualIbanRepository virtualIbanRepository)
{
    public Task<IReadOnlyList<VirtualIban>> ExecuteAsync(string? accountId, CancellationToken ct = default)
    {
        return string.IsNullOrWhiteSpace(accountId)
            ? Task.FromResult<IReadOnlyList<VirtualIban>>([])
            : virtualIbanRepository.GetVirtualIbansAsync(accountId, ct);
    }
}
