using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Ports;
using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Auth;

internal sealed record TokenRequestBody(
    [property: JsonPropertyName("grant_type")] string GrantType,
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("client_secret")] string ClientSecret);

internal sealed record TokenResponseDto(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

/// <summary>
/// TODO: Map to Xpollens OAuth2 token endpoint — see https://docs.xpollens.com/reference/overview
/// Current assumption: POST /v1/oauth/token with client_credentials grant.
/// </summary>
public sealed class XpollensAuthService(HttpClient httpClient, ILogger<XpollensAuthService> logger) : IAuthService
{
    private const string TokenEndpoint = "v1/oauth/token"; // TODO: confirm exact path from Xpollens docs

    public async Task<TokenResponse> AuthenticateAsync(Credentials credentials, CancellationToken ct = default)
    {
        logger.LogInformation("Authenticating with Xpollens (client_id={ClientId})", credentials.ClientId);

        var body = new TokenRequestBody("client_credentials", credentials.ClientId, credentials.ClientSecret);
        using var response = await httpClient.PostAsJsonAsync(TokenEndpoint, body, ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Authentication failed: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Authentification échouée : {response.StatusCode}", null, response.StatusCode);
        }

        var dto = await response.Content.ReadFromJsonAsync<TokenResponseDto>(ct)
            ?? throw new InvalidOperationException("Réponse token vide.");

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(dto.ExpiresIn);
        logger.LogInformation("Authentication successful, token expires at {ExpiresAt}", expiresAt);

        return new TokenResponse(dto.AccessToken, dto.TokenType, dto.ExpiresIn, expiresAt);
    }
}
