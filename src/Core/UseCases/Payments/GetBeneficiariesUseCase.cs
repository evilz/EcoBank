using EcoBank.Core.Application;
using EcoBank.Core.Domain.Payments;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Payments;

public sealed class GetBeneficiariesUseCase(UserContext userContext, IBeneficiaryRepository beneficiaryRepository)
{
    public Task<IReadOnlyList<Beneficiary>> ExecuteAsync(CancellationToken ct = default)
    {
        var appUserId = userContext.SelectedUser?.AppUserId;
        return string.IsNullOrWhiteSpace(appUserId)
            ? Task.FromResult<IReadOnlyList<Beneficiary>>([])
            : beneficiaryRepository.GetBeneficiariesAsync(appUserId, ct);
    }
}
