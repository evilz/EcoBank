using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Documents;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Documents;

internal sealed record KycDemandDto([property: JsonPropertyName("diligences")] List<KycDiligenceDto>? Diligences);

internal sealed record KycDiligenceDto(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("files")] List<KycAttachmentDto>? Files,
    [property: JsonPropertyName("creationDate")] DateTimeOffset? CreationDate);

internal sealed record KycAttachmentDto(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string Name);

public sealed class XpollensDocumentRepository(HttpClient httpClient, ILogger<XpollensDocumentRepository> logger) : IDocumentRepository
{
    public async Task<IReadOnlyList<UserDocument>> GetDocumentsAsync(string appUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching existing KYC documents for {AppUserId}", appUserId);
        using var response = await httpClient.GetAsync($"api/v3.0/users/{Uri.EscapeDataString(appUserId)}/kyc/demand", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var demand = await response.Content.ReadFromJsonAsync<KycDemandDto>(cancellationToken: ct);
        return (demand?.Diligences ?? [])
            .SelectMany(d => d.Files?.Select(f => new UserDocument(
                f.Key,
                string.IsNullOrWhiteSpace(f.Name) ? d.Type ?? "Document" : f.Name,
                DocumentKind.Kyc,
                GuessContentType(f.Name),
                d.CreationDate)) ?? [])
            .ToList()
            .AsReadOnly();
    }

    public async Task<UserDocumentContent?> GetDocumentContentAsync(string appUserId, string key, DocumentKind kind, CancellationToken ct = default)
    {
        var segment = kind == DocumentKind.Fatca ? "fatca" : "kyc";
        using var response = await httpClient.GetAsync(
            $"api/v3.0/users/{Uri.EscapeDataString(appUserId)}/{segment}/attachments/{Uri.EscapeDataString(key)}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var contentType = response.Content.Headers.ContentType?.MediaType;
        return new UserDocumentContent(key, key, kind, contentType, bytes);
    }

    private static string? GuessContentType(string? name)
    {
        var ext = Path.GetExtension(name)?.ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            _ => null
        };
    }
}
