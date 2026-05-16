using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Cases;
using NorvixHub.Contracts.Documents;
using NorvixHub.Contracts.Intake;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Documents;

public sealed partial class DocumentEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public DocumentEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Upload_document_creates_metadata_and_audit_event()
    {
        using var client = _factory.CreateClient();
        var auditBefore = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "Document",
            "DocumentUploaded");

        using var response = await SendMultipartAsync(client, HttpMethod.Post, "/api/documents", "demo.pdf", "application/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var document = await response.Content.ReadFromJsonAsync<DocumentResponse>(
            TestContext.Current.CancellationToken);
        document!.Status.Should().Be("Uploaded");
        document.CurrentVersionId.Should().NotBeNull();

        var auditAfter = await _factory.CountAuditEventsAsync(
            LocalDevTenantContext.DemoTenantId,
            "Document",
            "DocumentUploaded");
        auditAfter.Should().Be(auditBefore + 1);
    }

    [Fact]
    public async Task Document_can_be_listed_and_viewed()
    {
        using var client = _factory.CreateClient();
        var document = await UploadDocumentAsync(client);

        var documents = await GetJsonAsync<List<DocumentResponse>>(client, "/api/documents");
        documents.Should().Contain(item => item.Id == document.Id);

        var fetched = await GetJsonAsync<DocumentResponse>(client, $"/api/documents/{document.Id}");
        fetched.Id.Should().Be(document.Id);
    }

    [Fact]
    public async Task Uploaded_document_can_be_downloaded()
    {
        using var client = _factory.CreateClient();
        var document = await UploadDocumentAsync(client);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Get,
            $"/api/documents/{document.Id}/download");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        response.Content.Headers.ContentDisposition!.FileNameStar.Should().Be("demo.pdf");
        var content = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Upload_version_creates_next_version()
    {
        using var client = _factory.CreateClient();
        var document = await UploadDocumentAsync(client);

        using var response = await SendMultipartAsync(
            client,
            HttpMethod.Post,
            $"/api/documents/{document.Id}/versions",
            "version2.pdf",
            "application/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var version = await response.Content.ReadFromJsonAsync<DocumentVersionResponse>(
            TestContext.Current.CancellationToken);
        version!.VersionNumber.Should().Be(2);
    }

    [Fact]
    public async Task Document_can_be_linked_to_case()
    {
        using var client = _factory.CreateClient();
        var document = await UploadDocumentAsync(client);
        var caseWorkspace = await CreateCaseAsync(client);
        var body = new LinkDocumentToCaseRequest(caseWorkspace.Id);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/documents/{document.Id}/link-to-case",
            body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var linked = await response.Content.ReadFromJsonAsync<DocumentResponse>(
            TestContext.Current.CancellationToken);
        linked!.CaseId.Should().Be(caseWorkspace.Id);
    }

    [Fact]
    public async Task Analyze_document_creates_suggestion_without_approving_classification()
    {
        using var client = _factory.CreateClient();
        var document = await UploadDocumentAsync(client);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/documents/{document.Id}/analyze");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var classification = await response.Content.ReadFromJsonAsync<DocumentClassificationResponse>(
            TestContext.Current.CancellationToken);
        classification!.DocumentType.Should().Be("PDF documentation");

        var current = await GetJsonAsync<DocumentResponse>(client, $"/api/documents/{document.Id}");
        current.DocumentType.Should().BeNull();
        current.Status.Should().Be("NeedsReview");
    }

    [Fact]
    public async Task Approve_classification_applies_reviewed_document_type()
    {
        using var client = _factory.CreateClient();
        var document = await UploadDocumentAsync(client);
        var classification = await AnalyzeDocumentAsync(client, document.Id);
        var approve = new ApproveDocumentClassificationRequest(
            classification.AiAnalysisRunId,
            "Reviewed certificate",
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)));

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/documents/{document.Id}/approve-classification",
            approve);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var approved = await response.Content.ReadFromJsonAsync<DocumentResponse>(
            TestContext.Current.CancellationToken);
        approved!.DocumentType.Should().Be("Reviewed certificate");
        approved.Status.Should().Be("Approved");
    }

    private static async Task<DocumentResponse> UploadDocumentAsync(HttpClient client)
    {
        using var response = await SendMultipartAsync(client, HttpMethod.Post, "/api/documents", "demo.pdf", "application/pdf");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DocumentResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<DocumentClassificationResponse> AnalyzeDocumentAsync(HttpClient client, Guid documentId)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, $"/api/documents/{documentId}/analyze");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DocumentClassificationResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<CaseResponse> CreateCaseAsync(HttpClient client)
    {
        var intake = await CreateIntakeAsync(client);
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, $"/api/intakes/{intake.Id}/convert-to-case");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CaseResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<IntakeItemResponse> CreateIntakeAsync(HttpClient client)
    {
        var request = new CreateIntakeRequest(
            "Manual",
            $"Document case {Guid.NewGuid():N}",
            "Case for document linking.",
            "Sordal Eiendom AS",
            "999888777",
            "Documentation",
            "Normal");
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, "/api/intakes", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<T> GetJsonAsync<T>(HttpClient client, string url)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(
            TestContext.Current.CancellationToken))!;
    }

    private static Task<HttpResponseMessage> SendMultipartAsync(
        HttpClient client,
        HttpMethod method,
        string url,
        string filename,
        string contentType)
    {
        var request = CreateMultipartRequest(method, url, filename, contentType);
        DevAuthHeaders.AddDemoAdmin(request);
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static HttpRequestMessage CreateMultipartRequest(
        HttpMethod method,
        string url,
        string filename,
        string contentType)
    {
        var fileContent = new ByteArrayContent("fake document content"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var multipart = new MultipartFormDataContent
        {
            { fileContent, "file", filename },
            { new StringContent("Uploaded document"), "title" }
        };

        return new HttpRequestMessage(method, url) { Content = multipart };
    }

    private static Task<HttpResponseMessage> SendWithDemoAuthAsync(
        HttpClient client,
        HttpMethod method,
        string url,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        DevAuthHeaders.AddDemoAdmin(request);
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
