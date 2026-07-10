"use client";

import { useEffect, useState } from "react";
import type { DemoStoryEvidenceStep } from "@/lib/demo-story";

type ReplayStatus = "idle" | "running" | "completed" | "error";

type AutomationTimelineProps = {
  steps: DemoStoryEvidenceStep[];
  onComplete?: () => void;
};

const evidenceModeLabels: Record<string, string> = {
  implemented: "Implementert",
  "public-data-capable": "Offentlig datakilde / lagret demoøyeblikk",
  "demo-adapter": "Demo-adapter",
};

export function AutomationTimeline({ steps, onComplete }: AutomationTimelineProps) {
  const [status, setStatus] = useState<ReplayStatus>("idle");
  const [revealedCount, setRevealedCount] = useState(0);

  useEffect(() => {
    if (status !== "running") {
      return;
    }

    const timer = window.setTimeout(() => {
      const nextCount = revealedCount + 1;
      setRevealedCount(nextCount);
      if (nextCount >= steps.length) {
        setStatus("completed");
        onComplete?.();
      }
    }, 450);

    return () => window.clearTimeout(timer);
  }, [onComplete, revealedCount, status, steps.length]);

  const currentStep = status === "running" ? steps[revealedCount] : null;
  const visibleSteps = steps.slice(0, revealedCount);

  function startReplay() {
    if (status === "running") {
      return;
    }

    if (steps.length === 0) {
      setStatus("error");
      return;
    }

    const reduceMotion = window.matchMedia(
      "(prefers-reduced-motion: reduce)",
    ).matches;
    if (reduceMotion) {
      setRevealedCount(steps.length);
      setStatus("completed");
      onComplete?.();
      return;
    }

    setRevealedCount(0);
    setStatus("running");
  }

  return (
    <section aria-labelledby="automation-timeline-heading" className="py-10 sm:py-14">
      <div className="max-w-3xl">
        <p className="text-sm font-semibold text-[#315ea8]">Etter automatisering</p>
        <h2
          className="mt-2 text-3xl font-semibold tracking-tight text-[#172033]"
          id="automation-timeline-heading"
        >
          Dette skjer automatisk
        </h2>
        <p className="mt-3 text-base leading-7 text-[#526075]">
          Demoen spiller av en ferdig, fiktiv arbeidsflyt basert på data som
          allerede finnes i demoarbeidsområdet. Ingen kundesystemer kontaktes
          direkte.
        </p>
      </div>

      <div className="mt-7">
        <button
          aria-disabled={status === "running"}
          className="inline-flex rounded-md bg-[#315ea8] px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-[#274d8b] aria-disabled:cursor-wait aria-disabled:opacity-70 focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
          onClick={startReplay}
          type="button"
        >
          {status === "running"
            ? "Spiller av automatisert flyt"
            : status === "completed"
              ? "Spill av på nytt"
              : status === "error"
                ? "Prøv på nytt"
                : "Kjør automatisert flyt"}
        </button>

        <div aria-atomic="true" aria-live="polite" className="sr-only">
          {statusMessage(status, currentStep, revealedCount, steps.length)}
        </div>

        {status === "running" ? (
          <div className="rounded-lg border border-[#bfd1ec] bg-[#edf4ff] px-5 py-4 text-sm text-[#244a86]">
            <span className="font-semibold">
              Trinn {Math.min(revealedCount + 1, steps.length)} av {steps.length}:
            </span>{" "}
            {currentStep?.title}
          </div>
        ) : null}

        {status === "error" ? (
          <div
            className="rounded-lg border border-[#f3b7b7] bg-[#fff2f2] p-5 text-sm text-[#8f2525]"
            role="alert"
          >
            <p className="font-semibold">Arbeidsflyten kunne ikke spilles av.</p>
            <p className="mt-1">Ingen dokumentasjonstrinn ble returnert.</p>
          </div>
        ) : null}

        {visibleSteps.length > 0 ? (
          <ol className="mt-6 grid gap-4" aria-label="Automatiserte trinn">
            {visibleSteps.map((step) => (
              <TimelineStep key={step.key} step={step} />
            ))}
          </ol>
        ) : null}

        {status === "completed" ? (
          <div className="mt-6 flex flex-col items-start gap-3 sm:flex-row sm:items-center">
            <p className="text-sm font-semibold text-[#24613f]">
              Flyten er fullført med {steps.length} sporbare trinn.
            </p>
          </div>
        ) : null}
      </div>
    </section>
  );
}

function TimelineStep({ step }: { step: DemoStoryEvidenceStep }) {
  return (
    <li className="grid gap-4 rounded-lg border border-[#d8dee8] bg-white p-5 shadow-sm sm:grid-cols-[2.5rem_1fr]">
      <div
        aria-hidden="true"
        className="flex h-10 w-10 items-center justify-center rounded-full bg-[#e4f4ea] text-sm font-bold text-[#24613f]"
      >
        ✓
      </div>
      <div>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.1em] text-[#64748b]">
              Trinn {step.sequence} · {step.system}
            </p>
            <h3 className="mt-1 text-lg font-semibold text-[#243147]">{step.title}</h3>
          </div>
          <span className="w-fit rounded-full border border-[#c9d7ea] bg-[#f1f6fd] px-2.5 py-1 text-xs font-semibold text-[#315a91]">
            {evidenceModeLabels[step.evidenceMode] ?? step.evidenceMode}
          </span>
        </div>
        <p className="mt-2 text-sm leading-6 text-[#526075]">{step.description}</p>
        <p className="mt-3 text-sm font-semibold text-[#344258]">
          Dokumentasjon: {step.evidenceLabel}
        </p>
      </div>
    </li>
  );
}

function statusMessage(
  status: ReplayStatus,
  currentStep: DemoStoryEvidenceStep | null,
  revealedCount: number,
  total: number,
) {
  if (status === "running") {
    return `Behandler trinn ${revealedCount + 1} av ${total}: ${currentStep?.title ?? ""}`;
  }
  if (status === "completed") {
    return `Automatisert flyt fullført med ${total} trinn.`;
  }
  if (status === "error") {
    return "Arbeidsflyten kunne ikke spilles av.";
  }
  return "Arbeidsflyten er klar til avspilling.";
}
