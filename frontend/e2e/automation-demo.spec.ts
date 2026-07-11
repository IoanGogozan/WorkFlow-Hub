import { expect, test } from "@playwright/test";

test("visitor understands and replays the client-facing automation story", async ({ page }) => {
  await page.goto("/demo");
  await expect(
    page.getByRole("heading", {
      name: "Se hvordan én manuell serviceflyt kan automatiseres",
    }),
  ).toBeVisible();
  await expect(page.getByText("Ingen ekte kundesystemer kontaktes")).toBeVisible();
  await page.getByRole("button", { name: "Se automatiseringen" }).click();
  await expect(page).toHaveURL(/\/$/);

  await expect(
    page.getByRole("heading", {
      name: "Fra e-post til opprettet sak – uten dobbeltregistrering",
    }),
  ).toBeVisible();
  await expect(page.getByText("Service og dokumentasjon – pumpestasjon 14")).toBeVisible();
  await expect(page.getByText("Kundereferanse: PO-10482")).toBeVisible();
  await expect(
    page.getByRole("heading", { name: "Slik gjøres det ofte manuelt" }),
  ).toBeVisible();
  await expect(page.getByText("12–20 minutter")).toBeVisible();
  await expect(page.getByText(/Faktisk tidsbruk og mulig reduksjon må måles/)).toBeVisible();

  const replayButton = page.getByRole("button", { name: "Kjør automatisert flyt" });
  await replayButton.focus();
  await expect(replayButton).toBeFocused();
  await page.keyboard.press("Enter");
  await expect(
    page.getByRole("button", { name: "Spiller av automatisert flyt" }),
  ).toBeFocused();
  await expect(page.getByText("Flyten er fullført med 8 sporbare trinn.")).toBeVisible();
  await expect(page.getByRole("heading", { name: "Sak opprettet" })).toBeVisible();
  await expect(page.getByText("Norvix demoarbeidsflyt")).toBeVisible();
  await expect(page.getByRole("heading", { name: "Leveringsgrunnlag opprettet" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Ekstern rapportering simulert" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Sporbarhet lagret" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Resultatet" })).toBeVisible();
  await expect(page.getByText("Kristiansand Kommune").last()).toBeVisible();
  await expect(page.getByText("1 kontrollpunkt")).toBeVisible();
  await expect(
    page.getByLabel("Resultatet").getByText("Sporbar hendelseslogg"),
  ).toBeVisible();

  await expect(page.getByText("30,3 timer")).toBeVisible();
  await page.getByLabel("Henvendelser per uke").fill("80");
  await expect(page.getByText("60,6 timer")).toBeVisible();
  await expect(page.getByText(/Faktisk effekt må måles i en avgrenset pilot/)).toBeVisible();
  await expect(
    page.getByText(/demonstreres uten å sende data til et ekte kundesystem/),
  ).toBeVisible();
  await expect(page.getByText("Fiktiv scenariokilde")).toBeVisible();
  await expect(page.getByText(/Outlook er ikke tilkoblet/)).toBeVisible();
  await expect(page.getByText("Simulert – ikke tilkoblet")).toHaveCount(3);

  const technicalSummary = page
    .locator("summary")
    .filter({ hasText: "Se hva som faktisk er implementert" });
  await technicalSummary.focus();
  await page.keyboard.press("Enter");
  await expect(technicalSummary).toBeFocused();
  await expect(page.getByText("ASP.NET Core / C#")).toBeVisible();
  await expect(page.getByRole("link", { name: "Åpne opprettet sak" })).toHaveAttribute(
    "href",
    /\/cases\/[0-9a-f-]+$/i,
  );
  await expect(
    page.getByRole("heading", { name: "Har dere en lignende manuell prosess?" }),
  ).toBeVisible();
  await expect(page.getByRole("link", { name: "Beskriv prosessen deres" })).toHaveAttribute(
    "href",
    /^mailto:contact@norvix\.no/,
  );

  for (const width of [375, 768, 1280]) {
    await page.setViewportSize({ width, height: 900 });
    const hasHorizontalOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth,
    );
    expect(hasHorizontalOverflow).toBe(false);
    if (process.env.CAPTURE_RESPONSIVE === "1") {
      await page.screenshot({
        fullPage: true,
        path: `test-results/responsive-${width}.png`,
      });
    }
  }

  await page.getByRole("button", { name: "Spill av på nytt" }).click();
  await expect(page.getByText(/Trinn 1 av 8:/)).toBeVisible();

  await page.goto("/automation");
  await expect(page).toHaveURL(/\/$/);
});
