using EcoBank.Core.Application;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Cards;

public class GetCardsUseCase(ICardRepository cardRepository, UserContext userContext)
{
    public Task<IReadOnlyList<Card>> ExecuteAsync(CancellationToken ct = default)
    {
        var userId = userContext.SelectedUser?.AppUserId
            ?? throw new InvalidOperationException("Aucun utilisateur sélectionné.");
        return cardRepository.GetCardsAsync(userId, ct);
    }
}
