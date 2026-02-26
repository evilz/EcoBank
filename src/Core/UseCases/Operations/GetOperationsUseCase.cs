using EcoBank.Core.Domain.Operations;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Operations;

public class GetOperationsUseCase(IOperationRepository operationRepository)
{
    public Task<IReadOnlyList<Operation>> ExecuteAsync(
        string? accountId = null,
        OperationType? type = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
        => operationRepository.GetOperationsAsync(accountId, type, from, to, search, page, pageSize, ct);
}
