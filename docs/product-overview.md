# Product Overview

This document is retained as a stable entry point for existing links. The current product narrative is maintained in the following focused documents:

- [Product Brief](product/product-brief.md) — problem, audience, scenario, principles, and non-goals.
- [Integration Boundaries](product/integration-boundaries.md) — authoritative classification of real, simulated, seeded, and demo-receiver behavior.
- [Public Demo Boundaries](product/demo-boundaries.md) — data, session, external-system, and operational limits.
- [Three-Minute Demo Script](product/demo-script.md) — primary portfolio walkthrough.
- [Detailed Product Walkthrough](product-walkthrough.md) — client-facing and technical presentation paths.
- [Current Implementation Status](current-implementation-status.md) — technical baseline and validation record.

## One-Sentence Position

Norvix WorkFlow Hub is a verifiable integration demo that turns a fictional service request into a tenant-scoped case, document, downstream system updates, and inspectable audit evidence.

## Public Route Structure

| Route | Purpose |
| --- | --- |
| `/` | Portfolio landing page and product explanation |
| `/demo` | Demo boundaries and temporary workspace creation |
| `/demo/run` | Interactive worker-backed workflow |
| `/technical` | Broader application and implementation evidence |
| `/technical/live-runs/{runId}` | Evidence for one exact workflow run |

## Presentation Rule

Lead with the manual handoff problem. Use technology and technical screens as evidence after the visitor understands the workflow. Never describe the SharePoint simulator as Microsoft 365, the ERP demo receiver as a customer accounting system, a Brreg fallback as a live response, or stored AI suggestions as autonomous decisions.
