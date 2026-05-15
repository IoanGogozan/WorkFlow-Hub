using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.AI;
using NorvixHub.Contracts.Intake;
using NorvixHub.Contracts.Reviews;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.AI;

public sealed class AiReviewEndpointTests : IClassFixture<NorvixHubApiFactory>
{
    private readonly NorvixHubApiFactory _factory;

    public AiReviewEndpointTests(NorvixHubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Analyze_intake_creates_ai_run_and_review_task_without_applying_suggestion()
    {
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client, customerName: null, category: null);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{intake.Id}/analyze");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var analysis = await response.Content.ReadFromJsonAsync<AiAnalysisRunResponse>(
            TestContext.Current.CancellationToken);
        analysis.Should().NotBeNull();
        analysis!.Status.Should().Be("NeedsReview");
        analysis.Suggestion.CustomerName.Should().NotBeNullOrWhiteSpace();

        var current = await GetIntakeAsync(client, intake.Id);
        current.CustomerName.Should().BeNull();
        current.Category.Should().BeNull();
        current.Status.Should().Be("NeedsReview");

        var reviewTasks = await ListReviewTasksAsync(client);
        reviewTasks.Should().Contain(task =>
            task.EntityId == intake.Id &&
            task.AiAnalysisRunId == analysis.Id &&
            task.Status == "Pending");
    }

    [Fact]
    public async Task Approve_ai_suggestion_applies_reviewed_fields()
    {
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client, customerName: null, category: null);
        var analysis = await AnalyzeAsync(client, intake.Id);
        var approve = new ApproveAiSuggestionRequest(
            analysis.Id,
            "Reviewed Customer AS",
            "999111222",
            "Reviewed category",
            "High");

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{intake.Id}/approve-ai",
            approve);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken);
        body!.CustomerName.Should().Be("Reviewed Customer AS");
        body.Category.Should().Be("Reviewed category");
        body.Urgency.Should().Be("High");
        body.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task Reject_ai_suggestion_does_not_apply_suggested_fields()
    {
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client, customerName: null, category: null);
        var analysis = await AnalyzeAsync(client, intake.Id);
        var reject = new ApproveAiSuggestionRequest(
            analysis.Id,
            "Rejected Customer AS",
            "999111222",
            "Rejected category",
            "High");

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{intake.Id}/reject-ai",
            reject);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var current = await GetIntakeAsync(client, intake.Id);
        current.CustomerName.Should().BeNull();
        current.Category.Should().BeNull();
    }

    [Fact]
    public async Task Analyze_without_auth_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsync(
            $"/api/intakes/{Guid.NewGuid()}/analyze",
            null,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Viewer_cannot_analyze_intake()
    {
        await _factory.SeedExtraTenantsAsync();
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client, customerName: null, category: null);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/intakes/{intake.Id}/analyze");
        DevAuthHeaders.Add(request, LocalDevTenantContext.DemoTenantId, NorvixHubApiFactory.ViewerUserId);

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Analyze_intake_from_another_tenant_returns_not_found()
    {
        await _factory.SeedExtraTenantsAsync();
        var otherTenantIntake = await _factory.CreateSecondTenantIntakeAsync();
        using var client = _factory.CreateClient();

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{otherTenantIntake.Id}/analyze");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Approve_unknown_ai_run_returns_not_found()
    {
        using var client = _factory.CreateClient();
        var intake = await CreateIntakeAsync(client, customerName: null, category: null);
        var approve = new ApproveAiSuggestionRequest(Guid.NewGuid(), "A", null, "B", null);

        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{intake.Id}/approve-ai",
            approve);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<IntakeItemResponse> CreateIntakeAsync(
        HttpClient client,
        string? customerName,
        string? category)
    {
        var body = new CreateIntakeRequest(
            "Manual",
            $"AI review request {Guid.NewGuid():N}",
            "Urgent document review request for operational follow-up.",
            customerName,
            null,
            category,
            null);
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, "/api/intakes", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<AiAnalysisRunResponse> AnalyzeAsync(HttpClient client, Guid intakeId)
    {
        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/intakes/{intakeId}/analyze");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AiAnalysisRunResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<IntakeItemResponse> GetIntakeAsync(HttpClient client, Guid intakeId)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Get, $"/api/intakes/{intakeId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IntakeItemResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<List<ReviewTaskResponse>> ListReviewTasksAsync(HttpClient client)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Get, "/api/review-tasks");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<ReviewTaskResponse>>(
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

