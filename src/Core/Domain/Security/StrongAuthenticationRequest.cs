namespace EcoBank.Core.Domain.Security;

public record StrongAuthenticationRequest(
    string AppUserId,
    string Action,
    string? ResourceId,
    decimal? Amount,
    string? Currency);

public record StrongAuthenticationResult(
    string RequestId,
    StrongAuthenticationStatus Status,
    string? Message);

public enum StrongAuthenticationStatus
{
    Pending,
    Approved,
    Rejected,
    Expired,
    Unknown
}
