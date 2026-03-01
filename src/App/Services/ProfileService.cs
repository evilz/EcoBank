using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Ports;

namespace EcoBank.App.Services;

public class ProfileService
{
    private const string ProfilesStorageKey = "saved_profiles";
    private const int PinSaltSize = 16;
    private const int PinKeySize = 32;
    private const int PinIterations = 100_000;
    private const int SecretSaltSize = 16;
    private const int SecretKeySize = 32;
    private const int SecretIterations = 100_000;
    private const int AesNonceSize = 12;
    private const int AesTagSize = 16;

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
        var salt = new byte[PinSaltSize];
        RandomNumberGenerator.Fill(salt);

        var key = Rfc2898DeriveBytes.Pbkdf2(pin, salt, PinIterations, HashAlgorithmName.SHA256, PinKeySize);

        // Layout: iterations (4) | salt (16) | key (32)
        var result = new byte[sizeof(int) + PinSaltSize + PinKeySize];
        Buffer.BlockCopy(BitConverter.GetBytes(PinIterations), 0, result, 0, sizeof(int));
        Buffer.BlockCopy(salt, 0, result, sizeof(int), PinSaltSize);
        Buffer.BlockCopy(key, 0, result, sizeof(int) + PinSaltSize, PinKeySize);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPin(string pin, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return false;

        byte[] decoded;
        try { decoded = Convert.FromBase64String(hash); }
        catch (FormatException) { return false; }

        if (decoded.Length < sizeof(int) + PinSaltSize + PinKeySize) return false;

        var iterations = BitConverter.ToInt32(decoded, 0);
        if (iterations <= 0) return false;

        var salt = new byte[PinSaltSize];
        Buffer.BlockCopy(decoded, sizeof(int), salt, 0, PinSaltSize);

        var storedKey = new byte[PinKeySize];
        Buffer.BlockCopy(decoded, sizeof(int) + PinSaltSize, storedKey, 0, PinKeySize);

        var computedKey = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterations, HashAlgorithmName.SHA256, PinKeySize);

        return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
    }

    public string EncryptSecret(string secret, string pin)
    {
        var salt = new byte[SecretSaltSize];
        RandomNumberGenerator.Fill(salt);

        var key = Rfc2898DeriveBytes.Pbkdf2(pin, salt, SecretIterations, HashAlgorithmName.SHA256, SecretKeySize);

        var nonce = new byte[AesNonceSize];
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(secret);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesTagSize];

        using var aesGcm = new AesGcm(key, AesTagSize);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Layout: salt (16) | nonce (12) | tag (16) | ciphertext
        var result = new byte[SecretSaltSize + AesNonceSize + AesTagSize + ciphertext.Length];
        Buffer.BlockCopy(salt, 0, result, 0, SecretSaltSize);
        Buffer.BlockCopy(nonce, 0, result, SecretSaltSize, AesNonceSize);
        Buffer.BlockCopy(tag, 0, result, SecretSaltSize + AesNonceSize, AesTagSize);
        Buffer.BlockCopy(ciphertext, 0, result, SecretSaltSize + AesNonceSize + AesTagSize, ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public string DecryptSecret(string encryptedSecret, string pin)
    {
        var data = Convert.FromBase64String(encryptedSecret);
        if (data.Length < SecretSaltSize + AesNonceSize + AesTagSize)
            throw new CryptographicException("Invalid encrypted secret.");

        var salt = data[..SecretSaltSize];
        var nonce = data[SecretSaltSize..(SecretSaltSize + AesNonceSize)];
        var tag = data[(SecretSaltSize + AesNonceSize)..(SecretSaltSize + AesNonceSize + AesTagSize)];
        var ciphertext = data[(SecretSaltSize + AesNonceSize + AesTagSize)..];
        var plaintext = new byte[ciphertext.Length];

        var key = Rfc2898DeriveBytes.Pbkdf2(pin, salt, SecretIterations, HashAlgorithmName.SHA256, SecretKeySize);

        using var aesGcm = new AesGcm(key, AesTagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
