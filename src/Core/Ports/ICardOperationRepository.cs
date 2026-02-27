using EcoBank.Core.Domain.Cards;

namespace EcoBank.Core.Ports;

public interface ICardOperationRepository
{
    Task<IReadOnlyList<CardOperation>> GetCardOperationsAsync(
        string? cardId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);
}
