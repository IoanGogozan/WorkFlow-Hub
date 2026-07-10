# Screenshots

Screenshots are stored in `docs/screenshots/`. Public-demo captures require an
active fictional demo session, so the normal Playwright flow should create the
session before capture.

## Current Captures

- `dashboard-desktop.png` - pre-redesign technical dashboard at desktop width.
- `dashboard-mobile.png` - pre-redesign technical dashboard at mobile width.

These captures document the technical application and are not the current
client-facing presentation. Keep their names only until reviewed replacement
captures are intentionally committed.

## Regenerate

Start the complete local stack from the repository root:

```powershell
npm run dev
```

In another terminal, run the client E2E with responsive capture enabled. It
creates a fictional session, completes the timeline, verifies no horizontal
overflow, and captures the full page at 375, 768, and 1280 px:

```powershell
$env:CAPTURE_RESPONSIVE="1"
npm --prefix frontend run test:e2e -- automation-demo.spec.ts
```

Review the generated files in `frontend/test-results/` before intentionally
copying selected captures into `docs/screenshots/`. Playwright reports and test
results are temporary and must not be committed.

The screenshot step is part of portfolio polish and should be repeated after meaningful frontend changes.
