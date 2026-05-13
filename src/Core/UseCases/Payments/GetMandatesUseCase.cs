using EcoBank.Core.Application;
using EcoBank.Core.Domain.Payments;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Payments;

public sealed class GetMandatesUseCase(UserContext userContext, IPaymentRepository paymentRepository)
{
    public Task<IReadOnlyList<Mandate>> ExecuteAsync(CancellationToken ct = default)
    {
        var appUserId = userContext.SelectedUser?.AppUserId;
        return string.IsNullOrWhiteSpace(appUserId)
            ? Task.FromResult<IReadOnlyList<Mandate>>([])
            : paymentRepository.GetMandatesAsync(appUserId, ct);
    }
}
