import { api, apiBlob } from "@/lib/api";

export type LiveDemoEvidenceRun = {
  runId: string;
  status: string;
  correlationId: string;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  totalDurationMs: number | null;
  retryCount: number;
  scenarioLabel: string;
};

export type LiveDemoEvidenceRequest = {
  title: string;
  body: string;
  customerReference: string;
  sourceLabel: string;
  createdAt: string;
};

export type LiveDemoEvidenceBrreg = {
  mode: string;
  organizationNumber: string;
  organizationName: string;
  lookupDurationMs: number | null;
  sourceUpdatedAt: string | null;
  statusMessage: string;
};

export type LiveDemoEvidenceCase = {
  caseNumber: string;
  title: string;
  status: string;
  customerName: string;
  createdAt: string;
  caseHref: string;
};

export type LiveDemoEvidenceDocument = {
  documentId: string;
  title: string;
  fileName: string;
  sizeBytes: number;
  contentType: string;
  versionNumber: number;
  contentHash: string | null;
  createdAt: string;
  documentHref: string;
  downloadHref: string;
};

export type LiveDemoEvidenceSharePointOperation = {
  timestamp: string;
  method: string;
  action: string;
  statusCode: number;
  result: string;
  durationMs: number;
  attempt: number;
  idempotencyResult: string;
};

export type LiveDemoEvidenceSharePoint = {
  mode: string;
  siteName: string;
  libraryName: string;
  folderPath: string;
  folderId: string;
  fileId: string;
  fileName: string;
  version: number;
  eTag: string;
  metadata: Record<string, string>;
  operations: LiveDemoEvidenceSharePointOperation[];
  technicalSharePointHref: string;
};

export type LiveDemoEvidenceErp = {
  mode: string;
  status: string;
  externalReceiptId: string | null;
  idempotencyKey: string | null;
  attempts: number;
  lastDurationMs: number | null;
  safeError: string | null;
  history: LiveDemoEvidenceErpAttempt[];
};

export type LiveDemoEvidenceErpAttempt = {
  timestamp: string;
  attempt: number;
  status: string;
  durationMs: number | null;
  message: string;
};

export type LiveDemoEvidenceAuditEvent = {
  timestamp: string;
  eventType: string;
  operationLabel: string;
  entityType: string;
  result: string;
  correlationId: string;
  provider: string | null;
  durationMs: number | null;
  attempt: number | null;
};

export type LiveDemoEvidenceLinks = {
  caseHref: string | null;
  documentHref: string | null;
  downloadHref: string | null;
  deliveryPackageHref: string | null;
  sharePointTechnicalHref: string;
  integrationDashboardHref: string;
};

export type LiveDemoEvidence = {
  run: LiveDemoEvidenceRun;
  request: LiveDemoEvidenceRequest | null;
  brreg: LiveDemoEvidenceBrreg | null;
  case: LiveDemoEvidenceCase | null;
  document: LiveDemoEvidenceDocument | null;
  sharePoint: LiveDemoEvidenceSharePoint | null;
  erp: LiveDemoEvidenceErp | null;
  auditEvents: LiveDemoEvidenceAuditEvent[];
  links: LiveDemoEvidenceLinks;
};

export function getLiveDemoEvidence(runId: string, signal?: AbortSignal) {
  return api<LiveDemoEvidence>(`/api/live-demo-runs/${encodeURIComponent(runId)}/evidence`, {
    signal,
  });
}

export async function openLiveDemoPdf(downloadHref: string) {
  const pdf = await apiBlob(downloadHref);
  const objectUrl = URL.createObjectURL(pdf);
  const link = document.createElement("a");
  link.href = objectUrl;
  link.target = "_blank";
  link.rel = "noopener noreferrer";
  link.click();
  window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
}
