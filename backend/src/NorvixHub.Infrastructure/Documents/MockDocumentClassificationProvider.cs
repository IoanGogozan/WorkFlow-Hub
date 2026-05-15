using NorvixHub.Application.Documents;
using NorvixHub.Domain.Documents;

namespace NorvixHub.Infrastructure.Documents;

public sealed class MockDocumentClassificationProvider : IDocumentClassificationProvider
{
    public string Provider => "Mock";
    public string Model => "mock-document-classifier-v1";
    public string PromptVersion => "document-classification-2026-05-15";

    public DocumentClassificationSuggestion Classify(DocumentRecord document, DocumentVersion version)
    {
        var type = version.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase)
            ? "PDF documentation"
            : "Supporting document";

        return new DocumentClassificationSuggestion(
            type,
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            $"Document '{document.Title}' was classified as {type}.",
            0.84m);
    }
}

