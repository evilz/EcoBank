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
internal sealed record OperationPartyDto(
    [property: JsonPropertyName("accountId")] string? AccountId,
    [property: JsonPropertyName("fullname")] string? Fullname,
    [property: JsonPropertyName("iban")] string? Iban,
    [property: JsonPropertyName("bic")] string? Bic);

internal sealed record OperationAmountDto(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("currency")] string Currency);

internal sealed record XpollensOperationDto(
    [property: JsonPropertyName("operationId")] string OperationId,
    [property: JsonPropertyName("operationType")] string? OperationType,
    [property: JsonPropertyName("direction")] string? Direction,
    [property: JsonPropertyName("isCorrective")] bool? IsCorrective,
    [property: JsonPropertyName("operationStatus")] string? OperationStatus,
    [property: JsonPropertyName("creationDate")] DateTimeOffset CreationDate,
    [property: JsonPropertyName("settlementDate")] DateTimeOffset? SettlementDate,
    [property: JsonPropertyName("expectedExecutionDate")] DateTimeOffset? ExpectedExecutionDate,
    [property: JsonPropertyName("sender")] OperationPartyDto? Sender,
    [property: JsonPropertyName("receiver")] OperationPartyDto? Receiver,
    [property: JsonPropertyName("amount")] OperationAmountDto Amount);

internal sealed record OperationPagedResponseDto(
    [property: JsonPropertyName("values")] List<XpollensOperationDto>? Values,
    [property: JsonPropertyName("continuationToken")] string? ContinuationToken);

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
        return (paged?.Values ?? []).Select(dto => Map(dto, logger)).ToList().AsReadOnly();
    }

    public async Task<Operation?> GetOperationAsync(string operationId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching operation {OperationId}", operationId);
        var dto = await httpClient.GetFromJsonAsync<XpollensOperationDto>($"api/v2.0/operations/{operationId}", ct);
        return dto is null ? null : Map(dto, logger);
    }

    private static Operation Map(XpollensOperationDto dto, ILogger logger)
    {
        var direction = ParseType(dto.Direction);
        // Label: show the counterparty name; for credit show sender, for debit show receiver.
        var label = direction == OperationType.Credit
            ? dto.Sender?.Fullname ?? dto.OperationType
            : dto.Receiver?.Fullname ?? dto.OperationType;
        // The "own" account is the receiver for credits and the sender for debits.
        var accountId = (direction == OperationType.Credit ? dto.Receiver?.AccountId : dto.Sender?.AccountId)
            ?? string.Empty;
        if (!decimal.TryParse(dto.Amount.Value, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            logger.LogWarning("Could not parse amount '{Value}' for operationId={OperationId}; defaulting to 0",
                dto.Amount.Value, dto.OperationId);
            amount = 0m;
        }
        return new Operation(
            dto.OperationId,
            accountId,
            amount,
            dto.Amount.Currency,
            label,
            direction,
            ParseStatus(dto.OperationStatus),
            dto.CreationDate,
            dto.OperationType,
            null);
    }

    private static OperationType ParseType(string? t) => t?.ToLowerInvariant() switch
    {
        "credit" => OperationType.Credit,
        "debit" => OperationType.Debit,
        _ => OperationType.Unknown
    };

    private static OperationStatus ParseStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "pending" => OperationStatus.Pending,
        "approved" => OperationStatus.Approved,
        "completed" or "processed" => OperationStatus.Completed,
        "cancelled" or "canceled" => OperationStatus.Cancelled,
        "rejected" => OperationStatus.Rejected,
        "failed" => OperationStatus.Failed,
        _ => OperationStatus.Unknown
    };
}
