using EcoBank.Core.Domain.Payments;

namespace EcoBank.Core.Ports;

public interface IPaymentRepository
{
    Task<PaymentResult> CreateSepaTransferAsync(PaymentOrder order, CancellationToken ct = default);
    Task<PaymentResult> CreateInstantPaymentAsync(PaymentOrder order, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentResult>> GetPaymentStatusAsync(string appUserId, CancellationToken ct = default);
    Task<IReadOnlyList<Mandate>> GetMandatesAsync(string appUserId, CancellationToken ct = default);
}
