import {
  LiveDemoStageCard,
  type LiveDemoStage,
} from "@/components/live-demo/live-demo-stage-card";
import { LiveDemoResultCard } from "@/components/live-demo/live-demo-result-card";
import type { LiveDemoCapabilities, LiveDemoRun, LiveDemoRunStep } from "@/lib/live-demo";

type LiveDemoRunPanelProps = {
  capabilities: LiveDemoCapabilities | null;
  error: string | null;
  isActive: boolean;
  isStarting: boolean;
  retry: () => void;
  run: LiveDemoRun | null;
  headingRef: React.RefObject<HTMLHeadingElement | null>;
};

const publicStages = ["Mottatt", "Kontrollert", "Opprettet", "Synkronisert"];

export function LiveDemoRunPanel({ capabilities, error, headingRef, isActive, isStarting, retry, run }: LiveDemoRunPanelProps) {
  const stages = createStages(run?.steps ?? []);
  const brregDurationMs = run?.steps.find((step) => step.key === "brreg-checked")?.durationMs ?? null;
  const erpStep = run?.steps.find((step) => step.key === "erp-received");
  return (
    <section aria-labelledby="live-preview-run-heading" id="live-preview-run">
      <div className="rounded-xl border border-[#dce1e8] bg-[#fdfefe] p-5 sm:p-8">
          <p className="text-sm font-semibold text-[#315ea8]">Live-kjøring</p>
        <h2
          className="mt-2 text-2xl font-semibold tracking-tight text-[#172033] sm:text-3xl"
          id="live-preview-run-heading"
          ref={headingRef}
          tabIndex={-1}
        >
          Én ny henvendelse, fire tydelige steg
        </h2>
        <p aria-atomic="true" aria-live="polite" className="sr-only" role="status">
          {screenReaderStatus(run, isStarting)}
        </p>
        <p className="mt-3 max-w-2xl text-base leading-7 text-[#526075]">
          Følg status, varighet og offentlig-safe bevis mens den fiktive
          henvendelsen behandles.
        </p>
        {capabilities?.enabled === false ? (
          <p className="mt-4 rounded-md bg-[#fff4e5] px-4 py-3 text-sm text-[#854d0e]">Live-demoen er ikke tilgjengelig akkurat nå.</p>
        ) : null}
        {error ? <p className="mt-4 rounded-md bg-[#fdecec] px-4 py-3 text-sm text-[#a33a3a]">{error}</p> : null}
        {!run && !error ? <p className="mt-4 text-sm text-[#64748b]">Start live-demoen for å opprette en ny fiktiv kjøring.</p> : null}
        <ol className="mt-6 grid gap-3 lg:grid-cols-2">
          {stages.map((stage) => (
            <LiveDemoStageCard key={stage.title} stage={stage} />
          ))}
        </ol>
        {run?.status === "Completed" && run.result ? (
          <LiveDemoResultCard
            brregDurationMs={brregDurationMs}
            erpAttemptCount={erpStep?.attemptCount ?? 0}
            erpDurationMs={erpStep?.durationMs ?? null}
            result={run.result}
            totalDurationMs={run.totalDurationMs}
          />
        ) : null}
        {run?.canRetry ? (
          <button className="mt-6 rounded-md border border-[#315ea8] px-5 py-3 text-sm font-semibold text-[#315ea8] hover:bg-[#e8f0ff] disabled:cursor-not-allowed disabled:opacity-60" disabled={isStarting || isActive} onClick={retry} type="button">Prøv igjen</button>
        ) : null}
      </div>
    </section>
  );
}

function screenReaderStatus(run: LiveDemoRun | null, isStarting: boolean) {
  if (isStarting) return "Live-demoen starter.";
  if (!run) return "Live-demoen er klar til å starte.";
  if (run.status === "Completed") return "Live-demoen er fullført. Resultater og bevis er tilgjengelige.";
  if (run.status === "Failed" && run.canRetry) return "Live-demoen feilet kontrollert. Prøv igjen er tilgjengelig.";
  if (run.status === "Failed") return "Live-demoen feilet.";
  return `Live-demoen kjører. Aktivt steg: ${run.currentStepKey ?? "venter"}.`;
}

function createStages(steps: LiveDemoRunStep[]): LiveDemoStage[] {
  return publicStages.map((title) => {
    const stageSteps = steps.filter((step) => step.publicStage === title);
    const brregStep = stageSteps.find((step) => step.key === "brreg-checked");
    const status = getStageStatus(stageSteps);
    const durationMs = stageSteps.reduce((total, step) => total + (step.durationMs ?? 0), 0);
    return {
      title,
      status,
      duration: durationMs > 0 ? `${(durationMs / 1000).toLocaleString("nb-NO", { maximumFractionDigits: 1 })} sek` : "–",
      provider: stageSteps.map((step) => step.provider).filter((value, index, values) => values.indexOf(value) === index).join(" + ") || "Venter på kjøring",
      summary: stageSteps.find((step) => step.publicErrorMessage)?.publicErrorMessage ?? stageSteps.find((step) => step.publicSummary)?.publicSummary ?? (status === "Venter" ? "Venter på neste sikre steg." : "Status oppdateres."),
      evidence: stageSteps.find((step) => step.publicEvidenceReference)?.publicEvidenceReference ?? "–",
      brregEvidence: createBrregEvidence(brregStep),
    };
  });
}

function createBrregEvidence(step: LiveDemoRunStep | undefined): LiveDemoStage["brregEvidence"] {
  if (step?.status !== "Completed") {
    return undefined;
  }

  if (step.publicEvidenceReference === "live") {
    const duration = step.durationMs === null
      ? "fullført"
      : `${(step.durationMs / 1000).toLocaleString("nb-NO", { maximumFractionDigits: 1 })} sek`;
    return { mode: "live", text: `Live kontroll – ${duration}` };
  }

  if (step.publicEvidenceReference === "fallback") {
    return { mode: "fallback", text: "Fallback-snapshot – live tjeneste var utilgjengelig" };
  }

  return undefined;
}

function getStageStatus(steps: LiveDemoRunStep[]): LiveDemoStage["status"] {
  if (steps.some((step) => step.status === "Failed")) return "Feilet";
  if (steps.some((step) => step.status === "Running")) return "Pågår";
  if (steps.length > 0 && steps.every((step) => step.status === "Completed" || step.status === "Skipped")) return "Fullført";
  return "Venter";
}
