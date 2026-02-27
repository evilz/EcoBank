using EcoBank.Core.Domain.Operations;

namespace EcoBank.Core.Ports;

public interface IOperationRepository
{
    Task<IReadOnlyList<Operation>> GetOperationsAsync(
        string? accountId = null,
        OperationType? type = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);
    Task<Operation?> GetOperationAsync(string operationId, CancellationToken ct = default);
}
