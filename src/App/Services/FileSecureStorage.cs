using System.Text;
using EcoBank.Core.Ports;

namespace EcoBank.App.Services;

/// <summary>
/// File-based secure storage that persists data to disk in the application's AppData directory.
/// Provides basic encryption via Base64 encoding (use AES or similar for production).
/// </summary>
public sealed class FileSecureStorage : ISecureStorage
{
    private readonly string _storagePath;
    private readonly object _lockObject = new();

    public FileSecureStorage()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = Path.Combine(appDataPath, "EcoBank");
        
        _storagePath = Path.Combine(appFolder, "secure_storage");
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public Task SaveAsync(string key, string value, CancellationToken ct = default)
    {
        lock (_lockObject)
        {
            var filePath = GetFilePath(key);
            
            // Simple Base64 encoding for demo. Use AES encryption in production.
            var encodedValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            
            File.WriteAllText(filePath, encodedValue, Encoding.UTF8);
        }

        return Task.CompletedTask;
    }

    public Task<string?> LoadAsync(string key, CancellationToken ct = default)
    {
        lock (_lockObject)
        {
            var filePath = GetFilePath(key);

            if (!File.Exists(filePath))
            {
                return Task.FromResult<string?>(null);
            }

            try
            {
                var encodedValue = File.ReadAllText(filePath, Encoding.UTF8);
                var decodedValue = Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue));
                return Task.FromResult<string?>(decodedValue);
            }
            catch (Exception)
            {
                return Task.FromResult<string?>(null);
            }
        }
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        lock (_lockObject)
        {
            var filePath = GetFilePath(key);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        return Task.CompletedTask;
    }

    private string GetFilePath(string key)
    {
        // Sanitize key to create safe filename
        var safeKey = string.Concat(key.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));
        return Path.Combine(_storagePath, $"{safeKey}.dat");
    }
}

