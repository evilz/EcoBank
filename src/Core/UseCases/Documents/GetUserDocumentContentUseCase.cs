using EcoBank.Core.Domain.Documents;
using EcoBank.Core.Ports;

namespace EcoBank.Core.UseCases.Documents;

public sealed class GetUserDocumentContentUseCase(IDocumentRepository documentRepository)
{
    public Task<UserDocumentContent?> ExecuteAsync(string appUserId, UserDocument document, CancellationToken ct = default)
    {
        return string.IsNullOrWhiteSpace(appUserId)
            ? Task.FromResult<UserDocumentContent?>(null)
            : documentRepository.GetDocumentContentAsync(appUserId, document.Key, document.Kind, ct);
    }
}
