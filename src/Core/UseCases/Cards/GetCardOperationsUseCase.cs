using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Cards;

public class GetCardOperationsUseCase(ICardOperationRepository repository)
{
    public Task<IReadOnlyList<CardOperation>> ExecuteAsync(
        string? cardId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
        => repository.GetCardOperationsAsync(cardId, page, pageSize, ct);
}
