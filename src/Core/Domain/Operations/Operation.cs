namespace EcoBank.Core.Domain.Operations;

public record Operation(
    string OperationId,
    string AccountId,
    decimal Amount,
    string Currency,
    string? Label,
    OperationType Type,
    OperationStatus Status,
    DateTimeOffset Date,
    string? Category,
    string? Note);

public enum OperationType { Credit, Debit, Unknown }
public enum OperationStatus { Pending, Completed, Cancelled, Failed, Unknown }
