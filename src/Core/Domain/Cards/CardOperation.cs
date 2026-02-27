namespace EcoBank.Core.Domain.Cards;

public record CardOperation(
    string CardOperationId,
    string? CardId,
    string? MaskedPan,
    decimal Amount,
    string Currency,
    string? Label,
    string? MerchantName,
    string? MerchantCategory,
    CardOperationStatus Status,
    DateTimeOffset Date);

public enum CardOperationStatus { Pending, Completed, Cancelled, Refused, Unknown }
