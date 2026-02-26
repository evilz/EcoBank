using EcoBank.Core.Domain.Cards;

namespace EcoBank.Core.Ports;

public interface ICardRepository
{
    Task<IReadOnlyList<Card>> GetCardsAsync(string appUserId, CancellationToken ct = default);
    Task<Card?> GetCardAsync(string cardId, CancellationToken ct = default);
    Task LockCardAsync(string cardId, CancellationToken ct = default);
    Task UnlockCardAsync(string cardId, CancellationToken ct = default);
}
