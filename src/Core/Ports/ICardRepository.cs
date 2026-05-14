using EcoBank.Core.Domain.Cards;

namespace EcoBank.Core.Ports;

public interface ICardRepository
{
    Task<IReadOnlyList<Card>> GetCardsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Card>> GetCardsByHolderAsync(string holderExternalRef, CancellationToken ct = default);
    Task<Card?> GetCardAsync(string cardId, CancellationToken ct = default);
    Task<Card> CreatePhysicalCardAsync(string holderExternalRef, CancellationToken ct = default);
    Task<Card> CreateVirtualCardAsync(string holderExternalRef, CancellationToken ct = default);
    Task LockCardAsync(string cardId, CancellationToken ct = default);
    Task UnlockCardAsync(string cardId, CancellationToken ct = default);
}
