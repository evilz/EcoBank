namespace EcoBank.Core.Domain.Documents;

public record UserDocument(
    string Key,
    string Name,
    DocumentKind Kind,
    string? ContentType,
    DateTimeOffset? CreatedAt);

public record UserDocumentContent(
    string Key,
    string Name,
    DocumentKind Kind,
    string? ContentType,
    byte[] Content);

public enum DocumentKind
{
    Kyc,
    Fatca,
    BankStatement,
    Unknown
}
