import { expect, test } from "@playwright/test";

test("visitor can verify the exact artifacts created by a live run", async ({ page }) => {
  await page.goto("/demo");
  await page.getByRole("button", { name: "Se automatiseringen" }).click();
  await page.goto("/live-preview");

  await page.getByRole("button", { name: "Kjør live demo" }).click();
  await expect(page.getByRole("heading", { name: /Fullført på/ })).toBeVisible({ timeout: 30_000 });

  const caseResult = page.locator("li").filter({ hasText: "Sak LIVE-" }).first();
  const caseNumber = (await caseResult.textContent())?.match(/LIVE-[0-9]{4}-[A-F0-9]+/)?.[0];
  expect(caseNumber).toBeTruthy();

  await page.getByRole("link", { name: "Se hva som faktisk ble opprettet" }).click();
  await expect(page).toHaveURL(/\/technical\/live-runs\/[0-9a-f-]+$/i);
  await expect(page.getByRole("heading", { name: "Verifiserbar dokumentasjon for én kjøring" })).toBeVisible();
  await expect(page.getByText("Fiktive data", { exact: true })).toBeVisible();

  const caseCard = page.getByRole("article").filter({ hasText: "Opprettet sak" });
  await expect(caseCard.getByText(caseNumber!, { exact: true })).toBeVisible();
  const caseHref = await caseCard.getByRole("link", { name: "Åpne saken" }).getAttribute("href");
  expect(caseHref).toMatch(/^\/cases\/[0-9a-f-]+$/i);
  await caseCard.getByRole("link", { name: "Åpne saken" }).click();
  await expect(page).toHaveURL(new RegExp(`${caseHref}$`, "i"));
  await expect(page.getByText(caseNumber!, { exact: true })).toBeVisible();
  await page.goBack();

  const documentCard = page.getByRole("article").filter({ hasText: "Opprettet dokument" });
  await expect(documentCard.getByText(/\.pdf$/i).first()).toBeVisible();
  await expect(documentCard.getByRole("link", { name: "Åpne dokumentdetaljer" })).toHaveAttribute(
    "href",
    /^\/documents\/[0-9a-f-]+$/i,
  );
  const pdfResponse = page.waitForResponse((response) =>
    response.url().includes("/api/documents/") &&
    response.url().endsWith("/download"),
  );
  await documentCard.getByRole("button", { name: "Åpne demo-PDF" }).click();
  expect((await pdfResponse).status()).toBe(200);

  const sharePoint = page.locator("#sharepoint");
  await expect(sharePoint.getByRole("heading", { name: "Lokal SharePoint-simulator" })).toBeVisible();
  await expect(sharePoint.getByText("Ingen Microsoft 365-konto er tilkoblet.")).toBeVisible();
  expect(await sharePoint.getByRole("row").count()).toBeGreaterThan(1);

  const audit = page.locator("#audit");
  await expect(audit.getByRole("heading", { name: "Hendelseslogg" })).toBeVisible();
  await expect(audit.getByRole("listitem").first()).toBeVisible();
  await expect(page.getByText(/Microsoft (live|tilkoblet live)/i)).toHaveCount(0);
});
