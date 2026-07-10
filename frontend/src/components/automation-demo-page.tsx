"use client";

import { useCallback, useEffect, useState } from "react";
import { AutomationTimeline } from "@/components/automation-timeline";
import { ClientDemoCta } from "@/components/client-demo-cta";
import { ClientDemoShell } from "@/components/client-demo-shell";
import { ErrorState } from "@/components/error-state";
import { IncomingRequestCard } from "@/components/incoming-request-card";
import { IntegrationEvidenceList } from "@/components/integration-evidence-list";
import { LoadingState } from "@/components/loading-state";
import { ManualProcessPanel } from "@/components/manual-process-panel";
import { OutcomeSummary } from "@/components/outcome-summary";
import { TechnicalEvidencePanel } from "@/components/technical-evidence-panel";
import { TimeSavingsCalculator } from "@/components/time-savings-calculator";
import { api } from "@/lib/api";
import type { DemoStory } from "@/lib/demo-story";

export function AutomationDemoPage() {
  const [story, setStory] = useState<DemoStory | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    api<DemoStory>("/api/demo-story", { signal: controller.signal })
      .then(setStory)
      .catch((loadError: unknown) => {
        if (loadError instanceof DOMException && loadError.name === "AbortError") {
          return;
        }
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Integrasjonseksempelet kunne ikke lastes.",
        );
      });

    return () => controller.abort();
  }, []);

  return (
    <ClientDemoShell>
      <div className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 sm:py-16">
        {!story && !error ? (
          <LoadingState label="Laster integrasjonseksempel" />
        ) : null}
        {error ? (
          <ErrorState
            title="Kunne ikke laste integrasjonseksempelet"
            message={error}
          />
        ) : null}
        {story ? <AutomationStory story={story} /> : null}
      </div>
    </ClientDemoShell>
  );
}

function AutomationStory({ story }: { story: DemoStory }) {
  const [isReplayComplete, setIsReplayComplete] = useState(false);
  const handleReplayComplete = useCallback(() => setIsReplayComplete(true), []);

  return (
    <>
      <section className="max-w-4xl py-4 sm:py-10">
        <p className="text-sm font-semibold text-[#315ea8]">
          Integrasjonseksempel for tekniske servicebedrifter
        </p>
        <h1 className="mt-4 text-4xl font-semibold leading-tight tracking-tight text-[#172033] sm:text-5xl">
          Fra e-post til opprettet sak – uten dobbeltregistrering
        </h1>
        <p className="mt-5 max-w-3xl text-base leading-7 text-[#526075] sm:text-lg">
          Denne demoen viser hvordan informasjon kan flyttes sikkert mellom
          systemene bedriften allerede bruker. Målet er mindre kopiering og
          innliming, færre feil og bedre sporbarhet.
        </p>
      </section>
      <IncomingRequestCard request={story.request} />
      <ManualProcessPanel />
      <AutomationTimeline steps={story.evidenceSteps} onComplete={handleReplayComplete} />
      {isReplayComplete ? (
        <>
          <OutcomeSummary outcome={story.outcome} />
          <TimeSavingsCalculator />
          <IntegrationEvidenceList integrations={story.integrations} />
          <TechnicalEvidencePanel links={story.technicalLinks} />
          <ClientDemoCta />
        </>
      ) : null}
    </>
  );
}
