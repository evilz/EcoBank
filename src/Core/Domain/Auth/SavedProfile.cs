namespace EcoBank.Core.Domain.Auth;

public record SavedProfile(
    string Id,
    string ClientId,
    string AppUserId,
    string EncryptedSecret,
    string PinHash,
    string? FirstName,
    string? LastName
)
{
    public string DisplayName => !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName) 
        ? $"{FirstName} {LastName}".Trim() 
        : AppUserId;

    public string MaskedClientId 
    {
        get
        {
            if (string.IsNullOrEmpty(ClientId) || ClientId.Length <= 6) return ClientId;
            return ClientId[..3] + new string('•', ClientId.Length - 6) + ClientId[^3..];
        }
    }
}
