using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Ports;

namespace EcoBank.App.Services;

public class ProfileService
{
    private const string ProfilesStorageKey = "saved_profiles";
    private readonly ISecureStorage _secureStorage;

    public ProfileService(ISecureStorage secureStorage)
    {
        _secureStorage = secureStorage;
    }

    public async Task<IReadOnlyList<SavedProfile>> GetProfilesAsync(CancellationToken ct = default)
    {
        var json = await _secureStorage.LoadAsync(ProfilesStorageKey, ct);
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<SavedProfile>();
        }

        try
        {
            var profiles = JsonSerializer.Deserialize<List<SavedProfile>>(json);
            return profiles ?? new List<SavedProfile>();
        }
        catch (JsonException)
        {
            return Array.Empty<SavedProfile>();
        }
    }

    public async Task SaveProfileAsync(SavedProfile profile, CancellationToken ct = default)
    {
        var profiles = (await GetProfilesAsync(ct)).ToList();
        
        // Remove existing profile if it exists (by Id or AppUserId)
        profiles.RemoveAll(p => p.Id == profile.Id || (p.AppUserId == profile.AppUserId && p.ClientId == profile.ClientId));
        
        profiles.Add(profile);

        var json = JsonSerializer.Serialize(profiles);
        await _secureStorage.SaveAsync(ProfilesStorageKey, json, ct);
    }

    public async Task DeleteProfileAsync(string id, CancellationToken ct = default)
    {
        var profiles = (await GetProfilesAsync(ct)).ToList();
        var removed = profiles.RemoveAll(p => p.Id == id);
        
        if (removed > 0)
        {
            var json = JsonSerializer.Serialize(profiles);
            await _secureStorage.SaveAsync(ProfilesStorageKey, json, ct);
        }
    }

    public string HashPin(string pin)
    {
        const int saltSize = 16;
        const int keySize = 32;
        const int iterations = 100_000;

        var salt = new byte[saltSize];
        RandomNumberGenerator.Fill(salt);

        var key = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterations, HashAlgorithmName.SHA256, keySize);

        // Layout: iterations (4) | salt (16) | key (32)
        var result = new byte[sizeof(int) + saltSize + keySize];
        Buffer.BlockCopy(BitConverter.GetBytes(iterations), 0, result, 0, sizeof(int));
        Buffer.BlockCopy(salt, 0, result, sizeof(int), saltSize);
        Buffer.BlockCopy(key, 0, result, sizeof(int) + saltSize, keySize);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPin(string pin, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return false;

        byte[] decoded;
        try { decoded = Convert.FromBase64String(hash); }
        catch (FormatException) { return false; }

        const int saltSize = 16;
        const int keySize = 32;
        if (decoded.Length < sizeof(int) + saltSize + keySize) return false;

        var iterations = BitConverter.ToInt32(decoded, 0);
        if (iterations <= 0) return false;

        var salt = new byte[saltSize];
        Buffer.BlockCopy(decoded, sizeof(int), salt, 0, saltSize);

        var storedKey = new byte[keySize];
        Buffer.BlockCopy(decoded, sizeof(int) + saltSize, storedKey, 0, keySize);

        var computedKey = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterations, HashAlgorithmName.SHA256, keySize);

        return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
    }

    public string EncryptSecret(string secret, string pin)
    {
        const int saltSize = 16;
        const int keySize = 32;
        const int iterations = 100_000;
        const int nonceSize = 12;
        const int tagSize = 16;

        var salt = new byte[saltSize];
        RandomNumberGenerator.Fill(salt);

        var key = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterations, HashAlgorithmName.SHA256, keySize);

        var nonce = new byte[nonceSize];
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(secret);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[tagSize];

        using var aesGcm = new AesGcm(key, tagSize);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Layout: salt (16) | nonce (12) | tag (16) | ciphertext
        var result = new byte[saltSize + nonceSize + tagSize + ciphertext.Length];
        Buffer.BlockCopy(salt, 0, result, 0, saltSize);
        Buffer.BlockCopy(nonce, 0, result, saltSize, nonceSize);
        Buffer.BlockCopy(tag, 0, result, saltSize + nonceSize, tagSize);
        Buffer.BlockCopy(ciphertext, 0, result, saltSize + nonceSize + tagSize, ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public string DecryptSecret(string encryptedSecret, string pin)
    {
        const int saltSize = 16;
        const int keySize = 32;
        const int iterations = 100_000;
        const int nonceSize = 12;
        const int tagSize = 16;

        var data = Convert.FromBase64String(encryptedSecret);
        if (data.Length < saltSize + nonceSize + tagSize)
            throw new CryptographicException("Invalid encrypted secret.");

        var salt = data[..saltSize];
        var nonce = data[saltSize..(saltSize + nonceSize)];
        var tag = data[(saltSize + nonceSize)..(saltSize + nonceSize + tagSize)];
        var ciphertext = data[(saltSize + nonceSize + tagSize)..];
        var plaintext = new byte[ciphertext.Length];

        var key = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterations, HashAlgorithmName.SHA256, keySize);

        using var aesGcm = new AesGcm(key, tagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
