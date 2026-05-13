using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Payments;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Payments;

internal sealed record PaymentAmountDto(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("currency")] string? Currency);

internal sealed record PaymentCreditorDto(
    [property: JsonPropertyName("beneficiaryId")] string BeneficiaryId);

internal sealed record SepaTransferRequestDto(
    [property: JsonPropertyName("sepaCreditTransferId")] string SepaCreditTransferId,
    [property: JsonPropertyName("accountId")] string AccountId,
    [property: JsonPropertyName("creditor")] PaymentCreditorDto Creditor,
    [property: JsonPropertyName("amount")] PaymentAmountDto Amount,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("reference")] string? Reference);

internal sealed record InstantPaymentRequestDto(
    [property: JsonPropertyName("sepaInstantPaymentId")] string SepaInstantPaymentId,
    [property: JsonPropertyName("accountId")] string AccountId,
    [property: JsonPropertyName("creditor")] PaymentCreditorDto Creditor,
    [property: JsonPropertyName("amount")] PaymentAmountDto Amount,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("reference")] string? Reference);

internal sealed record PaymentResponseDto(
    [property: JsonPropertyName("sepaCreditTransferId")] string? SepaCreditTransferId,
    [property: JsonPropertyName("sepaInstantPaymentId")] string? SepaInstantPaymentId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("description")] string? Description);

internal sealed record MandatePagedDto([property: JsonPropertyName("values")] List<MandateDto>? Values);

internal sealed record MandateDto(
    [property: JsonPropertyName("mandateId")] string? MandateId,
    [property: JsonPropertyName("creditorName")] string? CreditorName,
    [property: JsonPropertyName("reference")] string? Reference,
    [property: JsonPropertyName("status")] string? Status);

public sealed class XpollensPaymentRepository(HttpClient httpClient, ILogger<XpollensPaymentRepository> logger) : IPaymentRepository
{
    public async Task<PaymentResult> CreateSepaTransferAsync(PaymentOrder order, CancellationToken ct = default)
    {
        logger.LogInformation("Creating SEPA transfer from account {AccountId}", order.SourceAccountId);
        var request = new SepaTransferRequestDto(
            $"eco-{Guid.NewGuid():N}",
            order.SourceAccountId,
            new PaymentCreditorDto(order.BeneficiaryId),
            ToAmount(order),
            order.Label,
            order.Label);
        var response = await httpClient.PostAsJsonAsync("api/v2.0/sepa-credit-transfers", request, ct);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PaymentResponseDto>(cancellationToken: ct);
        return Map(dto, request.SepaCreditTransferId);
    }

    public async Task<PaymentResult> CreateInstantPaymentAsync(PaymentOrder order, CancellationToken ct = default)
    {
        logger.LogInformation("Creating instant payment from account {AccountId}", order.SourceAccountId);
        var request = new InstantPaymentRequestDto(
            $"eco-{Guid.NewGuid():N}",
            order.SourceAccountId,
            new PaymentCreditorDto(order.BeneficiaryId),
            ToAmount(order),
            order.Label,
            order.Label);
        var response = await httpClient.PostAsJsonAsync("api/v3.0/sepa-instant-payments", request, ct);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PaymentResponseDto>(cancellationToken: ct);
        return Map(dto, request.SepaInstantPaymentId);
    }

    public Task<IReadOnlyList<PaymentResult>> GetPaymentStatusAsync(string appUserId, CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<PaymentResult>>([]);
    }

    public async Task<IReadOnlyList<Mandate>> GetMandatesAsync(string appUserId, CancellationToken ct = default)
    {
        var response = await httpClient.GetFromJsonAsync<MandatePagedDto>(
            $"api/v2.0/mandates?appUserId={Uri.EscapeDataString(appUserId)}", ct);
        return (response?.Values ?? []).Select(MapMandate).ToList().AsReadOnly();
    }

    private static PaymentAmountDto ToAmount(PaymentOrder order) => new(
        order.Amount.ToString("0.00", CultureInfo.InvariantCulture),
        order.Currency);

    private static PaymentResult Map(PaymentResponseDto? dto, string fallbackId) => new(
        dto?.SepaCreditTransferId ?? dto?.SepaInstantPaymentId ?? fallbackId,
        ParseStatus(dto?.Status),
        dto?.Description);

    private static Mandate MapMandate(MandateDto dto) => new(
        dto.MandateId ?? "",
        dto.CreditorName ?? "Créancier",
        dto.Reference,
        dto.Status?.ToLowerInvariant() switch
        {
            "active" => MandateStatus.Active,
            "pending" => MandateStatus.Pending,
            "revoked" => MandateStatus.Revoked,
            "rejected" => MandateStatus.Rejected,
            _ => MandateStatus.Unknown
        });

    private static PaymentStatus ParseStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "created" or "approved" => PaymentStatus.Pending,
        "completed" => PaymentStatus.Completed,
        "rejected" => PaymentStatus.Rejected,
        "canceled" or "cancelled" => PaymentStatus.Failed,
        _ => PaymentStatus.Unknown
    };
}
