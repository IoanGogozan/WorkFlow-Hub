import { expect, test } from "@playwright/test";

test("technical reviewer can complete the detailed demo workflow", async ({ page }) => {
  await page.goto("/demo");
  await page.getByRole("button", { name: "Se automatiseringen" }).click();
  await expect(page).toHaveURL(/\/$/);

  await page.goto("/technical");
  await expect(
    page.getByRole("heading", { name: "Fra henvendelse til sak og leveranse uten dobbeltregistrering" }),
  ).toBeVisible();
  await page.goto("/integrations");
  await expect(page.getByText("Simulert – ingen ekte tilkobling")).toHaveCount(3);
  await expect(page.getByText("Simulering klar – ingen ekte tilkobling")).toHaveCount(3);
  await expect(page.getByText("Tilkoblet", { exact: true })).toHaveCount(1);
  await page.goto("/technical");
  await page.getByRole("link", { name: "Start teknisk gjennomgang" }).click();
  const firstIntakeLink = page.getByRole("link", { name: "Behandle første input" });
  await expect(firstIntakeLink).toHaveAttribute("href", /\/intakes\/[0-9a-f-]+$/i);
  await firstIntakeLink.click();
  await expect(page.getByRole("heading", { name: "Original henvendelse" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Kontroller og godkjenn" })).toBeVisible();
  await expect(page.getByText("Forslag klart for godkjenning")).toBeVisible();

  await page.getByRole("button", { name: "Godkjenn og opprett sak" }).click();
  await expect(page).toHaveURL(/\/cases\/[0-9a-f-]+$/i);
  const caseUrl = page.url();
  const caseId = caseUrl.split("/").pop()!;
  await expect(
    page.getByRole("heading", { name: /Fra godkjent input til sporbar sak/ }),
  ).toBeVisible();

  await page.goto("/documents");
  await page.getByRole("button", { name: "Opprett demo-dokument" }).click();
  const sampleDocumentLink = page
    .locator("article")
    .filter({ hasText: "Demo inspection report" })
    .first()
    .getByRole("link", { name: "Åpne dokument" });
  await expect(sampleDocumentLink).toHaveAttribute("href", /\/documents\/[0-9a-f-]+$/i);
  await page.goto((await sampleDocumentLink.getAttribute("href"))!);
  await expect(page).toHaveURL(/\/documents\/[0-9a-f-]+$/i);
  const documentTitle = (await page.getByRole("heading", { level: 2 }).textContent())!;

  await expect(
    page.getByRole("heading", { name: "Valgfri AI-klassifisering" }),
  ).toBeVisible();
  await page.getByRole("button", { name: "Kjør AI-klassifisering" }).click();
  await expect(page.getByRole("heading", { name: "Forslag med kontroll" })).toBeVisible();
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
  await expect(
    page.getByRole("heading", { name: "Dokumenter i leveransen" }),
  ).toBeVisible();
  await expect(page.getByText(documentTitle)).toBeVisible();

  await page.goto("/summary");
  await expect(page).toHaveURL(/\/#resultat$/);
  await expect(
    page.getByRole("heading", {
      name: "Fra e-post til opprettet sak – uten dobbeltregistrering",
    }),
  ).toBeVisible();
});
