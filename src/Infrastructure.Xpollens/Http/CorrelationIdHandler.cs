namespace EcoBank.Infrastructure.Xpollens.Http;

/// <summary>
/// Adds a unique X-Correlation-ID header to each outgoing request.
/// </summary>
public sealed class CorrelationIdHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", Guid.NewGuid().ToString());
        return base.SendAsync(request, ct);
    }
}
