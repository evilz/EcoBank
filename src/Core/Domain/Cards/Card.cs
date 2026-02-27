namespace EcoBank.Core.Domain.Cards;

public record Card(
    string CardId,
    string? MaskedPan,
    CardType Type,
    CardStatus Status,
    string? HolderName,
    decimal? DailyLimit,
    decimal? MonthlyLimit,
    string Currency);

public enum CardType { Physical, Virtual, Unknown }
public enum CardStatus { Active, Blocked, Cancelled, Unknown }
