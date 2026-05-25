"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { SourceBadge } from "@/components/source-badge";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
import { api } from "@/lib/api";
import { cleanDemoSubject, getDemoPreviewIntakes } from "@/lib/demo-intakes";
import type { IntakeListItem, IntegrationConnection } from "@/lib/types";

type StartPageData = {
  intakes: IntakeListItem[];
  integrations: IntegrationConnection[];
};

const processSteps = [
  {
    title: "1. Samle input",
    text: "E-post, skjema, API og manuelle henvendelser havner i samme arbeidsliste.",
  },
  {
    title: "2. Få forslag",
    text: "AI foreslår kunde, kategori, prioritet, oppgaver og hva som mangler.",
  },
  {
    title: "3. Send videre",
    text: "Godkjente data brukes til sak, dokumenter, rapportering og eksterne systemer.",
  },
];

const gains = [
  "Raskere saksbehandling",
  "Mindre manuelt arbeid",
  "Færre feil i overføring",
  "Full sporbarhet",
];

export default function Home() {
  const [data, setData] = useState<StartPageData | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadStartPage() {
      try {
        setError(null);
        const [intakes, integrations] = await Promise.all([
          api<IntakeListItem[]>("/api/intakes", { signal: controller.signal }),
          api<IntegrationConnection[]>("/api/integrations", {
            signal: controller.signal,
          }),
        ]);

        setData({ intakes, integrations });
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Demo start data could not be loaded.",
          );
        }
      }
    }

    void loadStartPage();
    return () => controller.abort();
  }, []);

  return (
    <AppShell>
      <div className="mx-auto max-w-7xl px-6 py-6">
        {error ? (
          <ErrorState message={error} />
        ) : !data ? (
          <LoadingState label="Laster demooversikt" />
        ) : (
          <StartPageContent data={data} />
        )}
      </div>
    </AppShell>
  );
}

function StartPageContent({ data }: { data: StartPageData }) {
  const previewIntakes = useMemo(
    () => getDemoPreviewIntakes(data.intakes),
    [data],
  );

  return (
    <>
      <section aria-labelledby="start-heading" className="space-y-6">
        <section className="rounded-md border border-[#d8deea] bg-white p-5 sm:p-6">
          <h2
            id="start-heading"
            className="max-w-3xl text-3xl font-semibold text-[#162033]"
          >
            Fra input til leveranse
          </h2>
          <p className="mt-3 max-w-4xl text-base leading-7 text-[#475569]">
            Samle e-post, skjema og API-data. La AI strukturere saken. Send
            godkjent informasjon videre til riktige systemer.
          </p>
          <div className="mt-5 flex flex-col gap-3 sm:flex-row sm:items-center">
            <Link
              className="inline-flex w-fit rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
              href="/intakes"
            >
              Åpne inputliste
            </Link>
          </div>
          <div className="mt-5">
            <WorkflowProgress activeStep={1} />
          </div>
        </section>

        <section className="rounded-md border border-[#d8deea] bg-white">
          <div className="flex items-center justify-between gap-3 border-b border-[#d8deea] px-5 py-4">
            <div>
              <h3 className="text-lg font-semibold">Demo data</h3>
              <p className="mt-1 text-sm text-[#64748b]">
                Velg et eksempel og se hvordan input blir til sak og leveranse.
              </p>
            </div>
            <Link
              className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
              href="/intakes"
            >
              Se alle input
            </Link>
          </div>
          <div className="divide-y divide-[#e2e8f0]">
            {previewIntakes.map((item) => (
              <article
                className="grid gap-3 p-4 md:grid-cols-[1fr_150px_92px] md:items-center"
                key={item.id}
              >
                <div>
                  <div className="flex flex-wrap items-center gap-2">
                    <SourceBadge source={item.source} />
                    <Link
                      className="font-semibold text-[#162033] hover:text-[#2563eb]"
                      href={
                        item.id.startsWith("preview-")
                          ? "/intakes"
                          : `/intakes/${item.id}`
                      }
                    >
                      {cleanDemoSubject(item.subject)}
                    </Link>
                  </div>
                  <p className="mt-1 text-sm text-[#64748b]">
                    {[item.customerName, item.category].filter(Boolean).join(" - ")}
                  </p>
                </div>
                <div className="md:text-right">
                  <StatusBadge status={item.status} />
                </div>
                <Link
                  className="inline-flex w-fit rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm font-semibold text-[#334155] hover:bg-[#eef2ff] md:justify-self-end"
                  href={
                    item.id.startsWith("preview-")
                      ? "/intakes"
                      : `/intakes/${item.id}`
                  }
                >
                  Åpne
                </Link>
              </article>
            ))}
          </div>
        </section>

        <section aria-labelledby="process-heading">
          <h3 id="process-heading" className="text-lg font-semibold">
            Hva skjer med valgt input?
          </h3>
          <div className="mt-3 grid gap-4 md:grid-cols-3">
            {processSteps.map((step) => (
              <article
                className="rounded-md border border-[#d8deea] bg-white p-4"
                key={step.title}
              >
                <h4 className="font-semibold text-[#162033]">{step.title}</h4>
                <p className="mt-2 text-sm leading-6 text-[#475569]">
                  {step.text}
                </p>
              </article>
            ))}
          </div>
        </section>

        <section className="rounded-md border border-[#d8deea] bg-white p-5">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <h3 className="text-lg font-semibold">Gevinst for kunden</h3>
              <p className="mt-1 text-sm leading-6 text-[#64748b]">
                Mindre dobbeltarbeid og bedre kontroll fra første henvendelse
                til ferdig leveranse.
              </p>
            </div>
            <Link
              className="inline-flex w-fit rounded-md border border-[#cbd5e1] bg-white px-4 py-2 text-sm font-semibold text-[#334155] hover:bg-[#eef2ff]"
              href="/integrations"
            >
              Se integrasjoner
            </Link>
          </div>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            {gains.map((gain) => (
              <div
                className="rounded-md border border-[#e2e8f0] bg-[#f8fafc] px-3 py-2 text-sm font-semibold text-[#334155]"
                key={gain}
              >
                {gain}
              </div>
            ))}
          </div>
        </section>

        <IntegrationSummaryCard integrations={data.integrations} />
      </section>
    </>
  );
}

function IntegrationSummaryCard({
  integrations,
}: {
  integrations: IntegrationConnection[];
}) {
  const displayNames =
    integrations.length > 0
      ? integrations.map((integration) => integration.displayName)
      : [
          "Brreg",
          "Microsoft Graph / SharePoint",
          "Power BI / Fabric",
          "Tripletex Accounting",
        ];

  return (
    <section className="rounded-md border border-[#d8deea] bg-white p-5">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h3 className="text-lg font-semibold">Data sendes videre til</h3>
          <p className="mt-1 text-sm leading-6 text-[#64748b]">
            Demoen viser typiske mottakere etter at data er kontrollert og
            godkjent.
          </p>
        </div>
        <Link
          className="inline-flex w-fit rounded-md border border-[#cbd5e1] bg-white px-4 py-2 text-sm font-semibold text-[#334155] hover:bg-[#eef2ff]"
          href="/integrations"
        >
          Åpne integrasjoner
        </Link>
      </div>
      <ul className="mt-4 grid gap-3 text-sm font-semibold text-[#334155] sm:grid-cols-2 lg:grid-cols-4">
        {displayNames.slice(0, 4).map((name) => (
          <li className="rounded-md border border-[#e2e8f0] bg-[#f8fafc] px-3 py-2" key={name}>
            {name}
          </li>
        ))}
      </ul>
    </section>
  );
}
