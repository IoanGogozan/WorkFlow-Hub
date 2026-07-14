"use client";

import { useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { AuditEvidenceTimeline } from "@/components/live-demo-evidence/audit-evidence-timeline";
import { BrregEvidenceCard } from "@/components/live-demo-evidence/brreg-evidence-card";
import { CaseEvidenceCard } from "@/components/live-demo-evidence/case-evidence-card";
import { DocumentEvidenceCard } from "@/components/live-demo-evidence/document-evidence-card";
import { ErrorState } from "@/components/error-state";
import { EvidenceOverview } from "@/components/live-demo-evidence/evidence-overview";
import { ErpReceiverEvidence } from "@/components/live-demo-evidence/erp-receiver-evidence";
import { RequestEvidenceCard } from "@/components/live-demo-evidence/request-evidence-card";
import { SharePointSimulatorEvidence } from "@/components/live-demo-evidence/sharepoint-simulator-evidence";
import { LoadingState } from "@/components/loading-state";
import {
  getLiveDemoEvidence,
  type LiveDemoEvidence,
} from "@/lib/live-demo-evidence";
import {
  clearDemoSession,
  getDemoSessionExpiresAt,
  getDemoSessionToken,
  redirectToDemoStart,
} from "@/lib/demo-session";

type LiveDemoEvidencePageProps = {
  runId: string;
};

export function LiveDemoEvidencePage({ runId }: LiveDemoEvidencePageProps) {
  const [evidence, setEvidence] = useState<LiveDemoEvidence | null>(null);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!hasActiveDemoSession()) {
      return;
    }

    const controller = new AbortController();
    getLiveDemoEvidence(runId, controller.signal)
      .then(setEvidence)
      .catch((loadError: unknown) => {
        if (!(loadError instanceof DOMException && loadError.name === "AbortError")) {
          setError(true);
        }
      });
    return () => controller.abort();
  }, [runId]);

  return (
    <AppShell>
      <div className="mx-auto max-w-6xl px-6 py-10">
        <div>
          {!evidence && !error ? <LoadingState label="Laster kjøringsbevis" /> : null}
          {error ? (
            <ErrorState
              message="Start en ny demo eller gå tilbake til live-demoen og prøv igjen."
              title="Kjøringsbeviset kunne ikke lastes"
            />
          ) : null}
          {evidence ? (
            <>
              <EvidenceOverview run={evidence.run} />
              <section aria-label="Henvendelse og Brreg-bevis" className="mt-8 grid gap-6 lg:grid-cols-2">
                <RequestEvidenceCard request={evidence.request} />
                <BrregEvidenceCard brreg={evidence.brreg} />
              </section>
              <section aria-label="Opprettet sak og dokument" className="mt-8 grid gap-6 lg:grid-cols-2">
                <CaseEvidenceCard caseEvidence={evidence.case} />
                <DocumentEvidenceCard documentEvidence={evidence.document} />
              </section>
              <div className="mt-8">
                <SharePointSimulatorEvidence evidence={evidence.sharePoint} />
              </div>
              {evidence.erp ? (
                <div className="mt-8">
                  <ErpReceiverEvidence evidence={evidence.erp} />
                </div>
              ) : null}
              <div className="mt-8">
                <AuditEvidenceTimeline events={evidence.auditEvents} />
              </div>
            </>
          ) : null}
        </div>
      </div>
    </AppShell>
  );
}

function hasActiveDemoSession() {
  const token = getDemoSessionToken();
  if (!token) {
    redirectToDemoStart("missing");
    return false;
  }

  const expiresAt = getDemoSessionExpiresAt();
  if (expiresAt && Date.parse(expiresAt) <= Date.now()) {
    clearDemoSession();
    redirectToDemoStart("expired");
    return false;
  }

  return true;
}
