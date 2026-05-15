# ADR 0001 - Technology Stack

## Status

Accepted for MVP planning.

## Context

Norvix WorkFlow Hub should demonstrate credible integration and workflow capability for Norwegian organizations. The target market often uses Microsoft 365, Azure, Entra ID, SharePoint, Power Platform, and Microsoft-oriented consulting stacks.

The project must be functional enough to publish online, not only a theoretical demo.

## Decision

Use:

- Next.js App Router, TypeScript, Tailwind CSS for frontend.
- ASP.NET Core / .NET 10 and C# for backend.
- Entity Framework Core and PostgreSQL for persistence.
- Azure Blob-compatible storage, with Azurite locally.
- Microsoft Entra ID as production auth direction, with local dev auth stub first.
- Docker Compose for local dependencies.
- GitHub Actions for CI.
- Terraform for cloud infrastructure.

## Rationale

This stack supports:

- strong Microsoft/Azure market alignment;
- realistic B2B architecture;
- clean API and background worker separation;
- mature testing with xUnit and Testcontainers;
- practical deployment to Azure App Service or Azure Container Apps;
- future integration with Microsoft Graph, Power Platform, Azure AI, and Fabric.

## Consequences

Positive:

- credible for Norwegian Microsoft-oriented customers;
- good portfolio value;
- clear path from mock adapters to real integrations;
- strong security and observability ecosystem.

Tradeoffs:

- developer must maintain both TypeScript and C# codebases;
- .NET backend setup is heavier than a pure Node/TypeScript stack;
- cloud deployment needs careful Azure configuration and secret handling.

## Implementation Notes

- Keep architecture practical.
- Do not overbuild enterprise patterns before the MVP flow works.
- Build mock adapters first.
- Add real integrations only after the relevant workflow is stable.
