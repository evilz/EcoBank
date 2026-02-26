using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Cards;

public class ToggleCardLockUseCase(ICardRepository cardRepository)
{
    public async Task ExecuteAsync(string cardId, bool lock_, CancellationToken ct = default)
    {
        if (lock_)
            await cardRepository.LockCardAsync(cardId, ct);
        else
            await cardRepository.UnlockCardAsync(cardId, ct);
    }
}
