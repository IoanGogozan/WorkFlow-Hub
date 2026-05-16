"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { useRouter } from "next/navigation";
import { AppShell } from "@/components/app-shell";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
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
              : "Case could not be loaded.",
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
        taskError instanceof Error ? taskError.message : "Task could not be added.",
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
        noteError instanceof Error ? noteError.message : "Note could not be added.",
      );
    } finally {
      setSubmitting(null);
    }
  }

  async function createDeliveryPackage(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (selectedDocumentIds.length === 0) {
      setActionError("Select at least one linked document for delivery.");
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
          : "Delivery package could not be created.",
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

  return (
    <AppShell>
      <div className="mx-auto max-w-6xl px-6 py-6">
        {error ? (
          <ErrorState message={error} />
        ) : !caseDetail || !activity ? (
          <LoadingState label="Loading case" />
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
            <section className="space-y-6">
              <div>
                <Link
                  className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                  href="/cases"
                >
                  Back to cases
                </Link>
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <h2 className="text-3xl font-semibold">{caseDetail.title}</h2>
                  <StatusBadge status={caseDetail.status} />
                </div>
                <p className="mt-2 text-sm text-[#64748b]">
                  {caseDetail.caseNumber} · Created{" "}
                  {formatDateTime(caseDetail.createdAt)}
                </p>
              </div>

              <article className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Description</h3>
                <p className="mt-3 whitespace-pre-wrap text-sm leading-6 text-[#475569]">
                  {caseDetail.description ?? "No description."}
                </p>
              </article>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Case fields</h3>
                <dl className="mt-4 grid gap-4 sm:grid-cols-2">
                  <FieldValue label="Due date" value={formatDate(caseDetail.dueDate)} />
                  <FieldValue
                    label="Source intake"
                    value={caseDetail.sourceIntakeItemId}
                  />
                  <FieldValue label="Owner user" value={caseDetail.ownerUserId} />
                  <FieldValue label="Tenant" value={caseDetail.tenantId} />
                </dl>
              </section>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Activity</h3>
                {activity.length === 0 ? (
                  <div className="mt-4">
                    <EmptyState
                      message="No audit activity was returned for this case yet."
                      title="No activity"
                    />
                  </div>
                ) : (
                  <div className="mt-4 divide-y divide-[#e2e8f0]">
                    {activity.map((item) => (
                      <div className="py-3" key={item.id}>
                        <p className="font-medium text-[#162033]">{item.action}</p>
                        <p className="mt-1 text-sm text-[#64748b]">
                          {item.entityType} · {formatDateTime(item.createdAt)}
                        </p>
                      </div>
                    ))}
                  </div>
                )}
              </section>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Linked documents</h3>
                {documents.length === 0 ? (
                  <div className="mt-4">
                    <EmptyState
                      message="Link approved documents to this case before creating a delivery package."
                      title="No linked documents"
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
                            {document.documentType ?? "Unclassified"} ·{" "}
                            {formatDate(document.expiryDate)}
                          </p>
                        </div>
                        <StatusBadge status={document.status} />
                      </div>
                    ))}
                  </div>
                )}
              </section>
            </section>

            <aside className="space-y-6">
              {actionError ? <ErrorState message={actionError} /> : null}

              <form
                className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                onSubmit={addTask}
              >
                <h3 className="text-lg font-semibold">Add task</h3>
                <label className="block">
                  <span className="mb-1 block text-sm font-semibold text-[#334155]">
                    Title
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
                    Description
                  </span>
                  <textarea
                    className="min-h-24 w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                    onChange={(event) => setTaskDescription(event.target.value)}
                    value={taskDescription}
                  />
                </label>
                <button
                  className="rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={submitting !== null}
                  type="submit"
                >
                  {submitting === "task" ? "Adding..." : "Add task"}
                </button>
              </form>

              <form
                className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                onSubmit={addNote}
              >
                <h3 className="text-lg font-semibold">Add note</h3>
                <label className="block">
                  <span className="mb-1 block text-sm font-semibold text-[#334155]">
                    Note
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
                  {submitting === "note" ? "Adding..." : "Add note"}
                </button>
              </form>

              <form
                className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                onSubmit={createDeliveryPackage}
              >
                <h3 className="text-lg font-semibold">Create delivery package</h3>
                <label className="block">
                  <span className="mb-1 block text-sm font-semibold text-[#334155]">
                    Package title
                  </span>
                  <input
                    className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                    onChange={(event) => setDeliveryTitle(event.target.value)}
                    placeholder={caseDetail.title}
                    value={deliveryTitle}
                  />
                </label>
                {documents.length === 0 ? (
                  <p className="text-sm leading-6 text-[#64748b]">
                    No case documents are available for delivery.
                  </p>
                ) : (
                  <div className="space-y-2">
                    <p className="text-sm font-semibold text-[#334155]">
                      Documents
                    </p>
                    {documents.map((document) => {
                      const isApproved =
                        document.status.toLowerCase() === "approved";
                      return (
                        <label
                          className="flex items-start gap-3 rounded-md border border-[#e2e8f0] p-3 text-sm"
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
                                ? document.documentType ?? "Approved"
                                : "Approve classification before delivery"}
                            </span>
                          </span>
                        </label>
                      );
                    })}
                  </div>
                )}
                <button
                  className="rounded-md bg-[#162033] px-4 py-2 text-sm font-semibold text-white hover:bg-[#334155] disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={submitting !== null || documents.length === 0}
                  type="submit"
                >
                  {submitting === "delivery" ? "Creating..." : "Create package"}
                </button>
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
