import { expect, test } from "@playwright/test";

test("visitor can verify the exact artifacts created by a live run", async ({ page }) => {
  await page.goto("/demo");
  await page.getByRole("button", { name: "Se automatiseringen" }).click();
  await expect(page).toHaveURL(/\/$/);

  await page.getByRole("button", { name: "Kjør live demo" }).click();
  await expect(page.getByRole("heading", { name: /Fullført på/ })).toBeVisible({ timeout: 30_000 });

  const caseResult = page
    .getByRole("article")
    .filter({ hasText: "Sak" })
    .filter({ hasText: "LIVE-" })
    .first();
  const caseNumber = (await caseResult.textContent())?.match(/LIVE-[0-9]{4}-[A-F0-9]+/)?.[0];
  expect(caseNumber).toBeTruthy();

  await expect(page.getByRole("link", { name: "Se simulatorbevis" })).toHaveAttribute(
    "href",
    /\/technical\/live-runs\/[0-9a-f-]+#sharepoint$/i,
  );
  await expect(page.getByRole("link", { name: "Se hendelseslogg" })).toHaveAttribute(
    "href",
    /\/technical\/live-runs\/[0-9a-f-]+#audit$/i,
  );

  const deliveryLink = page.getByRole("link", { name: "Åpne leveringspakken" });
  await expect(deliveryLink).toHaveAttribute("href", /^\/delivery-packages\/[0-9a-f-]+$/i);
  const deliveryHref = await deliveryLink.getAttribute("href");
  const deliveryStatus = await page.evaluate(async (href) => {
    const response = await fetch(href!);
    return response.status;
  }, deliveryHref);
  expect(deliveryStatus).toBe(200);

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
  await documentCard.getByRole("link", { name: "Åpne dokumentdetaljer" }).click();
  await expect(page).toHaveURL(/\/documents\/[0-9a-f-]+$/i);
  await page.goBack();
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
  await page.setViewportSize({ width: 375, height: 900 });
  const operationRegion = sharePoint.getByRole("region", { name: "SharePoint simulatoroperasjoner" });
  await expect(operationRegion).toBeVisible();
  expect(await operationRegion.evaluate((element) => element.scrollWidth > element.clientWidth)).toBe(true);
  await operationRegion.focus();
  await expect(operationRegion).toBeFocused();
  expect(await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth)).toBe(false);

  const erp = page.locator("#erp");
  await expect(erp.getByRole("heading", { name: "Norvix ERP demo receiver" })).toBeVisible();
  await expect(erp.getByText("Selvhostet", { exact: true })).toBeVisible();
  await expect(erp.getByText("Melding mottatt", { exact: true })).toBeVisible();
  await expect(erp.getByText(/^ERP-DEMO-/)).toBeVisible();
  await expect(erp.getByText("1", { exact: true })).toBeVisible();
  await expect(page.getByText(/failure demo/i)).toHaveCount(0);

  const audit = page.locator("#audit");
  await expect(audit.getByRole("heading", { name: "Hendelseslogg" })).toBeVisible();
  await expect(audit.getByRole("listitem").first()).toBeVisible();
  await expect(page.getByText(/Microsoft (live|tilkoblet live)/i)).toHaveCount(0);

  await page.emulateMedia({ reducedMotion: "reduce" });
  const transitionDuration = await page.locator("body").evaluate(
    (element) => getComputedStyle(element).transitionDuration,
  );
  expect(Number.parseFloat(transitionDuration)).toBeLessThanOrEqual(0.00001);
});
