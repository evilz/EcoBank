namespace EcoBank.Core.Domain.Users;

public record User(
    string AppUserId,
    string? FirstName,
    string? LastName,
    string? Email,
    KycStatus KycStatus,
    DateTimeOffset? LastLogin);

public enum KycStatus
{
    Unknown,
    Pending,
    InProgress,
    Validated,
    Refused
}
