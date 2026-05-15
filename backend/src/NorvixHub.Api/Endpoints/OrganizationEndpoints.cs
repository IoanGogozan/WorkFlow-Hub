using Microsoft.EntityFrameworkCore;
using NorvixHub.Application.Audit;
using NorvixHub.Application.Organizations;
using NorvixHub.Application.Tenancy;
using NorvixHub.Contracts.Customers;
using NorvixHub.Contracts.Organizations;
using NorvixHub.Domain.Customers;
using NorvixHub.Domain.Users;
using NorvixHub.Infrastructure.Persistence;

namespace NorvixHub.Api.Endpoints;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/organizations/search", SearchOrganizations).WithName("SearchOrganizations");
        app.MapGet("/api/organizations/brreg/{orgNumber}", GetOrganization).WithName("GetOrganization");
        app.MapPost("/api/customers/from-brreg", CreateCustomerFromBrreg).WithName("CreateCustomerFromBrreg");
        return app;
    }

    private static async Task<IResult> SearchOrganizations(
        string query,
        ITenantContext tenantContext,
        IBrregClient brregClient,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Results.Unauthorized();
        }

        if (!IsValidQuery(query, out var error))
        {
            return Results.BadRequest(new { error });
        }

        var results = await brregClient.SearchAsync(query.Trim(), cancellationToken);
        return Results.Ok(results.Select(ToResponse).ToList());
    }

    private static async Task<IResult> GetOrganization(
        string orgNumber,
        ITenantContext tenantContext,
        IBrregClient brregClient,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Results.Unauthorized();
        }

        if (!IsValidOrganizationNumber(orgNumber))
        {
            return Results.BadRequest(new { error = "Organization number must be exactly 9 digits." });
        }

        var organization = await brregClient.GetByOrganizationNumberAsync(orgNumber, cancellationToken);
        return organization is null ? Results.NotFound() : Results.Ok(ToResponse(organization));
    }

    private static async Task<IResult> CreateCustomerFromBrreg(
        CreateCustomerFromBrregRequest request,
        ITenantContext tenantContext,
        IBrregClient brregClient,
        NorvixHubDbContext dbContext,
        IAuditEventWriter auditEventWriter,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!CanManageCustomers(tenantContext))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!IsValidOrganizationNumber(request.OrganizationNumber))
        {
            return Results.BadRequest(new { error = "Organization number must be exactly 9 digits." });
        }

        var organization = await brregClient.GetByOrganizationNumberAsync(
            request.OrganizationNumber,
            cancellationToken);
        if (organization is null)
        {
            return Results.NotFound();
        }

        var tenantId = tenantContext.TenantId!.Value;
        var existing = await dbContext.Customers.SingleOrDefaultAsync(
            customer => customer.TenantId == tenantId && customer.OrganizationNumber == request.OrganizationNumber,
            cancellationToken);
        if (existing is not null)
        {
            return Results.Ok(ToResponse(existing));
        }

        var customer = new Customer
        {
            TenantId = tenantId,
            CreatedBy = tenantContext.UserId,
            Name = organization.Name,
            OrganizationNumber = organization.OrganizationNumber,
            BrregDataJson = organization.RawJson,
            SourceUpdatedAt = organization.SourceUpdatedAt
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(auditEventWriter, customer, tenantContext, httpContext, cancellationToken);
        return Results.Created($"/api/customers/{customer.Id}", ToResponse(customer));
    }

    private static bool CanManageCustomers(ITenantContext tenantContext)
    {
        return tenantContext.Role is TenantRole.TenantOwner or TenantRole.Admin or TenantRole.OperationsUser;
    }

    private static bool IsValidQuery(string? query, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            error = "Search query must contain at least 2 characters.";
            return false;
        }

        return true;
    }

    private static bool IsValidOrganizationNumber(string orgNumber)
    {
        return orgNumber.Length == 9 && orgNumber.All(char.IsDigit);
    }

    private static OrganizationSearchResultResponse ToResponse(BrregOrganization organization)
    {
        return new OrganizationSearchResultResponse(
            organization.OrganizationNumber,
            organization.Name,
            organization.OrganizationForm,
            organization.Municipality,
            organization.PostalAddress,
            organization.IsDeleted,
            organization.SourceUpdatedAt);
    }

    private static CustomerResponse ToResponse(Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.TenantId,
            customer.Name,
            customer.OrganizationNumber,
            customer.Source,
            customer.SourceUpdatedAt,
            customer.CreatedAt);
    }

    private static Task WriteAuditAsync(
        IAuditEventWriter auditEventWriter,
        Customer customer,
        ITenantContext tenantContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var request = new AuditEventRequest(
            customer.TenantId,
            tenantContext.UserId,
            "User",
            "Customer",
            customer.Id.ToString(),
            "CustomerCreatedFromBrreg",
            null,
            $$"""{"organizationNumber":"{{customer.OrganizationNumber}}"}""",
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString(),
            httpContext.TraceIdentifier);

        return auditEventWriter.WriteAsync(request, cancellationToken);
    }
}

