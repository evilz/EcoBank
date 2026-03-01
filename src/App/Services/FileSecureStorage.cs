using System.Security.Cryptography;
using System.Text;
using EcoBank.Core.Ports;

namespace EcoBank.App.Services;

/// <summary>
/// File-based secure storage that persists data to disk in the application's AppData directory.
/// Data is encrypted at rest using AES-GCM with a per-installation key stored in the app data folder.
/// </summary>
public sealed class FileSecureStorage : ISecureStorage
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;

    private readonly string _storagePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private byte[]? _encryptionKey;

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

    private byte[] GetOrCreateEncryptionKey()
    {
        if (_encryptionKey is not null) return _encryptionKey;

        var keyFile = Path.Combine(_storagePath, "_key.bin");

        if (File.Exists(keyFile))
        {
            var stored = File.ReadAllBytes(keyFile);
            if (stored.Length == KeySize)
            {
                _encryptionKey = stored;
                return _encryptionKey;
            }
        }

        _encryptionKey = new byte[KeySize];
        RandomNumberGenerator.Fill(_encryptionKey);
        File.WriteAllBytes(keyFile, _encryptionKey);
        return _encryptionKey;
    }

    private byte[] Encrypt(string plaintext)
    {
        var key = GetOrCreateEncryptionKey();
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Layout: nonce (12) | tag (16) | ciphertext
        var result = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);
        return result;
    }

    private string Decrypt(byte[] data)
    {
        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Invalid encrypted data.");

        var key = GetOrCreateEncryptionKey();
        var nonce = data[..NonceSize];
        var tag = data[NonceSize..(NonceSize + TagSize)];
        var ciphertext = data[(NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }

    public async Task SaveAsync(string key, string value, CancellationToken ct = default)
    {
        var filePath = GetFilePath(key);
        var encrypted = Encrypt(value);

        await _semaphore.WaitAsync(ct);
        try
        {
            await File.WriteAllBytesAsync(filePath, encrypted, ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<string?> LoadAsync(string key, CancellationToken ct = default)
    {
        var filePath = GetFilePath(key);

        await _semaphore.WaitAsync(ct);
        try
        {
            if (!File.Exists(filePath)) return null;

            try
            {
                var encrypted = await File.ReadAllBytesAsync(filePath, ct);
                return Decrypt(encrypted);
            }
            catch (Exception)
            {
                return null;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var filePath = GetFilePath(key);

        await _semaphore.WaitAsync(ct);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private string GetFilePath(string key)
    {
        var safeKey = string.Concat(key.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));

        if (string.IsNullOrEmpty(safeKey))
            throw new ArgumentException($"Key '{key}' produces an empty filename after sanitization.", nameof(key));

        return Path.Combine(_storagePath, $"{safeKey}.dat");
    }
}

