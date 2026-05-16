# Screenshots

Screenshots are generated from the frontend shell and stored in `docs/screenshots/`.

## Current Captures

- `dashboard-desktop.png` - desktop operational dashboard.
- `dashboard-mobile.png` - mobile dashboard layout.

## Regenerate

From the repository root:

```powershell
cd frontend
npm run build
$env:PORT=3000
npm run start
```

Then capture:

```powershell
npx playwright screenshot http://localhost:3000 ../docs/screenshots/dashboard-desktop.png --viewport-size=1440,1000
npx playwright screenshot http://localhost:3000 ../docs/screenshots/dashboard-mobile.png --viewport-size=390,900
```

The screenshot step is part of portfolio polish and should be repeated after meaningful frontend changes.
