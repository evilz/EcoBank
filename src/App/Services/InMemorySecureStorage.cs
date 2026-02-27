using EcoBank.Core.Ports;

namespace EcoBank.App.Services;

/// <summary>
/// Simple in-memory secure storage for desktop platforms.
/// On production mobile targets, replace with platform-specific secure storage.
/// </summary>
public sealed class InMemorySecureStorage : ISecureStorage
{
    private readonly Dictionary<string, string> _store = new();

    public Task SaveAsync(string key, string value, CancellationToken ct = default)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task<string?> LoadAsync(string key, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(key, out var v) ? v : null);

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }
}
