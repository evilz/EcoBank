namespace EcoBank.Core.Domain.Payments;

public record Mandate(
    string MandateId,
    string CreditorName,
    string? Reference,
    MandateStatus Status);

public enum MandateStatus
{
    Pending,
    Active,
    Revoked,
    Rejected,
    Unknown
}
