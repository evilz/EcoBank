namespace EcoBank.Core.Ports;

public interface ISecureStorage
{
    Task SaveAsync(string key, string value, CancellationToken ct = default);
    Task<string?> LoadAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
