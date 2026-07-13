import { api } from "@/lib/api";

export type LiveDemoRunStep = {
  key: string;
  sequence: number;
  publicStage: string;
  provider: string;
  status: string;
  evidenceMode: string;
  attemptCount: number;
  durationMs: number | null;
  publicSummary: string | null;
  publicEvidenceReference: string | null;
  publicErrorCode: string | null;
  publicErrorMessage: string | null;
};

export type LiveDemoRunResult = {
  caseNumber: string | null;
  documentFileName: string | null;
  brregMode: string | null;
  sharePointFolderReference: string | null;
  sharePointFileReference: string | null;
  erpReceiptId: string | null;
  auditEventCount: number | null;
  evidenceHref: string;
  caseHref: string | null;
  documentHref: string | null;
  documentDownloadHref: string | null;
  sharePointEvidenceHref: string | null;
  auditHref: string;
};

export type LiveDemoRun = {
  runId: string;
  status: "Queued" | "Running" | "Completed" | "Failed";
  currentStepKey: string | null;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  totalDurationMs: number | null;
  retryCount: number;
  canRetry: boolean;
  publicErrorCode: string | null;
  publicErrorMessage: string | null;
  steps: LiveDemoRunStep[];
  result: LiveDemoRunResult | null;
};

export type LiveDemoCapabilities = {
  enabled: boolean;
  brregLiveEnabled: boolean;
  sharePointEnabled: boolean;
  erpReceiverEnabled: boolean;
  failureDemoEnabled: boolean;
};

type CreateLiveDemoRunResponse = {
  runId: string;
};

export async function createLiveDemoRun(signal?: AbortSignal) {
  return api<CreateLiveDemoRunResponse>("/api/live-demo-runs", {
    method: "POST",
    body: { simulateErpFailureOnce: false },
    signal,
  });
}

export async function getLiveDemoRun(runId: string, signal?: AbortSignal) {
  return api<LiveDemoRun>(`/api/live-demo-runs/${runId}`, { signal });
}

export async function retryLiveDemoRun(runId: string, signal?: AbortSignal) {
  return api<{ runId: string }>(`/api/live-demo-runs/${runId}/retry`, {
    method: "POST",
    signal,
  });
}

export async function getLiveDemoCapabilities(signal?: AbortSignal) {
  return api<LiveDemoCapabilities>("/api/live-demo-capabilities", { signal });
}
