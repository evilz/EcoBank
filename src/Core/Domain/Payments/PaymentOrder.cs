namespace EcoBank.Core.Domain.Payments;

public record PaymentOrder(
    string SourceAccountId,
    string BeneficiaryId,
    decimal Amount,
    string Currency,
    string? Label,
    PaymentExecutionMode ExecutionMode);

public enum PaymentExecutionMode
{
    Standard,
    Instant
}
