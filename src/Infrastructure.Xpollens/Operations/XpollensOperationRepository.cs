using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Operations;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Operations;

/// <summary>
/// Xpollens transaction endpoints.
/// GET api/v3.0/accounts/{accountId}/transactions — list transactions for an account
/// </summary>
internal sealed record OperationDto(
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
        // Transactions require an accountId per the Xpollens API spec.
        // If no accountId is provided the call is made without account scoping,
        // which may not be supported — callers should always pass an accountId.
        var path = accountId is not null
            ? $"api/v3.0/accounts/{accountId}/transactions"
            : "api/v3.0/accounts/transactions";

        var query = $"{path}?page={page}&pageSize={pageSize}";
        if (type is not null) query += $"&type={type.ToString()!.ToLowerInvariant()}";
        if (from is not null) query += $"&from={from.Value:O}";
        if (to is not null) query += $"&to={to.Value:O}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";

        logger.LogDebug("Fetching operations: accountId={AccountId}, page={Page}", accountId, page);
        var dtos = await httpClient.GetFromJsonAsync<List<OperationDto>>(query, ct) ?? [];
        return dtos.Select(Map).ToList().AsReadOnly();
    }

    public async Task<Operation?> GetOperationAsync(string operationId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching operation {OperationId}", operationId);
        var dto = await httpClient.GetFromJsonAsync<OperationDto>($"api/v3.0/transactions/{operationId}", ct);
        return dto is null ? null : Map(dto);
    }

    private static Operation Map(OperationDto dto) => new(
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
