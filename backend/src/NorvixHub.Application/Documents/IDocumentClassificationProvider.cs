using NorvixHub.Domain.Documents;

namespace NorvixHub.Application.Documents;

public interface IDocumentClassificationProvider
{
    string Provider { get; }
    string Model { get; }
    string PromptVersion { get; }

    DocumentClassificationSuggestion Classify(DocumentRecord document, DocumentVersion version);
}

