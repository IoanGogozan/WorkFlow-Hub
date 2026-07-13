import { expect, test } from "@playwright/test";

test("visitor can run a fresh internal live demo workflow", async ({ page }) => {
  const firstCaseNumber = await startAndCompleteRun(page);

  await page.evaluate(() => window.sessionStorage.clear());
  const secondCaseNumber = await startAndCompleteRun(page);

  expect(firstCaseNumber).not.toBe(secondCaseNumber);
});

test("shows live Brreg evidence with the measured duration", async ({ page }) => {
  await showMockedBrregRun(page, "live", 800);

  await expect(page.getByText("Brreg", { exact: true }).last()).toBeVisible();
  await expect(page.getByText("Live kontroll – 0,8 sek")).toBeVisible();
  await expect(page.getByText("Brreg: Live kontroll")).toBeVisible();
});

test("shows fallback Brreg evidence without presenting it as live", async ({ page }) => {
  await showMockedBrregRun(page, "fallback", 800);

  await expect(page.getByText("Fallback-snapshot – live tjeneste var utilgjengelig")).toBeVisible();
  await expect(page.getByText("Brreg: Fallback-snapshot")).toBeVisible();
  await expect(page.getByText("Live kontroll – 0,8 sek")).toHaveCount(0);
});

test("labels SharePoint synchronization as simulated", async ({ page }) => {
  await showMockedBrregRun(page, "live", 800);

  const resultCard = page.locator("section[aria-labelledby='live-demo-result-heading']");
  await expect(
    resultCard.getByText(/Simulated SharePoint adapter — no Microsoft 365 tenant connected/),
  ).toBeVisible();
  await expect(page.getByText(/SharePoint connected/i)).toHaveCount(0);
});

async function startAndCompleteRun(page: import("@playwright/test").Page) {
  await page.goto("/demo");
  await page.getByRole("button", { name: "Se automatiseringen" }).click();
  await expect(page).toHaveURL(/\/$/);
  await page.goto("/live-preview");

  await expect(
    page.getByRole("heading", {
      name: "Fra henvendelse til sak og SharePoint – på sekunder",
    }),
  ).toBeVisible();
  const createResponse = page.waitForResponse((response) =>
    response.request().method() === "POST" &&
    response.url().endsWith("/api/live-demo-runs") &&
    response.status() === 202,
  );
  await page.getByRole("button", { name: "Kjør live demo" }).click();
  const created = await createResponse;
  const createdBody = (await created.json()) as { runId: string; status: string };
  expect(createdBody.status).toBe("Queued");
  expect(createdBody.runId).not.toBeFalsy();

  await expect(page.getByText("Venter", { exact: true }).first()).toBeVisible();
  const resultHeading = page.getByRole("heading", { name: /Fullført på/ });
  await expect(resultHeading).toBeVisible({ timeout: 20_000 });
  const caseText = await page
    .locator("li")
    .filter({ hasText: "Sak LIVE-" })
    .first()
    .textContent();
  const caseNumber = caseText?.match(/LIVE-[0-9]{4}-[A-F0-9]+/)?.[0];
  expect(caseNumber).toBeTruthy();

  await expect(page.getByText("Venter", { exact: true }).first()).toBeVisible();
  await expect(page.getByText(/ERP demo receiver:/)).toHaveCount(0);

  const manualProcess = page
    .locator("details")
    .filter({ hasText: "Slik ser den manuelle prosessen ofte ut" });
  await expect(manualProcess).not.toHaveAttribute("open", "");
  const manualSummary = manualProcess.getByText("Slik ser den manuelle prosessen ofte ut");
  await manualSummary.focus();
  await page.keyboard.press("Enter");
  await expect(manualProcess).toHaveAttribute("open", "");

  for (const width of [375, 768, 1280]) {
    await page.setViewportSize({ width, height: 900 });
    expect(await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth)).toBe(false);
  }

  return caseNumber!;
}

async function showMockedBrregRun(
  page: import("@playwright/test").Page,
  mode: "live" | "fallback",
  durationMs: number,
) {
  const runId = "11111111-1111-4111-8111-111111111111";
  await page.addInitScript(() => {
    window.sessionStorage.setItem("norvix.demoSessionToken", "e2e-token");
    window.sessionStorage.setItem("norvix.demoSessionExpiresAt", "2099-01-01T00:00:00.000Z");
  });
  await page.route("**/api/live-demo-capabilities", async (route) => {
    await route.fulfill({ json: { enabled: true, brregLiveEnabled: true, sharePointEnabled: false, erpReceiverEnabled: false, failureDemoEnabled: false } });
  });
  await page.route("**/api/live-demo-runs", async (route) => {
    if (route.request().method() === "POST") {
      await route.fulfill({ status: 202, json: { runId } });
      return;
    }
    await route.continue();
  });
  await page.route(`**/api/live-demo-runs/${runId}`, async (route) => {
    await route.fulfill({ json: createCompletedRun(runId, mode, durationMs) });
  });

  await page.goto("/live-preview");
  await page.getByRole("button", { name: "Kjør live demo" }).click();
  await expect(page.getByRole("heading", { name: /Fullført på/ })).toBeVisible();
}

function createCompletedRun(runId: string, mode: "live" | "fallback", durationMs: number) {
  const steps = [
    ["request-created", 1, "Mottatt", "Norvix WorkFlow Hub", "implemented"],
    ["brreg-checked", 2, "Kontrollert", "Brreg", "live-or-fallback"],
    ["case-created", 3, "Opprettet", "Norvix WorkFlow Hub", "implemented"],
    ["document-created", 4, "Opprettet", "Norvix WorkFlow Hub", "implemented"],
    ["sharepoint-synced", 5, "Synkronisert", "SharePoint simulator", "simulated-sharepoint"],
    ["erp-received", 6, "Synkronisert", "ERP demo receiver", "demo-receiver"],
    ["run-completed", 7, "Synkronisert", "Norvix WorkFlow Hub", "implemented"],
  ].map(([key, sequence, publicStage, provider, evidenceMode]) => ({
    key,
    sequence,
    publicStage,
    provider,
    status: "Completed",
    evidenceMode,
    attemptCount: 1,
    durationMs: key === "brreg-checked" ? durationMs : 10,
    publicSummary: key === "brreg-checked"
      ? "Firmadata kontrollert."
      : key === "sharepoint-synced"
        ? "Simulated SharePoint adapter — no Microsoft 365 tenant connected."
        : "Steg fullført.",
    publicEvidenceReference: key === "brreg-checked"
      ? mode
      : key === "sharepoint-synced"
        ? "01SP-DEMO-ABCD"
        : "RUN-STEP",
    publicErrorCode: null,
    publicErrorMessage: null,
  }));

  return {
    runId,
    status: "Completed",
    currentStepKey: "run-completed",
    createdAt: "2026-07-12T10:00:00.000Z",
    startedAt: "2026-07-12T10:00:00.000Z",
    completedAt: "2026-07-12T10:00:02.000Z",
    totalDurationMs: 2000,
    retryCount: 0,
    canRetry: false,
    publicErrorCode: null,
    publicErrorMessage: null,
    steps,
    result: {
      caseNumber: "LIVE-2026-ABCD1234",
      brregMode: mode,
      sharePointFolderReference: "Customers/CASE-2026-ABCD",
      sharePointFileReference: "01SP-DEMO-ABCD",
      erpReceiptId: null,
      auditEventCount: 6,
      evidenceHref: `/technical/live-runs/${runId}`,
      caseHref: "/cases/case-id",
      documentHref: "/documents/document-id",
      documentDownloadHref: "/api/documents/document-id/download",
      sharePointEvidenceHref: `/technical/live-runs/${runId}#sharepoint`,
      auditHref: `/technical/live-runs/${runId}#audit`,
    },
  };
}
