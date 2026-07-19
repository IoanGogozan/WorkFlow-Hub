# WorkFlow Hub Frontend

Next.js frontend for the WorkFlow Hub portfolio landing page, interactive demo, technical application, and run-specific evidence views.

## Public Routes

| Route | Purpose |
| --- | --- |
| `/` | Portfolio landing page |
| `/demo` | Sandbox boundaries and session creation |
| `/demo/run` | Interactive worker-backed workflow |
| `/technical` | Technical application entry |
| `/technical/live-runs/[runId]` | Evidence for one exact run |

Compatibility routes such as `/automation`, `/summary`, and `/live-preview` redirect into the current demo journey.

## Development

From the repository root, prefer the complete local stack:

```powershell
npm run dev
```

To work on the frontend only:

```powershell
npm --prefix frontend install
npm --prefix frontend run dev
```

The frontend uses `NEXT_PUBLIC_API_BASE_URL` when the API is not available through the same origin.

## Verification

```powershell
npm --prefix frontend run lint
npm --prefix frontend run build
npm --prefix frontend run test:e2e
```

The full E2E suite expects the API, worker, PostgreSQL, and ERP demo receiver. Use `npm run test:e2e:public-demo` from the repository root to start the supported test environment.

## Presentation Rules

- Keep `/` understandable without technical context.
- Keep the public scenario fictional and bounded.
- Derive environment-dependent claims from capability flags.
- Label Brreg fallback, SharePoint simulation, and the ERP demo receiver explicitly.
- Keep detailed implementation evidence available without forcing it into the primary visitor journey.
