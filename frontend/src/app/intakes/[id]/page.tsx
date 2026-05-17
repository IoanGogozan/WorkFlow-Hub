"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import {
  DemoCapabilityBadge,
  DemoCapabilityNote,
} from "@/components/demo-capability-badge";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { api } from "@/lib/api";
import { aiCapability } from "@/lib/demo-capabilities";
import { formatDateTime } from "@/lib/format";
import type { AiAnalysisRun, IntakeItem, CaseDetail } from "@/lib/types";

type SuggestionForm = {
  customerName: string;
  organizationNumber: string;
  category: string;
  urgency: string;
};

export default function IntakeDetailPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const intakeId = params.id;
  const [intake, setIntake] = useState<IntakeItem | null>(null);
  const [analysis, setAnalysis] = useState<AiAnalysisRun | null>(null);
  const [suggestionForm, setSuggestionForm] = useState<SuggestionForm | null>(
    null,
  );
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [action, setAction] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadIntake() {
      try {
        setError(null);
        setIntake(
          await api<IntakeItem>(`/api/intakes/${intakeId}`, {
            signal: controller.signal,
          }),
        );
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Intake could not be loaded.",
          );
        }
      }
    }

    void loadIntake();
    return () => controller.abort();
  }, [intakeId]);

  async function analyze() {
    try {
      setAction("analyze");
      setActionError(null);
      const run = await api<AiAnalysisRun>(`/api/intakes/${intakeId}/analyze`, {
        method: "POST",
      });
      setAnalysis(run);
      setSuggestionForm({
        customerName: run.suggestion.customerName ?? "",
        organizationNumber: run.suggestion.organizationNumber ?? "",
        category: run.suggestion.category ?? "",
        urgency: run.suggestion.urgency ?? "",
      });
      setIntake(await api<IntakeItem>(`/api/intakes/${intakeId}`));
    } catch (analysisError) {
      setActionError(
        analysisError instanceof Error
          ? analysisError.message
          : "AI analysis could not be started.",
      );
    } finally {
      setAction(null);
    }
  }

  async function approve(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!analysis || !suggestionForm) {
      return;
    }

    try {
      setAction("approve");
      setActionError(null);
      const updated = await api<IntakeItem>(
        `/api/intakes/${intakeId}/approve-ai`,
        {
          method: "POST",
          body: {
            aiAnalysisRunId: analysis.id,
            ...suggestionForm,
          },
        },
      );
      setIntake(updated);
    } catch (approveError) {
      setActionError(
        approveError instanceof Error
          ? approveError.message
          : "AI suggestion could not be approved.",
      );
    } finally {
      setAction(null);
    }
  }

  async function convertToCase() {
    try {
      setAction("convert");
      setActionError(null);
      const createdCase = await api<CaseDetail>(
        `/api/intakes/${intakeId}/convert-to-case`,
        { method: "POST" },
      );
      router.push(`/cases/${createdCase.id}`);
    } catch (convertError) {
      setActionError(
        convertError instanceof Error
          ? convertError.message
          : "Intake could not be converted to a case.",
      );
    } finally {
      setAction(null);
    }
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-5xl px-6 py-6">
        {error ? (
          <ErrorState message={error} />
        ) : !intake ? (
          <LoadingState label="Loading intake" />
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
            <section className="space-y-6">
              <div>
                <Link
                  className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                  href="/intakes"
                >
                  Back to intakes
                </Link>
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <h2 className="text-3xl font-semibold">{intake.subject}</h2>
                  <StatusBadge status={intake.status} />
                </div>
                <p className="mt-2 text-sm text-[#64748b]">
                  {intake.source} · Received {formatDateTime(intake.receivedAt)}
                </p>
              </div>

              <article className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Request body</h3>
                <p className="mt-3 whitespace-pre-wrap text-sm leading-6 text-[#475569]">
                  {intake.body}
                </p>
              </article>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Current fields</h3>
                <dl className="mt-4 grid gap-4 sm:grid-cols-2">
                  <FieldValue label="Customer" value={intake.customerName} />
                  <FieldValue
                    label="Organization number"
                    value={intake.organizationNumber}
                  />
                  <FieldValue label="Category" value={intake.category} />
                  <FieldValue label="Urgency" value={intake.urgency} />
                </dl>
              </section>
            </section>

            <aside className="space-y-6">
              {actionError ? <ErrorState message={actionError} /> : null}

              <section className="rounded-md border border-[#d8deea] bg-white p-5">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="text-lg font-semibold">AI review</h3>
                  <DemoCapabilityBadge capability={aiCapability} />
                </div>
                <p className="mt-2 text-sm leading-6 text-[#64748b]">
                  AI suggestions require approval before they change intake data.
                </p>
                <div className="mt-3">
                  <DemoCapabilityNote capability={aiCapability} />
                </div>
                <button
                  className="mt-4 rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={action !== null}
                  onClick={analyze}
                  type="button"
                >
                  {action === "analyze" ? "Analyzing..." : "Analyze with AI"}
                </button>
              </section>

              {analysis && suggestionForm ? (
                <form
                  className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                  onSubmit={approve}
                >
                  <div>
                    <h3 className="text-lg font-semibold">AI suggestion</h3>
                    <p className="mt-1 text-sm text-[#64748b]">
                      Confidence: {Math.round(analysis.confidence * 100)}%
                    </p>
                  </div>
                  <p className="text-sm leading-6 text-[#475569]">
                    {analysis.suggestion.summary}
                  </p>
                  <SuggestionInput
                    label="Customer name"
                    name="customerName"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.customerName}
                  />
                  <SuggestionInput
                    label="Organization number"
                    name="organizationNumber"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.organizationNumber}
                  />
                  <SuggestionInput
                    label="Category"
                    name="category"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.category}
                  />
                  <SuggestionInput
                    label="Urgency"
                    name="urgency"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.urgency}
                  />
                  {analysis.suggestion.suggestedTasks.length > 0 ? (
                    <div>
                      <p className="text-sm font-semibold">Suggested tasks</p>
                      <ul className="mt-2 list-disc space-y-1 pl-5 text-sm text-[#475569]">
                        {analysis.suggestion.suggestedTasks.map((task) => (
                          <li key={task}>{task}</li>
                        ))}
                      </ul>
                    </div>
                  ) : null}
                  <button
                    className="rounded-md bg-[#047857] px-4 py-2 text-sm font-semibold text-white hover:bg-[#065f46] disabled:cursor-not-allowed disabled:opacity-60"
                    disabled={action !== null}
                    type="submit"
                  >
                    {action === "approve" ? "Approving..." : "Approve suggestion"}
                  </button>
                </form>
              ) : null}

              <section className="rounded-md border border-[#d8deea] bg-white p-5">
                <h3 className="text-lg font-semibold">Case conversion</h3>
                <p className="mt-2 text-sm leading-6 text-[#64748b]">
                  Create a case workspace from this intake.
                </p>
                <button
                  className="mt-4 rounded-md bg-[#162033] px-4 py-2 text-sm font-semibold text-white hover:bg-[#334155] disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={action !== null}
                  onClick={convertToCase}
                  type="button"
                >
                  {action === "convert" ? "Converting..." : "Convert to case"}
                </button>
              </section>
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
      <dd className="mt-1 text-sm text-[#64748b]">{value ?? "Not set"}</dd>
    </div>
  );
}

function SuggestionInput({
  label,
  name,
  value,
  setSuggestionForm,
}: {
  label: string;
  name: keyof SuggestionForm;
  value: string;
  setSuggestionForm: React.Dispatch<React.SetStateAction<SuggestionForm | null>>;
}) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-semibold text-[#334155]">
        {label}
      </span>
      <input
        className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
        onChange={(event) =>
          setSuggestionForm((current) =>
            current ? { ...current, [name]: event.target.value } : current,
          )
        }
        value={value}
      />
    </label>
  );
}
