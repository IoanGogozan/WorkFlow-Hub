"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { AppShell } from "@/components/app-shell";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
import { WhyThisMatters } from "@/components/why-this-matters";
import { api } from "@/lib/api";
import { formatDate, formatDateTime } from "@/lib/format";
import type {
  CaseActivity,
  CaseDetail,
  CaseNote,
  CaseTask,
  DeliveryPackage,
  DocumentRecord,
} from "@/lib/types";

export default function CaseDetailPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const caseId = params.id;
  const [caseDetail, setCaseDetail] = useState<CaseDetail | null>(null);
  const [activity, setActivity] = useState<CaseActivity[] | null>(null);
  const [documents, setDocuments] = useState<DocumentRecord[]>([]);
  const [selectedDocumentIds, setSelectedDocumentIds] = useState<string[]>([]);
  const [deliveryTitle, setDeliveryTitle] = useState("");
  const [taskTitle, setTaskTitle] = useState("");
  const [taskDescription, setTaskDescription] = useState("");
  const [noteBody, setNoteBody] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadCase() {
      try {
        setError(null);
        const [detail, activityItems] = await Promise.all([
          api<CaseDetail>(`/api/cases/${caseId}`, {
            signal: controller.signal,
          }),
          api<CaseActivity[]>(`/api/cases/${caseId}/activity`, {
            signal: controller.signal,
          }),
        ]);
        const documentItems = await api<DocumentRecord[]>("/api/documents", {
          signal: controller.signal,
        });
        const linkedDocuments = documentItems.filter(
          (document) => document.caseId === caseId,
        );
        setCaseDetail(detail);
        setActivity(activityItems);
        setDocuments(linkedDocuments);
        setSelectedDocumentIds(
          linkedDocuments
            .filter((document) => document.status.toLowerCase() === "approved")
            .map((document) => document.id),
        );
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Saken kunne ikke lastes.",
          );
        }
      }
    }

    void loadCase();
    return () => controller.abort();
  }, [caseId]);

  async function refreshActivity() {
    setActivity(await api<CaseActivity[]>(`/api/cases/${caseId}/activity`));
  }

  async function addTask(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    try {
      setSubmitting("task");
      setActionError(null);
      await api<CaseTask>(`/api/cases/${caseId}/tasks`, {
        method: "POST",
        body: {
          title: taskTitle,
          description: taskDescription,
          dueDate: null,
        },
      });
      setTaskTitle("");
      setTaskDescription("");
      await refreshActivity();
    } catch (taskError) {
      setActionError(
        taskError instanceof Error
          ? taskError.message
          : "Oppgaven kunne ikke legges til.",
      );
    } finally {
      setSubmitting(null);
    }
  }

  async function addNote(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    try {
      setSubmitting("note");
      setActionError(null);
      await api<CaseNote>(`/api/cases/${caseId}/notes`, {
        method: "POST",
        body: {
          body: noteBody,
          visibility: "Internal",
        },
      });
      setNoteBody("");
      await refreshActivity();
    } catch (noteError) {
      setActionError(
        noteError instanceof Error
          ? noteError.message
          : "Notatet kunne ikke legges til.",
      );
    } finally {
      setSubmitting(null);
    }
  }

  async function createDeliveryPackage(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (selectedDocumentIds.length === 0) {
      setActionError("Velg minst ett godkjent dokument for levering.");
      return;
    }

    try {
      setSubmitting("delivery");
      setActionError(null);
      const created = await api<DeliveryPackage>(
        `/api/cases/${caseId}/delivery-packages`,
        {
          method: "POST",
          body: {
            title: deliveryTitle,
            documentIds: selectedDocumentIds,
          },
        },
      );
      router.push(`/delivery-packages/${created.id}`);
    } catch (deliveryError) {
      setActionError(
        deliveryError instanceof Error
          ? deliveryError.message
          : "Leveringspakken kunne ikke opprettes.",
      );
    } finally {
      setSubmitting(null);
    }
  }

  function toggleDocument(documentId: string) {
    setSelectedDocumentIds((current) =>
      current.includes(documentId)
        ? current.filter((id) => id !== documentId)
        : [...current, documentId],
    );
  }

  const approvedDocuments = documents.filter(
    (document) => document.status.toLowerCase() === "approved",
  );
  const latestActivity = activity?.slice(0, 5) ?? [];
  const firstDocument = documents[0];
  const firstPendingDocument = documents.find(
    (document) => document.status.toLowerCase() !== "approved",
  );

  return (
    <AppShell>
      <div className="mx-auto max-w-6xl px-6 py-6">
        {error ? (
          <ErrorState message={error} />
        ) : !caseDetail || !activity ? (
          <LoadingState label="Laster sak" />
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
            <section className="space-y-6">
              <div>
                <Link
                  className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                  href="/cases"
                >
                  Tilbake til saker
                </Link>
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <h2 className="text-3xl font-semibold">{caseDetail.title}</h2>
                  <StatusBadge status={caseDetail.status} />
                </div>
                <p className="mt-2 text-sm text-[#64748b]">
                  {caseDetail.caseNumber} - opprettet{" "}
                  {formatDateTime(caseDetail.createdAt)}
                </p>
                <div className="mt-5">
                  <WorkflowProgress activeStep={3} />
                </div>
              </div>

              <WhyThisMatters title="Fra godkjent input til sporbar sak">
                <p>
                  Når input er godkjent, opprettes en sak med ansvar, status,
                  dokumenter, oppgaver og historikk.
                </p>
                <p>
                  Dette erstatter manuell oppfølging i e-post, Excel og mapper.
                </p>
              </WhyThisMatters>

              <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                <ProcessMetric
                  detail={caseDetail.caseNumber}
                  label="Sak"
                  value={caseDetail.status}
                />
                <ProcessMetric
                  detail="koblet til saken"
                  label="Dokumenter"
                  value={`${documents.length}`}
                />
                <ProcessMetric
                  detail="klar for levering"
                  label="Godkjent"
                  value={`${approvedDocuments.length}`}
                />
                <ProcessMetric
                  detail="kan settes senere"
                  label="Frist"
                  value={formatDate(caseDetail.dueDate) ?? "Ikke satt"}
                />
              </section>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Hva handler saken om?</h3>
                <p className="mt-3 whitespace-pre-wrap text-sm leading-6 text-[#475569]">
                  {caseDetail.description ?? "Ingen beskrivelse."}
                </p>
              </section>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h3 className="text-lg font-semibold">Dokumenter på saken</h3>
                    <p className="mt-1 text-sm leading-6 text-[#64748b]">
                      Dokumenter må være godkjent før de kan sendes videre.
                    </p>
                  </div>
                  <Link
                    className="inline-flex w-fit rounded-md border border-[#bfdbfe] px-3 py-2 text-sm font-semibold text-[#2563eb] hover:bg-[#eff6ff]"
                    href="/documents"
                  >
                    Gå til dokumenter
                  </Link>
                </div>
                {documents.length === 0 ? (
                  <div className="mt-4">
                    <EmptyState
                      action={
                        <Link
                          className="inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
                          href="/documents"
                        >
                          Opprett dokument
                        </Link>
                      }
                      message="Opprett eller åpne et demo-dokument, godkjenn klassifisering og koble det til denne saken."
                      title="Ingen dokumenter koblet ennå"
                    />
                  </div>
                ) : (
                  <div className="mt-4 divide-y divide-[#e2e8f0]">
                    {documents.map((document) => (
                      <div
                        className="flex flex-col gap-2 py-3 sm:flex-row sm:items-center sm:justify-between"
                        key={document.id}
                      >
                        <div>
                          <Link
                            className="font-medium text-[#162033] hover:text-[#2563eb]"
                            href={`/documents/${document.id}`}
                          >
                            {document.title}
                          </Link>
                          <p className="mt-1 text-sm text-[#64748b]">
                            {document.documentType ?? "Ikke klassifisert"} -{" "}
                            {formatDate(document.expiryDate)}
                          </p>
                        </div>
                        <StatusBadge status={document.status} />
                      </div>
                    ))}
                  </div>
                )}
              </section>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Siste aktivitet</h3>
                {latestActivity.length === 0 ? (
                  <div className="mt-4">
                    <EmptyState
                      message="Ingen aktivitet er registrert på saken ennå."
                      title="Ingen aktivitet"
                    />
                  </div>
                ) : (
                  <div className="mt-4 divide-y divide-[#e2e8f0]">
                    {latestActivity.map((item) => (
                      <div className="py-3" key={item.id}>
                        <p className="font-medium text-[#162033]">
                          {activityLabel(item.action)}
                        </p>
                        <p className="mt-1 text-sm text-[#64748b]">
                          {entityLabel(item.entityType)} -{" "}
                          {formatDateTime(item.createdAt)}
                        </p>
                      </div>
                    ))}
                  </div>
                )}
              </section>

              <details className="rounded-md border border-[#d8deea] bg-white p-6">
                <summary className="cursor-pointer text-lg font-semibold">
                  Interne notater og oppgaver
                </summary>
                <div className="mt-5 grid gap-5 lg:grid-cols-2">
                  <form className="space-y-4" onSubmit={addTask}>
                    <h4 className="font-semibold">Legg til oppgave</h4>
                    <label className="block">
                      <span className="mb-1 block text-sm font-semibold text-[#334155]">
                        Tittel
                      </span>
                      <input
                        className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                        maxLength={240}
                        onChange={(event) => setTaskTitle(event.target.value)}
                        required
                        value={taskTitle}
                      />
                    </label>
                    <label className="block">
                      <span className="mb-1 block text-sm font-semibold text-[#334155]">
                        Beskrivelse
                      </span>
                      <textarea
                        className="min-h-24 w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                        onChange={(event) =>
                          setTaskDescription(event.target.value)
                        }
                        value={taskDescription}
                      />
                    </label>
                    <button
                      className="rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
                      disabled={submitting !== null}
                      type="submit"
                    >
                      {submitting === "task"
                        ? "Legger til..."
                        : "Legg til oppgave"}
                    </button>
                  </form>

                  <form className="space-y-4" onSubmit={addNote}>
                    <h4 className="font-semibold">Legg til notat</h4>
                    <label className="block">
                      <span className="mb-1 block text-sm font-semibold text-[#334155]">
                        Notat
                      </span>
                      <textarea
                        className="min-h-28 w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                        maxLength={4000}
                        onChange={(event) => setNoteBody(event.target.value)}
                        required
                        value={noteBody}
                      />
                    </label>
                    <button
                      className="rounded-md bg-[#047857] px-4 py-2 text-sm font-semibold text-white hover:bg-[#065f46] disabled:cursor-not-allowed disabled:opacity-60"
                      disabled={submitting !== null}
                      type="submit"
                    >
                      {submitting === "note"
                        ? "Legger til..."
                        : "Legg til notat"}
                    </button>
                  </form>
                </div>
              </details>
            </section>

            <aside className="space-y-6">
              {actionError ? <ErrorState message={actionError} /> : null}

              <form
                className="space-y-4 rounded-md border border-[#bfdbfe] bg-[#eff6ff] p-5"
                onSubmit={createDeliveryPackage}
              >
                <p className="text-sm font-semibold uppercase tracking-wide text-[#2563eb]">
                  Neste handling
                </p>
                <h3 className="text-xl font-semibold">
                  {documents.length === 0
                    ? "Koble dokument til saken"
                    : approvedDocuments.length === 0
                      ? "Godkjenn dokument"
                      : "Lag leveringspakke"}
                </h3>
                {documents.length === 0 ? (
                  <p className="text-sm leading-6 text-[#334155]">
                    Saken er opprettet. Nå mangler et dokument som kan
                    klassifiseres, godkjennes og kobles til saken.
                  </p>
                ) : approvedDocuments.length === 0 ? (
                  <p className="text-sm leading-6 text-[#334155]">
                    Dokumentet er koblet til, men klassifiseringen må godkjennes
                    før levering kan opprettes.
                  </p>
                ) : (
                  <>
                    <p className="text-sm leading-6 text-[#334155]">
                      Velg godkjente dokumenter og opprett pakken som sendes
                      videre til kunden.
                    </p>
                    <label className="block">
                      <span className="mb-1 block text-sm font-semibold text-[#334155]">
                        Tittel på leveringspakke
                      </span>
                      <input
                        className="w-full rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                        onChange={(event) => setDeliveryTitle(event.target.value)}
                        placeholder={caseDetail.title}
                        value={deliveryTitle}
                      />
                    </label>
                    <div className="space-y-2">
                      <p className="text-sm font-semibold text-[#334155]">
                        Dokumenter
                      </p>
                      {documents.map((document) => {
                        const isApproved =
                          document.status.toLowerCase() === "approved";
                        return (
                          <label
                            className="flex items-start gap-3 rounded-md border border-[#d8deea] bg-white p-3 text-sm"
                            key={document.id}
                          >
                            <input
                              checked={selectedDocumentIds.includes(document.id)}
                              className="mt-1"
                              disabled={!isApproved}
                              onChange={() => toggleDocument(document.id)}
                              type="checkbox"
                            />
                            <span>
                              <span className="block font-medium text-[#162033]">
                                {document.title}
                              </span>
                              <span className="block text-[#64748b]">
                                {isApproved
                                  ? document.documentType ?? "Godkjent"
                                  : "Godkjenn klassifisering før levering"}
                              </span>
                            </span>
                          </label>
                        );
                      })}
                    </div>
                  </>
                )}
                {documents.length === 0 ? (
                  <Link
                    className="inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
                    href="/documents"
                  >
                    Opprett dokument
                  </Link>
                ) : approvedDocuments.length === 0 && firstPendingDocument ? (
                  <Link
                    className="inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
                    href={`/documents/${firstPendingDocument.id}`}
                  >
                    Åpne dokument
                  </Link>
                ) : (
                  <button
                    className="rounded-md bg-[#162033] px-4 py-2 text-sm font-semibold text-white hover:bg-[#334155] disabled:cursor-not-allowed disabled:opacity-60"
                    disabled={
                      submitting !== null || selectedDocumentIds.length === 0
                    }
                    type="submit"
                  >
                    {submitting === "delivery"
                      ? "Oppretter..."
                      : "Lag leveringspakke"}
                  </button>
                )}
              </form>

              <section className="rounded-md border border-[#d8deea] bg-white p-5">
                <h3 className="text-lg font-semibold">Automatisert i demoen</h3>
                <ul className="mt-3 space-y-2 text-sm leading-6 text-[#475569]">
                  <li>Input er kontrollert, beriket og godkjent av menneske.</li>
                  <li>Sak er opprettet med sporbar historikk.</li>
                  <li>
                    {firstDocument
                      ? "Dokumentflyten er koblet til saken."
                      : "Neste del er dokumentflyt og levering."}
                  </li>
                </ul>
              </section>
            </aside>
          </div>
        )}
      </div>
    </AppShell>
  );
}

function ProcessMetric({
  label,
  value,
  detail,
}: {
  label: string;
  value: string;
  detail: string;
}) {
  return (
    <div className="rounded-md border border-[#d8deea] bg-white p-4">
      <p className="text-sm font-semibold text-[#64748b]">{label}</p>
      <p className="mt-2 break-words text-xl font-semibold text-[#162033]">
        {value}
      </p>
      <p className="mt-1 text-sm text-[#64748b]">{detail}</p>
    </div>
  );
}

function activityLabel(action: string) {
  const labels: Record<string, string> = {
    AiAnalysisRequested: "Forslag generert",
    AiSuggestionApproved: "Forslag godkjent",
    AiSuggestionRejected: "Forslag avvist",
    CaseCreated: "Sak opprettet",
    CaseNoteCreated: "Notat lagt til",
    CaseTaskCreated: "Oppgave lagt til",
    DeliveryLinkCreated: "Kundelenke opprettet",
    DeliveryLinkRevoked: "Kundelenke deaktivert",
    DeliveryPackageCreated: "Leveringspakke opprettet",
    DeliveryPdfGenerated: "PDF generert",
    DocumentClassificationApproved: "Dokumentklassifisering godkjent",
    DocumentClassificationRequested: "Dokumentklassifisering foreslått",
    DocumentLinkedToCase: "Dokument koblet til sak",
    DocumentUploaded: "Dokument lastet opp",
    DocumentVersionUploaded: "Ny dokumentversjon lastet opp",
    IntakeCreated: "Input mottatt",
    SampleDocumentCreated: "Demo-dokument opprettet",
    ViewedDocument: "Kundedokument åpnet",
    ViewedPackage: "Kundeside åpnet",
  };

  return labels[action] ?? action;
}

function entityLabel(entityType: string) {
  const labels: Record<string, string> = {
    Case: "Sak",
    DeliveryPackage: "Levering",
    Document: "Dokument",
    IntakeItem: "Input",
    PublicDelivery: "Kundelenke",
  };

  return labels[entityType] ?? entityType;
}
