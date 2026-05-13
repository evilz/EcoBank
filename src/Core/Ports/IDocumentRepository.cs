using EcoBank.Core.Domain.Documents;

namespace EcoBank.Core.Ports;

public interface IDocumentRepository
{
    Task<IReadOnlyList<UserDocument>> GetDocumentsAsync(string appUserId, CancellationToken ct = default);
    Task<UserDocumentContent?> GetDocumentContentAsync(string appUserId, string key, DocumentKind kind, CancellationToken ct = default);
}
