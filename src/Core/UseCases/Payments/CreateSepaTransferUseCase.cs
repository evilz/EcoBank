using EcoBank.Core.Domain.Payments;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Payments;

public sealed class CreateSepaTransferUseCase(IPaymentRepository paymentRepository)
{
    public Task<PaymentResult> ExecuteAsync(PaymentOrder order, CancellationToken ct = default)
    {
        Validate(order);
        return order.ExecutionMode == PaymentExecutionMode.Instant
            ? paymentRepository.CreateInstantPaymentAsync(order, ct)
            : paymentRepository.CreateSepaTransferAsync(order, ct);
    }

    private static void Validate(PaymentOrder order)
    {
        if (string.IsNullOrWhiteSpace(order.SourceAccountId))
        {
            throw new ArgumentException("Source account is required.", nameof(order));
        }

        if (string.IsNullOrWhiteSpace(order.BeneficiaryId))
        {
            throw new ArgumentException("Beneficiary is required.", nameof(order));
        }

        if (order.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(order));
        }

        if (string.IsNullOrWhiteSpace(order.Currency))
        {
            throw new ArgumentException("Currency is required.", nameof(order));
        }
    }
}
