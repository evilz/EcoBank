using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Cards;

/// <summary>
/// TODO: Map to Xpollens card endpoints — see https://docs.xpollens.com/reference/overview
/// Current assumption: GET /v1/users/{appUserId}/cards, POST /v1/cards/{cardId}/lock, POST /v1/cards/{cardId}/unlock
/// </summary>
internal sealed record CardDto(
    [property: JsonPropertyName("cardId")] string CardId,
    [property: JsonPropertyName("maskedPan")] string? MaskedPan,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("holderName")] string? HolderName,
    [property: JsonPropertyName("dailyLimit")] decimal? DailyLimit,
    [property: JsonPropertyName("monthlyLimit")] decimal? MonthlyLimit,
    [property: JsonPropertyName("currency")] string Currency);

public sealed class XpollensCardRepository(HttpClient httpClient, ILogger<XpollensCardRepository> logger) : ICardRepository
{
    public async Task<IReadOnlyList<Card>> GetCardsAsync(string appUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching cards for user {AppUserId}", appUserId);
        var dtos = await httpClient.GetFromJsonAsync<List<CardDto>>($"v1/users/{appUserId}/cards", ct) ?? [];
        return dtos.Select(Map).ToList().AsReadOnly();
    }

    public async Task<Card?> GetCardAsync(string cardId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching card {CardId}", cardId);
        var dto = await httpClient.GetFromJsonAsync<CardDto>($"v1/cards/{cardId}", ct);
        return dto is null ? null : Map(dto);
    }

    public async Task LockCardAsync(string cardId, CancellationToken ct = default)
    {
        logger.LogInformation("Locking card {CardId}", cardId);
        var response = await httpClient.PostAsync($"v1/cards/{cardId}/lock", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnlockCardAsync(string cardId, CancellationToken ct = default)
    {
        logger.LogInformation("Unlocking card {CardId}", cardId);
        var response = await httpClient.PostAsync($"v1/cards/{cardId}/unlock", null, ct);
        response.EnsureSuccessStatusCode();
    }

    private static Card Map(CardDto dto) => new(
        dto.CardId,
        dto.MaskedPan,
        ParseType(dto.Type),
        ParseStatus(dto.Status),
        dto.HolderName,
        dto.DailyLimit,
        dto.MonthlyLimit,
        dto.Currency);

    private static CardType ParseType(string? t) => t?.ToLowerInvariant() switch
    {
        "physical" => CardType.Physical,
        "virtual" => CardType.Virtual,
        _ => CardType.Unknown
    };

    private static CardStatus ParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "active" => CardStatus.Active,
        "blocked" => CardStatus.Blocked,
        "cancelled" => CardStatus.Cancelled,
        _ => CardStatus.Unknown
    };
}
