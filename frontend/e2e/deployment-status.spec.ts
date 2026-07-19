import { expect, test } from "@playwright/test";

test("technical page links the live deployment to its exact revision", async ({ page }) => {
  await page.route("**/health/version", (route) => route.fulfill({
    json: {
      commit: "1da060937a02a21e2f9c61c4c99774699e2b4b38",
      builtAt: "2026-07-19T11:44:47Z",
      environment: "Demo",
      deploymentTarget: "HomeServer",
    },
  }));

  await page.goto("/technical");

  await expect(page.getByRole("heading", { name: "Verifiserbar versjon" })).toBeVisible();
  await expect(page.getByText("Ready", { exact: true })).toBeVisible();
  await expect(page.getByText("HomeServer", { exact: true })).toBeVisible();
  await expect(page.getByText("Demo", { exact: true })).toBeVisible();
  await expect(page.getByRole("link", { name: "1da0609" })).toHaveAttribute(
    "href",
    "https://github.com/IoanGogozan/WorkFlow-Hub/commit/1da060937a02a21e2f9c61c4c99774699e2b4b38",
  );
});

test("technical page remains useful when version information is unavailable", async ({ page }) => {
  await page.route("**/health/version", (route) => route.fulfill({ status: 503 }));

  await page.goto("/technical");

  await expect(page.getByText("Unavailable", { exact: true })).toBeVisible();
  await expect(page.getByText(/Dette påvirker ikke selve demoen/)).toBeVisible();
});
