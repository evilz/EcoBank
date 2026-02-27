using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Cards;

public class GetCardsUseCase(ICardRepository cardRepository)
{
    public Task<IReadOnlyList<Card>> ExecuteAsync(CancellationToken ct = default)
        => cardRepository.GetCardsAsync(ct);
}
