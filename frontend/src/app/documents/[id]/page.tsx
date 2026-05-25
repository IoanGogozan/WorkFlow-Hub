"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { AppShell } from "@/components/app-shell";
import {
  DemoCapabilityBadge,
  DemoCapabilityNote,
} from "@/components/demo-capability-badge";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
import { api } from "@/lib/api";
import { documentAiCapability } from "@/lib/demo-capabilities";
import { formatDate, formatDateTime } from "@/lib/format";
import type {
  CaseListItem,
  DocumentClassification,
  DocumentRecord,
} from "@/lib/types";

type ClassificationForm = {
  documentType: string;
  expiryDate: string;
};

export default function DocumentDetailPage() {
  const params = useParams<{ id: string }>();
  const documentId = params.id;
  const [document, setDocument] = useState<DocumentRecord | null>(null);
  const [cases, setCases] = useState<CaseListItem[]>([]);
  const [classification, setClassification] =
    useState<DocumentClassification | null>(null);
  const [classificationForm, setClassificationForm] =
    useState<ClassificationForm | null>(null);
  const [selectedCaseId, setSelectedCaseId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [action, setAction] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadData() {
      try {
        setError(null);
        const [documentResponse, caseItems] = await Promise.all([
          api<DocumentRecord>(`/api/documents/${documentId}`, {
            signal: controller.signal,
          }),
          api<CaseListItem[]>("/api/cases", { signal: controller.signal }),
        ]);
        setDocument(documentResponse);
        setCases(caseItems);
        setSelectedCaseId(documentResponse.caseId ?? caseItems[0]?.id ?? "");
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Document could not be loaded.",
          );
        }
      }
    }

    void loadData();
    return () => controller.abort();
  }, [documentId]);

  async function refreshDocument() {
    setDocument(await api<DocumentRecord>(`/api/documents/${documentId}`));
  }

  async function analyzeDocument() {
    try {
      setAction("analyze");
      setActionError(null);
      const result = await api<DocumentClassification>(
        `/api/documents/${documentId}/analyze`,
        { method: "POST" },
      );
      setClassification(result);
      setClassificationForm({
        documentType: result.documentType,
        expiryDate: result.expiryDate ?? "",
      });
      await refreshDocument();
    } catch (analyzeError) {
      setActionError(
        analyzeError instanceof Error
          ? analyzeError.message
          : "Document analysis could not be started.",
      );
    } finally {
      setAction(null);
    }
  }

  async function approveClassification(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!classification || !classificationForm) {
      return;
    }

    try {
      setAction("approve");
      setActionError(null);
      const approved = await api<DocumentRecord>(
        `/api/documents/${documentId}/approve-classification`,
        {
          method: "POST",
          body: {
            aiAnalysisRunId: classification.aiAnalysisRunId,
            documentType: classificationForm.documentType,
            expiryDate: classificationForm.expiryDate || null,
          },
        },
      );
      setDocument(approved);
    } catch (approveError) {
      setActionError(
        approveError instanceof Error
          ? approveError.message
          : "Classification could not be approved.",
      );
    } finally {
      setAction(null);
    }
  }

  async function linkToCase(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedCaseId) {
      setActionError("Choose a case before linking this document.");
      return;
    }

    try {
      setAction("link");
      setActionError(null);
      const linked = await api<DocumentRecord>(
        `/api/documents/${documentId}/link-to-case`,
        {
          method: "POST",
          body: { caseId: selectedCaseId },
        },
      );
      setDocument(linked);
    } catch (linkError) {
      setActionError(
        linkError instanceof Error
          ? linkError.message
          : "Document could not be linked to the case.",
      );
    } finally {
      setAction(null);
    }
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-6xl px-6 py-6">
        {error ? (
          <ErrorState message={error} />
        ) : !document ? (
          <LoadingState label="Loading document" />
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
            <section className="space-y-6">
              <div>
                <Link
                  className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                  href="/documents"
                >
                  Tilbake til dokumenter
                </Link>
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <h2 className="text-3xl font-semibold">{document.title}</h2>
                  <StatusBadge status={document.status} />
                </div>
                <p className="mt-2 text-sm text-[#64748b]">
                  Opprettet {formatDateTime(document.createdAt)}
                </p>
                <div className="mt-5">
                  <WorkflowProgress activeStep={6} />
                </div>
              </div>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Dokumentstruktur</h3>
                <dl className="mt-4 grid gap-4 sm:grid-cols-2">
                  <FieldValue label="Dokumenttype" value={document.documentType} />
                  <FieldValue
                    label="Versjon"
                    value={document.currentVersionId}
                  />
                  <FieldValue label="Koblet sak" value={document.caseId} />
                  <FieldValue
                    label="Utløpsdato"
                    value={formatDate(document.expiryDate)}
                  />
                </dl>
              </section>

              {classification ? (
                <section className="rounded-md border border-[#d8deea] bg-white p-6">
                  <h3 className="text-lg font-semibold">AI-forslag</h3>
                  <p className="mt-2 text-sm text-[#64748b]">
                    Sikkerhet: {Math.round(classification.confidence * 100)}%
                  </p>
                  <p className="mt-3 text-sm leading-6 text-[#475569]">
                    {classification.summary}
                  </p>
                </section>
              ) : null}
            </section>

            <aside className="space-y-6">
              {actionError ? <ErrorState message={actionError} /> : null}

              <section className="rounded-md border border-[#d8deea] bg-white p-5">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="text-lg font-semibold">AI-klassifisering</h3>
                  <DemoCapabilityBadge capability={documentAiCapability} />
                </div>
                <p className="mt-2 text-sm leading-6 text-[#64748b]">
                  La AI foreslå dokumenttype og utløpsdato. Mennesket
                  godkjenner før dokumentet brukes i leveranse.
                </p>
                <div className="mt-3">
                  <DemoCapabilityNote capability={documentAiCapability} />
                </div>
                <button
                  className="mt-4 rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={action !== null}
                  onClick={analyzeDocument}
                  type="button"
                >
                  {action === "analyze" ? "Analyserer..." : "Kjør AI-klassifisering"}
                </button>
              </section>

              {classificationForm ? (
                <form
                  className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                  onSubmit={approveClassification}
                >
                  <h3 className="text-lg font-semibold">Godkjenn klassifisering</h3>
                  <label className="block">
                    <span className="mb-1 block text-sm font-semibold text-[#334155]">
                      Dokumenttype
                    </span>
                    <input
                      className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                      maxLength={120}
                      onChange={(event) =>
                        setClassificationForm((current) =>
                          current
                            ? { ...current, documentType: event.target.value }
                            : current,
                        )
                      }
                      required
                      value={classificationForm.documentType}
                    />
                  </label>
                  <label className="block">
                    <span className="mb-1 block text-sm font-semibold text-[#334155]">
                      Utløpsdato
                    </span>
                    <input
                      className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                      onChange={(event) =>
                        setClassificationForm((current) =>
                          current
                            ? { ...current, expiryDate: event.target.value }
                            : current,
                        )
                      }
                      type="date"
                      value={classificationForm.expiryDate}
                    />
                  </label>
                  <button
                    className="rounded-md bg-[#047857] px-4 py-2 text-sm font-semibold text-white hover:bg-[#065f46] disabled:cursor-not-allowed disabled:opacity-60"
                    disabled={action !== null}
                    type="submit"
                  >
                    {action === "approve"
                      ? "Godkjenner..."
                      : "Godkjenn klassifisering"}
                  </button>
                </form>
              ) : null}

              <form
                className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                onSubmit={linkToCase}
              >
                <h3 className="text-lg font-semibold">Koble til sak</h3>
                {cases.length === 0 ? (
                  <EmptyState
                    message="Godkjenn et input og opprett sak før dokumentet kobles."
                    title="Ingen saker tilgjengelig"
                  />
                ) : (
                  <>
                    <label className="block">
                      <span className="mb-1 block text-sm font-semibold text-[#334155]">
                        Sak
                      </span>
                      <select
                        className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                        onChange={(event) => setSelectedCaseId(event.target.value)}
                        value={selectedCaseId}
                      >
                        {cases.map((caseItem) => (
                          <option key={caseItem.id} value={caseItem.id}>
                            {caseItem.caseNumber} - {caseItem.title}
                          </option>
                        ))}
                      </select>
                    </label>
                    <button
                      className="rounded-md bg-[#162033] px-4 py-2 text-sm font-semibold text-white hover:bg-[#334155] disabled:cursor-not-allowed disabled:opacity-60"
                      disabled={action !== null}
                      type="submit"
                    >
                      {action === "link" ? "Kobler..." : "Koble dokument"}
                    </button>
                  </>
                )}
              </form>
            </aside>
          </div>
        )}
      </div>
    </AppShell>
  );
}

function FieldValue({ label, value }: { label: string; value: string | null }) {
  return (
    <div>
      <dt className="text-sm font-semibold text-[#334155]">{label}</dt>
      <dd className="mt-1 break-all text-sm text-[#64748b]">
        {value ?? "Not set"}
      </dd>
    </div>
  );
}
