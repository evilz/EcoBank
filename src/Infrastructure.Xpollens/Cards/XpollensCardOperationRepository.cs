using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Cards;

/// <summary>
/// Xpollens card operations endpoint.
/// GET api/v2.0/card-operations — list card operations by criteria
/// </summary>
internal sealed record CardOperationDto(
    [property: JsonPropertyName("cardOperationId")] string CardOperationId,
    [property: JsonPropertyName("cardId")] string? CardId,
    [property: JsonPropertyName("maskedPan")] string? MaskedPan,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("merchantName")] string? MerchantName,
    [property: JsonPropertyName("merchantCategory")] string? MerchantCategory,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("date")] DateTimeOffset Date);

public sealed class XpollensCardOperationRepository(HttpClient httpClient, ILogger<XpollensCardOperationRepository> logger) : ICardOperationRepository
{
    public async Task<IReadOnlyList<CardOperation>> GetCardOperationsAsync(
        string? cardId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = $"api/v2.0/card-operations?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(cardId))
            query += $"&cardId={Uri.EscapeDataString(cardId)}";

        logger.LogDebug("Fetching card operations: cardId={CardId}, page={Page}", cardId, page);
        var dtos = await httpClient.GetFromJsonAsync<List<CardOperationDto>>(query, ct) ?? [];
        return dtos.Select(Map).ToList().AsReadOnly();
    }

    private static CardOperation Map(CardOperationDto dto) => new(
        dto.CardOperationId,
        dto.CardId,
        dto.MaskedPan,
        dto.Amount,
        dto.Currency,
        dto.Label,
        dto.MerchantName,
        dto.MerchantCategory,
        ParseStatus(dto.Status),
        dto.Date);

    private static CardOperationStatus ParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "pending" => CardOperationStatus.Pending,
        "completed" or "processed" => CardOperationStatus.Completed,
        "cancelled" => CardOperationStatus.Cancelled,
        "refused" or "rejected" => CardOperationStatus.Refused,
        _ => CardOperationStatus.Unknown
    };
}
