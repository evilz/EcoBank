using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Operations;

/// <summary>
/// Xpollens operations endpoints.
/// GET api/v2.0/accounts/{accountId}/operations — list operations for an account (paged envelope)
/// </summary>
internal sealed record XpollensOperationDto(
    [property: JsonPropertyName("operationId")] string OperationId,
    [property: JsonPropertyName("accountId")] string AccountId,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("date")] DateTimeOffset Date,
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("note")] string? Note);

internal sealed record OperationPagedResponseDto(
    [property: JsonPropertyName("values")] List<XpollensOperationDto>? Values);

public sealed class XpollensOperationRepository(HttpClient httpClient, ILogger<XpollensOperationRepository> logger) : IOperationRepository
{
    public async Task<IReadOnlyList<Operation>> GetOperationsAsync(
        string? accountId = null,
        OperationType? type = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        // accountId is required by the Xpollens API; return empty list if not provided.
        if (string.IsNullOrEmpty(accountId))
        {
            logger.LogWarning("GetOperationsAsync called without an accountId; returning empty list");
            return [];
        }

        logger.LogDebug("Fetching operations: accountId={AccountId}", accountId);
        var paged = await httpClient.GetFromJsonAsync<OperationPagedResponseDto>(
            $"api/v2.0/accounts/{Uri.EscapeDataString(accountId)}/operations?limit={pageSize}", ct);
        return (paged?.Values ?? []).Select(Map).ToList().AsReadOnly();
    }

    public async Task<Operation?> GetOperationAsync(string operationId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching operation {OperationId}", operationId);
        var dto = await httpClient.GetFromJsonAsync<XpollensOperationDto>($"api/v2.0/operations/{operationId}", ct);
        return dto is null ? null : Map(dto);
    }

    private static Operation Map(XpollensOperationDto dto) => new(
        dto.OperationId,
        dto.AccountId,
        dto.Amount,
        dto.Currency,
        dto.Label,
        ParseType(dto.Type),
        ParseStatus(dto.Status),
        dto.Date,
        dto.Category,
        dto.Note);

    private static OperationType ParseType(string? t) => t?.ToLowerInvariant() switch
    {
        "credit" => OperationType.Credit,
        "debit" => OperationType.Debit,
        _ => OperationType.Unknown
    };

    private static OperationStatus ParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "pending" => OperationStatus.Pending,
        "completed" or "processed" => OperationStatus.Completed,
        "cancelled" => OperationStatus.Cancelled,
        "failed" => OperationStatus.Failed,
        _ => OperationStatus.Unknown
    };
}
