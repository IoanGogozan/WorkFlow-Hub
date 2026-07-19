# Three-Minute Demo Script

Open the portfolio landing page, then select **Launch interactive demo**.

## 1. Establish the Problem

Explain that a service request normally triggers repeated work across email, customer records, case management, document storage, an ERP or project system, and reporting.

The demo uses one fictional pump-station request so the complete handoff remains easy to follow.

## 2. Start an Isolated Workspace

On `/demo`, point out:

- no registration is required;
- all data is fictional;
- no customer systems are connected;
- the workspace expires automatically.

Start the demo and continue to the interactive run.

## 3. Run the Workflow

Select **Kjør live demo** and follow the four public stages:

1. request received;
2. organization checked;
3. case and document created;
4. downstream systems synchronized.

Explain that the backend worker executes persisted steps. The timeline is not a browser-only animation.

## 4. Read the Result

Show the generated case number, document, Brreg mode, SharePoint simulator reference, ERP receipt, total duration, and audit-event count.

If Brreg used fallback, call it out. If a capability is unavailable, do not describe it as completed.

## 5. Inspect the Evidence

Open the exact run evidence page. Show:

- source request;
- live or fallback Brreg evidence;
- case and document references;
- generated PDF access;
- SharePoint simulator history;
- ERP receipt and attempts;
- audit timeline.

## 6. Demonstrate Recovery

When the controlled failure capability is enabled, run the failure scenario. Show the failed ERP step and use retry. Explain that the downstream operations use idempotency so retrying the workflow does not intentionally duplicate the completed artifacts.

## 7. Close Honestly

End with this message:

> WorkFlow Hub is a portfolio demonstration of one bounded automation pattern. Brreg can be live, SharePoint is a functional local simulator, and ERP is a separate project-owned demo receiver. A real deployment would connect customer-owned systems and add production identity, governance, and operations.

## Supporting Documents

- [Product brief](product-brief.md)
- [Integration boundaries](integration-boundaries.md)
- [Public demo boundaries](demo-boundaries.md)
- [Detailed product walkthrough](../product-walkthrough.md)
