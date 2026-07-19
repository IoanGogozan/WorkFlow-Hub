# Public Demo Boundaries

## Purpose

The public environment is an inspectable portfolio sandbox. It is designed to demonstrate implementation decisions safely, not to host operational customer workflows.

## Data

- Use fictional data and generated demo documents only.
- Do not enter customer, employee, supplier, or confidential project information.
- Seeded names, organization details, requests, references, and attachments exist only to make the scenario understandable.

## Session Isolation

- Starting the demo creates a temporary tenant and user context for the visitor.
- Demo API requests use the temporary session token.
- Tenant-scoped queries prevent one demo workspace from reading another workspace's business data.
- Sessions expire and cleanup removes associated demo data and stored files.

## External Systems

- Brreg is the only public third-party data service contacted by the main run when enabled.
- A labelled deterministic fallback keeps the scenario usable when Brreg is unavailable.
- No Outlook or customer mailbox is connected.
- No Microsoft tenant or Microsoft Graph endpoint is connected.
- No customer accounting or project system is connected.
- SharePoint and ERP behavior follows the classifications in [Integration Boundaries](integration-boundaries.md).

## AI and Human Control

AI-related screens demonstrate stored suggestions and a review workflow. The public integration run is deterministic and must not be described as an autonomous AI agent. Any production analysis feature would require provider selection, data-handling rules, evaluation, monitoring, and explicit human-control decisions.

## Operational Limits

- Availability is best-effort and no service-level objective is offered.
- Demo state can be reset or removed without notice.
- Rate limits may reject repeated automated use.
- Generated links and evidence are scoped to the temporary environment.
- The environment is not approved for real business processing.

## Required Public Notice

The demo entry and final call to action should communicate, in plain language:

- fictional data only;
- no account required;
- temporary isolated workspace;
- no customer systems connected;
- public state expires automatically.
