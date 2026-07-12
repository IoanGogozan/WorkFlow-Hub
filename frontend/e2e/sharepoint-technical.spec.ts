import { expect, test } from "@playwright/test";

test("technical SharePoint simulator shows safe evidence and restricted access", async ({ page }) => {
  await page.addInitScript(() => {
    window.sessionStorage.setItem("norvix.demoSessionToken", "e2e-token");
    window.sessionStorage.setItem("norvix.demoSessionExpiresAt", "2099-01-01T00:00:00.000Z");
  });
  await page.route("**/api/technical/sharepoint/status", (route) => route.fulfill({ json: {
    mode: "Simulated", isSimulated: true, isConfigured: true, siteId: "site-demo-service",
    siteName: "Service Operations Demo", driveId: "drive-shared-documents", libraryName: "Shared Documents",
    permissionModel: "Sites.Selected simulation", permissionLevel: "write",
    publicMessage: "Local simulator. No Microsoft 365 tenant is connected.",
  } }));
  await page.route("**/api/technical/sharepoint/tree", (route) => route.fulfill({ json: [
    "/Shared Documents/Customers/Fjord/CASE-001/Incoming",
    "/Shared Documents/Customers/Fjord/CASE-001/Approved",
    "/Shared Documents/Customers/Fjord/CASE-001/Delivery",
  ] }));
  await page.route("**/api/technical/sharepoint/documents", (route) => route.fulfill({ json: [{
    name: "service-request.pdf", parentPath: "/Shared Documents/Customers/Fjord/CASE-001/Incoming",
    externalItemId: "01SP-DEMO-ABCD", eTag: "demo-etag-1", version: "1.0",
    syncStatus: "Synchronized", lastSyncedAt: "2026-07-12T10:00:00Z",
  }] }));
  await page.route("**/api/technical/sharepoint/operations", (route) => route.fulfill({ json: [{
    createdAt: "2026-07-12T10:00:00Z", httpMethod: "PUT", operation: "UploadDocument",
    target: "/Shared Documents/Customers/Fjord/CASE-001/Incoming/service-request.pdf",
    statusCode: 201, succeeded: true, durationMilliseconds: 4, errorCode: null,
  }] }));
  await page.route("**/api/technical/sharepoint/test-restricted-access", (route) => route.fulfill({ json: {
    succeeded: false, statusCode: 403, errorCode: "accessDenied",
    publicMessage: "Access to this simulated site is denied.",
  } }));

  await page.goto("/technical/sharepoint");

  await expect(page.getByRole("heading", { name: /SharePoint \/ Microsoft Graph/ })).toBeVisible();
  await expect(page.getByText("No live Microsoft 365 tenant is connected.")).toBeVisible();
  await expect(page.getByRole("cell", { name: "service-request.pdf", exact: true })).toBeVisible();
  await expect(page.getByText(/CASE-001\/Approved/)).toBeVisible();
  await page.getByRole("button", { name: "Test restricted-site access" }).click();
  await expect(page.getByText(/Result: 403 accessDenied/)).toBeVisible();
});
