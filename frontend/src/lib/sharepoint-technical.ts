import { api } from "@/lib/api";

export type SharePointTechnicalStatus = {
  mode: string;
  isSimulated: boolean;
  isConfigured: boolean;
  siteId: string;
  siteName: string;
  driveId: string;
  libraryName: string;
  permissionModel: string;
  permissionLevel: string;
  publicMessage: string;
};

export type SharePointTechnicalDocument = {
  name: string;
  parentPath: string;
  externalItemId: string;
  eTag: string;
  version: string;
  syncStatus: string;
  lastSyncedAt: string;
};

export type SharePointTechnicalOperation = {
  createdAt: string;
  httpMethod: string;
  operation: string;
  target: string;
  statusCode: number;
  succeeded: boolean;
  durationMilliseconds: number;
  errorCode: string | null;
};

export type SharePointAccessEvidence = {
  succeeded: boolean;
  statusCode: number;
  errorCode: string | null;
  publicMessage: string;
};

export async function getSharePointTechnicalEvidence() {
  const [status, tree, documents, operations] = await Promise.all([
    api<SharePointTechnicalStatus>("/api/technical/sharepoint/status"),
    api<string[]>("/api/technical/sharepoint/tree"),
    api<SharePointTechnicalDocument[]>("/api/technical/sharepoint/documents"),
    api<SharePointTechnicalOperation[]>("/api/technical/sharepoint/operations"),
  ]);
  return { status, tree, documents, operations };
}

export function testRestrictedSharePointAccess() {
  return api<SharePointAccessEvidence>("/api/technical/sharepoint/test-restricted-access", {
    method: "POST",
  });
}
