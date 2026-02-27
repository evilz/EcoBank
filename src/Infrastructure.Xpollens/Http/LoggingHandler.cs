using Microsoft.Extensions.Logging;

namespace EcoBank.Infrastructure.Xpollens.Http;

/// <summary>
/// Logs HTTP requests and responses. Secrets are never logged.
/// </summary>
public sealed class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        logger.LogDebug("HTTP {Method} {Uri}", request.Method, request.RequestUri?.PathAndQuery);
        var response = await base.SendAsync(request, ct);
        logger.LogDebug("HTTP {Method} {Uri} -> {StatusCode}", request.Method, request.RequestUri?.PathAndQuery, (int)response.StatusCode);
        return response;
    }
}
