using EcoBank.Core.Domain.Payments;

namespace EcoBank.Core.Ports;

public interface IBeneficiaryRepository
{
    Task<IReadOnlyList<Beneficiary>> GetBeneficiariesAsync(string appUserId, CancellationToken ct = default);
    Task<Beneficiary?> GetBeneficiaryAsync(string appUserId, string beneficiaryId, CancellationToken ct = default);
    Task<Beneficiary> SaveBeneficiaryAsync(string appUserId, Beneficiary beneficiary, CancellationToken ct = default);
    Task DeleteBeneficiaryAsync(string appUserId, string beneficiaryId, CancellationToken ct = default);
}
