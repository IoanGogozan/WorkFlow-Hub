using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.AI;
using NorvixHub.Contracts.Cases;
using NorvixHub.Contracts.Delivery;
using NorvixHub.Contracts.Documents;
using NorvixHub.Contracts.Intake;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Cases;

public sealed class CaseEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public CaseEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Convert_intake_to_case_creates_case_and_marks_intake_converted()
    {
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{intake.Id}/convert-to-case");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdCase = await response.Content.ReadFromJsonAsync<CaseResponse>(
            TestContext.Current.CancellationToken);
        createdCase!.SourceIntakeItemId.Should().Be(intake.Id);
        createdCase.Status.Should().Be("Open");

        var convertedIntake = await GetIntakeAsync(client, intake.Id);
        convertedIntake.Status.Should().Be("ConvertedToCase");
    }

    [Fact]
    public async Task Case_can_be_listed_viewed_and_show_activity()
    {
        using var client = _factory.CreateClient();
        var createdCase = await CreateCaseAsync(client);

        var cases = await GetJsonAsync<List<CaseListItemResponse>>(client, "/api/cases");
        cases.Should().Contain(item => item.Id == createdCase.Id);

        var fetchedCase = await GetJsonAsync<CaseResponse>(client, $"/api/cases/{createdCase.Id}");
        fetchedCase.Id.Should().Be(createdCase.Id);

        var activity = await GetJsonAsync<List<CaseActivityResponse>>(
            client,
            $"/api/cases/{createdCase.Id}/activity");
        activity.Should().Contain(item => item.Action == "CaseCreated");
    }

    [Fact]
    public async Task Case_activity_includes_related_intake_document_delivery_and_public_access_events()
    {
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client);
        var analysis = await PostJsonAsync<AiAnalysisRunResponse>(
            client,
            $"/api/intakes/{intake.Id}/analyze");
        await PostJsonAsync<IntakeItemResponse>(
            client,
            $"/api/intakes/{intake.Id}/approve-ai",
            new ApproveAiSuggestionRequest(
                analysis.Id,
                analysis.Suggestion.CustomerName,
                analysis.Suggestion.OrganizationNumber,
                analysis.Suggestion.Category,
                analysis.Suggestion.Urgency));
        var createdCase = await PostJsonAsync<CaseResponse>(
            client,
            $"/api/intakes/{intake.Id}/convert-to-case");

        var document = await PostJsonAsync<DocumentResponse>(client, "/api/documents/sample");
        var classification = await PostJsonAsync<DocumentClassificationResponse>(
            client,
            $"/api/documents/{document.Id}/analyze");
        await PostJsonAsync<DocumentResponse>(
            client,
            $"/api/documents/{document.Id}/approve-classification",
            new ApproveDocumentClassificationRequest(
                classification.AiAnalysisRunId,
                classification.DocumentType,
                classification.ExpiryDate));
        await PostJsonAsync<DocumentResponse>(
            client,
            $"/api/documents/{document.Id}/link-to-case",
            new LinkDocumentToCaseRequest(createdCase.Id));

        var package = await PostJsonAsync<DeliveryPackageResponse>(
            client,
            $"/api/cases/{createdCase.Id}/delivery-packages",
            new CreateDeliveryPackageRequest("Audit demo package", [document.Id]));
        package = await PostJsonAsync<DeliveryPackageResponse>(
            client,
            $"/api/delivery-packages/{package.Id}/generate-pdf");
        package = await PostJsonAsync<DeliveryPackageResponse>(
            client,
            $"/api/delivery-packages/{package.Id}/create-link",
            new CreateDeliveryLinkRequest(null, DateTimeOffset.UtcNow.AddDays(7)));
        var token = package.Links.Single(link => link.Token is not null).Token;
        using (var publicResponse = await client.GetAsync(
            $"/delivery/{token}",
            TestContext.Current.CancellationToken))
        {
            publicResponse.EnsureSuccessStatusCode();
        }

        var activity = await GetJsonAsync<List<CaseActivityResponse>>(
            client,
            $"/api/cases/{createdCase.Id}/activity");

        activity.Should().Contain(item => item.Action == "IntakeCreated");
        activity.Should().Contain(item => item.Action == "AiAnalysisRequested");
        activity.Should().Contain(item => item.Action == "AiSuggestionApproved");
        activity.Should().Contain(item => item.Action == "CaseCreated");
        activity.Should().Contain(item => item.Action == "SampleDocumentCreated");
        activity.Should().Contain(item => item.Action == "DocumentClassificationRequested");
        activity.Should().Contain(item => item.Action == "DocumentClassificationApproved");
        activity.Should().Contain(item => item.Action == "DocumentLinkedToCase");
        activity.Should().Contain(item => item.Action == "DeliveryPackageCreated");
        activity.Should().Contain(item => item.Action == "DeliveryPdfGenerated");
        activity.Should().Contain(item => item.Action == "DeliveryLinkCreated");
        activity.Should().Contain(item => item.EntityType == "PublicDelivery" && item.Action == "ViewedPackage");
    }

    [Fact]
    public async Task User_can_add_task_and_note_to_case()
    {
        using var client = _factory.CreateClient();
        var createdCase = await CreateCaseAsync(client);
        var taskBody = new CreateCaseTaskRequest("Check documentation", "Review attached files", null);
        var noteBody = new CreateCaseNoteRequest("Customer called with extra context.", "Internal");

        using var taskResponse = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/cases/{createdCase.Id}/tasks",
            taskBody);
        using var noteResponse = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/cases/{createdCase.Id}/notes",
            noteBody);

        taskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        noteResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Viewer_cannot_convert_intake_to_case()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/intakes/{intake.Id}/convert-to-case");
        DevAuthHeaders.Add(request, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Accessing_case_from_another_tenant_returns_not_found()
    {
        await _factory.SeedExtraTenantsAsync();
        var otherTenantCaseId = await _factory.CreateSecondTenantCaseAsync();
        using var client = _factory.CreateClient();

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Get,
            $"/api/cases/{otherTenantCaseId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Add_task_rejects_missing_title()
    {
        using var client = _factory.CreateClient();
        var createdCase = await CreateCaseAsync(client);
        var body = new CreateCaseTaskRequest(" ", null, null);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/cases/{createdCase.Id}/tasks",
            body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<CaseResponse> CreateCaseAsync(HttpClient client)
    {
        var intake = await CreateIntakeAsync(client);
        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{intake.Id}/convert-to-case");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CaseResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<IntakeItemResponse> CreateIntakeAsync(HttpClient client)
    {
        var body = new CreateIntakeRequest(
            "Manual",
            $"Case workspace request {Guid.NewGuid():N}",
            "Customer needs a case workspace.",
            "Sordal Eiendom AS",
            "999888777",
            "Operations",
            "Normal");
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, "/api/intakes", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<IntakeItemResponse> GetIntakeAsync(HttpClient client, Guid intakeId)
    {
        return await GetJsonAsync<IntakeItemResponse>(client, $"/api/intakes/{intakeId}");
    }

    private static async Task<T> GetJsonAsync<T>(HttpClient client, string url)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<T> PostJsonAsync<T>(
        HttpClient client,
        string url,
        object? body = null)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, url, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(
            TestContext.Current.CancellationToken))!;
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
