import { expect, test } from "@playwright/test";

test("visitor can complete the automated public demo workflow", async ({ page }) => {
  await page.goto("/demo");
  await page.getByRole("button", { name: /Start demo/ }).click();

  await expect(page.getByRole("heading", { name: "Fra input til leveranse" })).toBeVisible();
  await expect(page.getByText("Public demo - fiktive data")).toBeVisible();

  await page.goto("/intakes");
  await page.getByRole("link", { name: "Behandle første input" }).click();

  await expect(page.getByRole("heading", { name: "Input slik det kom inn" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Kontroller og godkjenn" })).toBeVisible();
  await expect(page.getByText("Forslag klart for godkjenning")).toBeVisible();

  await page.getByRole("button", { name: "Godkjenn og opprett sak" }).click();
  await expect(page).toHaveURL(/\/cases\/[0-9a-f-]+$/i);
  const caseUrl = page.url();
  const caseId = caseUrl.split("/").pop()!;
  await expect(page.getByRole("heading", { name: /Fra godkjent input til sporbar sak/ })).toBeVisible();

  await page.goto("/documents");
  await page.getByRole("button", { name: "Opprett demo-dokument" }).click();
  await page
    .locator("article")
    .filter({ hasText: "Uploaded" })
    .first()
    .getByRole("link", { name: "Åpne dokument" })
    .click();
  await expect(page).toHaveURL(/\/documents\/[0-9a-f-]+$/i);
  const documentTitle = (await page.getByRole("heading", { level: 2 }).textContent())!;

  await expect(page.getByRole("heading", { name: "AI-klassifisering" })).toBeVisible();
  await page.getByRole("button", { name: "Kjør AI-klassifisering" }).click();
  await expect(page.getByRole("heading", { name: "AI-forslag" })).toBeVisible();
  await page.getByRole("button", { name: "Godkjenn klassifisering" }).click();
  await page.getByRole("combobox", { name: "Sak" }).selectOption(caseId);
  await page.getByRole("button", { name: "Koble dokument" }).click();

  await page.goto(caseUrl);
  await expect(page.getByRole("heading", { name: "Dokumenter på saken" })).toBeVisible();
  await expect(page.getByRole("link", { name: documentTitle })).toBeVisible();

  await page.getByLabel("Tittel på leveringspakke").fill("E2E leveringspakke");
  await page.getByLabel(new RegExp(documentTitle)).check();
  await page.getByRole("button", { name: "Lag leveringspakke" }).click();

  await expect(page).toHaveURL(/\/delivery-packages\/[0-9a-f-]+$/i);
  await expect(page.getByRole("heading", { name: "E2E leveringspakke" })).toBeVisible();
  await page.getByRole("button", { name: "Generer PDF" }).click();
  await expect(page.getByText("Generert")).toBeVisible();
  await page.getByLabel("Mottaker e-post").fill("kunde@example.test");
  await page.getByRole("button", { name: "Lag kundelenke" }).click();

  const publicLink = page.getByRole("link", { name: /\/delivery\// }).last();
  await expect(publicLink).toBeVisible();
  const publicHref = await publicLink.getAttribute("href");
  expect(publicHref).toBeTruthy();

  await page.goto(publicHref!);
  await expect(page.getByRole("heading", { name: "Delivery documents" })).toBeVisible();
  await expect(page.getByText(documentTitle)).toBeVisible();

  await page.goto("/summary");
  await expect(page.getByRole("heading", { name: "Du har fullført en integrert arbeidsflyt" })).toBeVisible();
});
