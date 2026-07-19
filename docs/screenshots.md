# Screenshots

Screenshots are stored in `docs/screenshots/`. Public-demo captures require an
active fictional demo session, so the normal Playwright flow should create the
session before capture.

## Current Captures

- `landing-desktop.png` - current portfolio landing page at desktop width.
- `landing-mobile.png` - current portfolio landing page at mobile width.
- `dashboard-desktop.png` - pre-redesign technical dashboard at desktop width.
- `dashboard-mobile.png` - pre-redesign technical dashboard at mobile width.

The dashboard captures document the technical application and are retained as
historical technical views. The landing captures represent the current public
presentation.

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

The static landing page can also be captured directly from a production build:

```powershell
npm --prefix frontend run build
npm --prefix frontend run start -- -p 3200
npx --prefix frontend playwright screenshot --browser chromium --viewport-size "1280,900" --full-page http://localhost:3200 docs/screenshots/landing-desktop.png
npx --prefix frontend playwright screenshot --browser chromium --viewport-size "390,844" --full-page http://localhost:3200 docs/screenshots/landing-mobile.png
```

Review generated files before intentionally placing them in `docs/screenshots/`.
Playwright reports and test results are temporary and must not be committed.

The screenshot step is part of portfolio polish and should be repeated after meaningful frontend changes.
