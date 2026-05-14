using EcoBank.Core.Application;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Cards;

public class CreateVirtualCardUseCase(ICardRepository cardRepository, UserContext userContext)
{
    public Task<Card> ExecuteAsync(CancellationToken ct = default)
    {
        var holderRef = userContext.SelectedUser?.AppUserId
            ?? throw new InvalidOperationException("No user selected.");
        return cardRepository.CreateVirtualCardAsync(holderRef, ct);
    }
}
