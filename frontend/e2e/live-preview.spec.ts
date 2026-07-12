import { expect, test } from "@playwright/test";

test("visitor can run a fresh internal live demo workflow", async ({ page }) => {
  const firstCaseNumber = await startAndCompleteRun(page);

  await page.evaluate(() => window.sessionStorage.clear());
  const secondCaseNumber = await startAndCompleteRun(page);

  expect(firstCaseNumber).not.toBe(secondCaseNumber);
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
