import { expect, test } from "@playwright/test";

test("visitor can review the concise live integration preview", async ({ page }) => {
  await page.goto("/live-preview");

  await expect(
    page.getByRole("heading", {
      name: "Fra henvendelse til sak og SharePoint – på sekunder",
    }),
  ).toBeVisible();
  await expect(page.getByRole("link", { name: "Kjør live demo" })).toBeVisible();
  await expect(
    page.getByRole("link", { name: "Beskriv prosessen deres" }).first(),
  ).toBeVisible();

  for (const stage of ["Mottatt", "Kontrollert", "Opprettet", "Synkronisert"]) {
    await expect(page.getByRole("heading", { name: stage })).toBeVisible();
  }

  await expect(
    page.getByRole("heading", { name: "Fullført på 8,4 sekunder" }),
  ).toBeVisible();
  await expect(page.getByText("ERP-RECEIPT-0142")).toBeVisible();

  const manualProcess = page
    .locator("details")
    .filter({ hasText: "Slik ser den manuelle prosessen ofte ut" });
  await expect(manualProcess).not.toHaveAttribute("open", "");
  const manualSummary = manualProcess.getByText(
    "Slik ser den manuelle prosessen ofte ut",
  );
  await manualSummary.focus();
  await expect(manualSummary).toBeFocused();
  await page.keyboard.press("Enter");
  await expect(manualProcess).toHaveAttribute("open", "");

  for (const width of [375, 768, 1280]) {
    await page.setViewportSize({ width, height: 900 });
    const hasHorizontalOverflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth,
    );
    expect(hasHorizontalOverflow).toBe(false);
  }
});
