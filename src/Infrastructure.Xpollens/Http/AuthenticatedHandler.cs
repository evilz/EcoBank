using EcoBank.Core.Application;

namespace EcoBank.Infrastructure.Xpollens.Http;

/// <summary>
/// Injects the Bearer token from UserContext into outgoing requests.
/// </summary>
public sealed class AuthenticatedHandler(UserContext userContext) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (userContext.Token is { } token)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
        return base.SendAsync(request, ct);
    }
}
