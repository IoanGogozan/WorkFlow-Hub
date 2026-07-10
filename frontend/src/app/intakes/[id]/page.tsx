"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { FormEvent, useCallback, useEffect, useRef, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { SourceBadge } from "@/components/source-badge";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
import { api } from "@/lib/api";
import { cleanDemoSubject } from "@/lib/demo-intakes";
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
  const autoAnalyzeStarted = useRef(false);

  const populateSuggestionForm = useCallback((run: AiAnalysisRun) => {
    setSuggestionForm({
      customerName: run.suggestion.customerName ?? "",
      organizationNumber: run.suggestion.organizationNumber ?? "",
      category: run.suggestion.category ?? "",
      urgency: run.suggestion.urgency ?? "",
    });
  }, []);

  useEffect(() => {
    const controller = new AbortController();

    async function loadIntake() {
      try {
        setError(null);
        const loadedIntake = await api<IntakeItem>(`/api/intakes/${intakeId}`, {
            signal: controller.signal,
          });
        setIntake(loadedIntake);

        try {
          const latestAnalysis = await api<AiAnalysisRun>(
            `/api/intakes/${intakeId}/latest-ai`,
            { signal: controller.signal },
          );
          setAnalysis(latestAnalysis);
          populateSuggestionForm(latestAnalysis);
        } catch {
          // No proposal exists yet. The next effect will start one when needed.
        }
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
  }, [intakeId, populateSuggestionForm]);

  const analyze = useCallback(async () => {
    try {
      setAction("analyze");
      setActionError(null);
      const run = await api<AiAnalysisRun>(`/api/intakes/${intakeId}/analyze`, {
        method: "POST",
      });
      setAnalysis(run);
      populateSuggestionForm(run);
      setIntake(await api<IntakeItem>(`/api/intakes/${intakeId}`));
    } catch (analysisError) {
      setActionError(
        analysisError instanceof Error
          ? analysisError.message
          : "Forslaget kunne ikke lages.",
      );
    } finally {
      setAction(null);
    }
  }, [intakeId, populateSuggestionForm]);

  useEffect(() => {
    if (
      !intake ||
      analysis ||
      action !== null ||
      autoAnalyzeStarted.current ||
      isFinishedStatus(intake.status)
    ) {
      return;
    }

    autoAnalyzeStarted.current = true;
    void analyze();
  }, [action, analysis, analyze, intake]);

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
      const createdCase = await api<CaseDetail>(
        `/api/intakes/${intakeId}/convert-to-case`,
        { method: "POST" },
      );
      router.push(`/cases/${createdCase.id}`);
    } catch (approveError) {
      setActionError(
        approveError instanceof Error
          ? approveError.message
          : "Forslaget kunne ikke godkjennes og rutes videre.",
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
          <div className="grid gap-6 lg:grid-cols-[1fr_380px]">
            <section className="space-y-6">
              <div>
                <Link
                  className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                  href="/intakes"
                >
                  Tilbake til input
                </Link>
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <h2 className="text-3xl font-semibold">
                    {cleanDemoSubject(intake.subject)}
                  </h2>
                  <StatusBadge status={intake.status} />
                </div>
                <div className="mt-3 flex flex-wrap items-center gap-2 text-sm text-[#64748b]">
                  <SourceBadge source={intake.source} />
                  <span>Mottatt {formatDateTime(intake.receivedAt)}</span>
                </div>
                <div className="mt-5">
                  <WorkflowProgress activeStep={getWorkflowStep(intake.status, analysis)} />
                </div>
              </div>

              <article className="rounded-md border border-[#d8deea] bg-white p-5">
                <p className="text-sm font-semibold text-[#64748b]">
                  1. Input slik det kom inn
                </p>
                <h3 className="mt-1 text-lg font-semibold">
                  Original henvendelse
                </h3>
                <p className="mt-3 whitespace-pre-wrap text-sm leading-6 text-[#475569]">
                  {intake.body}
                </p>
              </article>
            </section>

            <aside className="space-y-6">
              {actionError ? <ErrorState message={actionError} /> : null}

              <section className="rounded-md border border-[#bfdbfe] bg-[#eff6ff] p-5">
                <h3 className="text-lg font-semibold text-[#1e3a8a]">
                  Valgfri AI-støtte
                </h3>
                <p className="mt-2 text-sm leading-6 text-[#475569]">
                  Demoen viser hvordan AI kan foreslå struktur når det gir
                  verdi. Flyten kan også brukes med manuell kontroll uten AI.
                </p>
                {action === "analyze" ? (
                  <p className="mt-3 text-sm font-semibold text-[#1d4ed8]">
                    Lager forslag...
                  </p>
                ) : analysis ? (
                  <p className="mt-3 text-sm font-semibold text-[#047857]">
                    Forslag klart for godkjenning
                  </p>
                ) : isFinishedStatus(intake.status) ? (
                  <p className="mt-3 text-sm font-semibold text-[#047857]">
                    Input er allerede behandlet
                  </p>
                ) : null}
              </section>

              {analysis && suggestionForm ? (
                <form
                  className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                  onSubmit={approve}
                >
                  <div>
                    <p className="text-sm font-semibold text-[#64748b]">
                      2. Forslag med kontroll
                    </p>
                    <h3 className="mt-1 text-lg font-semibold">
                      Kontroller og godkjenn
                    </h3>
                    <p className="mt-1 text-sm text-[#64748b]">
                      Sikkerhet: {Math.round(analysis.confidence * 100)}%
                    </p>
                  </div>
                  <p className="text-sm leading-6 text-[#475569]">
                    {analysis.suggestion.summary}
                  </p>
                  <div className="rounded-md border border-[#e2e8f0] bg-[#f8fafc] p-3 text-sm leading-6 text-[#475569]">
                    Er noe feil, retter mennesket feltene under før godkjenning.
                    Når forslaget godkjennes, opprettes saken automatisk.
                  </div>
                  <div className="rounded-md border border-[#bbf7d0] bg-[#f0fdf4] p-3 text-sm leading-6 text-[#166534]">
                    4. Sak opprettes automatisk når forslaget godkjennes.
                  </div>
                  <SuggestionInput
                    label="Kunde"
                    name="customerName"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.customerName}
                  />
                  <SuggestionInput
                    label="Organisasjonsnummer"
                    name="organizationNumber"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.organizationNumber}
                  />
                  <SuggestionInput
                    label="Kategori"
                    name="category"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.category}
                  />
                  <SuggestionInput
                    label="Prioritet"
                    name="urgency"
                    setSuggestionForm={setSuggestionForm}
                    value={suggestionForm.urgency}
                  />
                  {analysis.suggestion.suggestedTasks.length > 0 ? (
                    <div>
                      <p className="text-sm font-semibold">Foreslåtte oppgaver</p>
                      <ul className="mt-2 list-disc space-y-1 pl-5 text-sm text-[#475569]">
                        {analysis.suggestion.suggestedTasks.map((task) => (
                          <li key={task}>{task}</li>
                        ))}
                      </ul>
                    </div>
                  ) : null}
                  {analysis.suggestion.missingInformation.length > 0 ? (
                    <div className="rounded-md border border-[#fde68a] bg-[#fffbeb] p-3">
                      <p className="text-sm font-semibold text-[#92400e]">
                        Mangler før full automatikk
                      </p>
                      <ul className="mt-2 list-disc space-y-1 pl-5 text-sm text-[#92400e]">
                        {analysis.suggestion.missingInformation.map((item) => (
                          <li key={item}>{item}</li>
                        ))}
                      </ul>
                    </div>
                  ) : null}
                  <button
                    className="rounded-md bg-[#047857] px-4 py-2 text-sm font-semibold text-white hover:bg-[#065f46] disabled:cursor-not-allowed disabled:opacity-60"
                    disabled={action !== null}
                    type="submit"
                  >
                    {action === "approve"
                      ? "Godkjenner og oppretter sak..."
                      : "Godkjenn og opprett sak"}
                  </button>
                </form>
              ) : isFinishedStatus(intake.status) ? (
                <section className="rounded-md border border-[#d8deea] bg-white p-5">
                  <p className="text-sm font-semibold text-[#64748b]">
                    2. Forslag med kontroll
                  </p>
                  <h3 className="mt-1 text-lg font-semibold">
                    Forslag er ikke tilgjengelig her
                  </h3>
                  <p className="mt-2 text-sm leading-6 text-[#64748b]">
                    Dette inputet er allerede behandlet i eksisterende demo-data.
                    Bruk Start ny demo for å starte med ferske input der forslag
                    vises før godkjenning.
                  </p>
                </section>
              ) : null}

              <SavedIntakeResult intake={intake} />

              {intake.status.toLowerCase() === "approved" ? (
                <section className="rounded-md border border-[#d8deea] bg-white p-5">
                  <h3 className="text-lg font-semibold">Godkjent input</h3>
                  <p className="mt-2 text-sm leading-6 text-[#64748b]">
                    Strukturen er godkjent. Du kan sende den videre til sak hvis
                    den ikke allerede er rutet.
                  </p>
                  <button
                    className="mt-4 rounded-md bg-[#162033] px-4 py-2 text-sm font-semibold text-white hover:bg-[#334155] disabled:cursor-not-allowed disabled:opacity-60"
                    disabled={action !== null}
                    onClick={convertToCase}
                    type="button"
                  >
                    {action === "convert" ? "Oppretter..." : "Opprett sak"}
                  </button>
                </section>
              ) : null}
            </aside>
          </div>
        )}
      </div>
    </AppShell>
  );
}

function SavedIntakeResult({ intake }: { intake: IntakeItem }) {
  return (
    <section className="rounded-md border border-[#d8deea] bg-white p-5">
      <p className="text-sm font-semibold text-[#64748b]">
        3. Menneskelig godkjenning
      </p>
      <h3 className="mt-1 text-lg font-semibold">
        Resultat etter godkjenning
      </h3>
      <dl className="mt-4 grid gap-4 sm:grid-cols-2">
        <FieldValue label="Kunde" value={intake.customerName} />
        <FieldValue
          label="Organisasjonsnummer"
          value={intake.organizationNumber}
        />
        <FieldValue label="Kategori" value={intake.category} />
        <FieldValue label="Prioritet" value={intake.urgency} />
      </dl>
    </section>
  );
}

function isFinishedStatus(status: string) {
  const normalized = status.replace(/\s/g, "").toLowerCase();
  return normalized === "approved" || normalized === "convertedtocase";
}

function getWorkflowStep(status: string, analysis: AiAnalysisRun | null) {
  const normalized = status.replace(/\s/g, "").toLowerCase();

  if (normalized === "approved" || normalized === "convertedtocase") {
    return 3;
  }

  return analysis ? 2 : 1;
}

function FieldValue({ label, value }: { label: string; value: string | null }) {
  return (
    <div>
      <dt className="text-sm font-semibold text-[#334155]">{label}</dt>
      <dd className="mt-1 text-sm text-[#64748b]">{value ?? "Ikke satt"}</dd>
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
