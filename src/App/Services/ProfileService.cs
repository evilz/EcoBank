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
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pin));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPin(string pin, string hash)
    {
        return HashPin(pin) == hash;
    }

    public string EncryptSecret(string secret, string pin)
    {
        // Simple base64 for demo purposes. True encryption would use AES derived from the PIN.
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(secret));
    }

    public string DecryptSecret(string encryptedSecret, string pin)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedSecret));
    }
}
