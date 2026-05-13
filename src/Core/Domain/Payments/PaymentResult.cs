namespace EcoBank.Core.Domain.Payments;

public record PaymentResult(
    string PaymentId,
    PaymentStatus Status,
    string? Message);

public enum PaymentStatus
{
    Draft,
    Pending,
    RequiresAuthentication,
    Completed,
    Rejected,
    Failed,
    Unknown
}
