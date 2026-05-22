import { expect, test } from "@playwright/test";

test("visitor can complete the public demo workflow", async ({ page }) => {
  await page.goto("/demo");
  await page.getByRole("button", { name: "Start demo workspace" }).click();
  await expect(page.getByRole("heading", { name: "Operational dashboard" })).toBeVisible();
  await expect(page.getByText("Public demo - fictional data - expires automatically")).toBeVisible();

  await page.getByRole("link", { name: "New intake" }).first().click();
  await page.getByLabel("Subject").fill(`E2E demo request ${Date.now()}`);
  await page.getByLabel("Body").fill("A public demo visitor needs an inspection case, approved document, and delivery package.");
  await page.getByLabel("Customer name").fill("Kristiansand Kommune");
  await page.getByLabel("Organization number").fill("963296746");
  await page.getByLabel("Category").fill("Inspection");
  await page.getByLabel("Urgency").selectOption("Normal");
  await page.getByRole("button", { name: "Create intake" }).click();

  await expect(page.getByRole("heading", { name: /E2E demo request/ })).toBeVisible();
  await page.getByRole("button", { name: "Analyze with AI" }).click();
  await expect(page.getByRole("heading", { name: "AI suggestion" })).toBeVisible();
  await page.getByRole("button", { name: "Approve suggestion" }).click();
  await expect(page.getByRole("button", { name: "Convert to case" })).toBeEnabled();
  await page.getByRole("button", { name: "Convert to case" }).click();

  await expect(page).toHaveURL(/\/cases\/[0-9a-f-]+$/i);
  const caseUrl = page.url();
  await expect(page.getByRole("heading", { name: /E2E demo request/ })).toBeVisible();

  await page.getByRole("link", { name: "Documents" }).click();
  await page.getByRole("button", { name: "Use sample document" }).click();
  await expect(page.getByRole("link", { name: "Demo inspection report" })).toBeVisible();
  await page.getByRole("link", { name: "Demo inspection report" }).first().click();

  await expect(page.getByRole("heading", { name: "Classification" })).toBeVisible();
  await page.getByRole("button", { name: "Analyze document" }).click();
  await expect(page.getByRole("heading", { name: "Approve classification" })).toBeVisible();
  await page.getByRole("button", { name: "Approve classification" }).click();
  await expect(page.getByText("Approved")).toBeVisible();
  await Promise.all([
    page.waitForResponse((response) =>
      response.url().includes("/link-to-case") && response.ok(),
    ),
    page.getByRole("button", { name: "Link document" }).click(),
  ]);

  await page.goto(caseUrl);
  await expect(page.getByRole("heading", { name: "Linked documents" })).toBeVisible();
  await expect(page.getByRole("link", { name: "Demo inspection report" })).toBeVisible();
  await page.getByLabel("Package title").fill("E2E delivery package");
  await page.getByLabel("Demo inspection report").check();
  await page.getByRole("button", { name: "Create package" }).click();

  await expect(page).toHaveURL(/\/delivery-packages\/[0-9a-f-]+$/i);
  await expect(page.getByRole("heading", { name: "E2E delivery package" })).toBeVisible();
  await page.getByRole("button", { name: "Generate PDF summary" }).click();
  await expect(page.getByText(/Generated/)).toBeVisible();
  await page.getByLabel("Recipient email").fill("recipient@example.test");
  await page.getByRole("button", { name: "Create public link" }).click();
  const publicLink = page.getByRole("link", { name: /\/delivery\// }).last();
  await expect(publicLink).toBeVisible();
  const publicHref = await publicLink.getAttribute("href");
  expect(publicHref).toBeTruthy();

  await page.goto(publicHref!);
  await expect(page.getByRole("heading", { name: "E2E delivery package" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Delivery documents" })).toBeVisible();
  await expect(page.getByText("Demo inspection report")).toBeVisible();

  await page.goto(caseUrl);
  await expect(page.getByText("Public delivery page opened")).toBeVisible();
  await expect(page.getByText("Delivery PDF generated")).toBeVisible();
  await expect(page.getByText("Public delivery link created")).toBeVisible();
});
