using System.Net.Http.Headers;
using System.Net.Http.Json;
using NorvixHub.Contracts.Cases;
using NorvixHub.Contracts.Delivery;
using NorvixHub.Contracts.Documents;
using NorvixHub.Contracts.Intake;
using NorvixHub.IntegrationTests.Support;
using Xunit;

namespace NorvixHub.IntegrationTests.Delivery;

public sealed partial class DeliveryEndpointTests
{
    private static async Task<DeliveryPackageResponse> CreateReadyPackageAsync(HttpClient client)
    {
        var caseWorkspace = await CreateCaseAsync(client);
        var document = await UploadAndLinkDocumentAsync(client, caseWorkspace.Id, "ready.pdf");
        return await CreatePackageAsync(client, caseWorkspace.Id, [document.Id]);
    }

    private static async Task<DeliveryPackageResponse> CreatePackageAsync(
        HttpClient client,
        Guid caseId,
        IReadOnlyCollection<Guid> documentIds)
    {
        return await PostJsonAsync<DeliveryPackageResponse>(
            client,
            $"/api/cases/{caseId}/delivery-packages",
            new CreateDeliveryPackageRequest("Customer delivery", documentIds));
    }

    private static async Task<DeliveryPackageResponse> CreateLinkAsync(
        HttpClient client,
        Guid packageId,
        DateTimeOffset expiresAt)
    {
        return await PostJsonAsync<DeliveryPackageResponse>(
            client,
            $"/api/delivery-packages/{packageId}/create-link",
            new CreateDeliveryLinkRequest("recipient@example.test", expiresAt));
    }

    private static async Task<DocumentResponse> UploadAndLinkDocumentAsync(
        HttpClient client,
        Guid caseId,
        string filename)
    {
        var document = await UploadDocumentAsync(client, filename);
        using var response = await SendWithDemoAuthAsync(
            client,
            HttpMethod.Post,
            $"/api/documents/{document.Id}/link-to-case",
            new LinkDocumentToCaseRequest(caseId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DocumentResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<DocumentResponse> UploadDocumentAsync(HttpClient client, string filename)
    {
        var fileContent = new ByteArrayContent("fake delivery document"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        using var multipart = new MultipartFormDataContent
        {
            { fileContent, "file", filename },
            { new StringContent(filename), "title" }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/documents") { Content = multipart };
        DevAuthHeaders.AddDemoAdmin(request);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DocumentResponse>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<CaseResponse> CreateCaseAsync(HttpClient client)
    {
        var intake = await PostJsonAsync<IntakeItemResponse>(
            client,
            "/api/intakes",
            new CreateIntakeRequest(
                "Manual",
                $"Delivery {Guid.NewGuid():N}",
                "Delivery test.",
                "Sordal Eiendom AS",
                "999888777",
                null,
                "Normal"));
        return await PostJsonAsync<CaseResponse>(client, $"/api/intakes/{intake.Id}/convert-to-case");
    }

    private static async Task<T> PostJsonAsync<T>(HttpClient client, string url, object? body = null)
    {
        using var response = await SendWithDemoAuthAsync(client, HttpMethod.Post, url, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(
            TestContext.Current.CancellationToken))!;
    }

    private static async Task<T> GetPublicJsonAsync<T>(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url, TestContext.Current.CancellationToken);
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
