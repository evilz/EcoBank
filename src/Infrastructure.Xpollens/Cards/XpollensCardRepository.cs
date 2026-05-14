using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Cards;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Cards;

/// <summary>
/// Xpollens card endpoints.
/// GET  api/v3.0/cards                              — list cards
/// GET  api/v2.0/card/holder/{holderExternalRef} — list cards by holder
/// GET  api/v3.0/cards/{cardId}                  — get a single card
/// POST api/v3.0/cards/physical                  — create a physical card
/// POST api/v3.0/cards/virtual                   — create a virtual card
/// POST api/v3.0/cards/{cardId}/lock             — lock a card
/// POST api/v3.0/cards/{cardId}/unlock           — unlock a card
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

internal sealed record HolderCardDto(
    [property: JsonPropertyName("cardId")] string CardId,
    [property: JsonPropertyName("maskedPan")] string? MaskedPan,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("holderName")] string? HolderName,
    [property: JsonPropertyName("currency")] string? Currency);

internal sealed record CreatePhysicalCardRequest(
    [property: JsonPropertyName("holderExternalRef")] string HolderExternalRef,
    [property: JsonPropertyName("cardPrint")] string CardPrint = "EcoBank");

internal sealed record CreateVirtualCardRequest(
    [property: JsonPropertyName("holderExternalRef")] string HolderExternalRef,
    [property: JsonPropertyName("cardPrint")] string CardPrint = "EcoBank");

public sealed class XpollensCardRepository(HttpClient httpClient, ILogger<XpollensCardRepository> logger) : ICardRepository
{
    public async Task<IReadOnlyList<Card>> GetCardsAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Fetching cards");
        var dtos = await httpClient.GetFromJsonAsync<List<CardDto>>("api/v3.0/cards", ct) ?? [];
        return dtos.Select(Map).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Card>> GetCardsByHolderAsync(string holderExternalRef, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching cards for holder {HolderExternalRef}", holderExternalRef);
        try
        {
            var dtos = await httpClient.GetFromJsonAsync<List<HolderCardDto>>(
                $"api/v2.0/card/holder/{Uri.EscapeDataString(holderExternalRef)}", ct) ?? [];
            return dtos
                .Select(d => new Card(
                    d.CardId,
                    d.MaskedPan,
                    ParseType(d.Type),
                    ParseStatus(d.Status),
                    d.HolderName,
                    null,
                    null,
                    d.Currency ?? "EUR"))
                .ToList()
                .AsReadOnly();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return [];
        }
    }

    public async Task<Card?> GetCardAsync(string cardId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching card {CardId}", cardId);
        var dto = await httpClient.GetFromJsonAsync<CardDto>($"api/v3.0/cards/{cardId}", ct);
        return dto is null ? null : Map(dto);
    }

    public async Task<Card> CreatePhysicalCardAsync(string holderExternalRef, CancellationToken ct = default)
    {
        logger.LogInformation("Creating physical card for holder {HolderExternalRef}", holderExternalRef);
        var request = new CreatePhysicalCardRequest(holderExternalRef);
        var response = await httpClient.PostAsJsonAsync("api/v3.0/cards/physical", request, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CardDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Card creation returned no data.");
        return Map(created);
    }

    public async Task<Card> CreateVirtualCardAsync(string holderExternalRef, CancellationToken ct = default)
    {
        logger.LogInformation("Creating virtual card for holder {HolderExternalRef}", holderExternalRef);
        var request = new CreateVirtualCardRequest(holderExternalRef);
        var response = await httpClient.PostAsJsonAsync("api/v3.0/cards/virtual", request, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CardDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Virtual card creation returned no data.");
        return Map(created);
    }

    public async Task LockCardAsync(string cardId, CancellationToken ct = default)
    {
        logger.LogInformation("Locking card {CardId}", cardId);
        var response = await httpClient.PostAsync($"api/v3.0/cards/{cardId}/lock", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnlockCardAsync(string cardId, CancellationToken ct = default)
    {
        logger.LogInformation("Unlocking card {CardId}", cardId);
        var response = await httpClient.PostAsync($"api/v3.0/cards/{cardId}/unlock", null, ct);
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
