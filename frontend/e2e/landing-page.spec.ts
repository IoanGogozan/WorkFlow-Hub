import { expect, test } from "@playwright/test";

test("portfolio landing page explains the product and opens the demo", async ({ page }) => {
  await page.goto("/");

  await expect(
    page.getByRole("heading", { name: "One request. Every handoff made visible." }),
  ).toBeVisible();
  await expect(page.getByText("Verifiable integration demo", { exact: true })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Built to be exercised, not just described." })).toBeVisible();
  await expect(page.getByRole("heading", { name: "A portfolio sandbox, not a production platform." })).toBeVisible();

  await page.getByRole("link", { name: /Run the workflow/ }).click();
  await expect(page).toHaveURL(/\/demo$/);
  await expect(
    page.getByRole("heading", { name: /Se hvordan .* manuell serviceflyt kan automatiseres/ }),
  ).toBeVisible();
});

test("landing page has no horizontal overflow at portfolio breakpoints", async ({ page }) => {
  for (const width of [375, 768, 1280]) {
    await page.setViewportSize({ width, height: 900 });
    await page.goto("/");
    expect(await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth)).toBe(false);
  }
});
