"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { SourceBadge } from "@/components/source-badge";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
import { api } from "@/lib/api";
import { cleanDemoSubject, getDemoReadyIntakes } from "@/lib/demo-intakes";
import { formatDateTime } from "@/lib/format";
import type { IntakeListItem } from "@/lib/types";

const intakeHighlights = [
  "E-post",
  "Skjema",
  "API",
  "Manuell registrering",
];

export default function IntakesPage() {
  const [intakes, setIntakes] = useState<IntakeListItem[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const demoIntakes = useMemo(
    () => (intakes ? getDemoReadyIntakes(intakes) : null),
    [intakes],
  );
  const displayIntakes = useMemo(
    () => (demoIntakes ? getDisplayIntakes(demoIntakes) : null),
    [demoIntakes],
  );
  const firstProcessableIntake = useMemo(
    () =>
      displayIntakes?.find((intake) => !isFinishedIntake(intake.status)) ??
      displayIntakes?.[0] ??
      null,
    [displayIntakes],
  );

  useEffect(() => {
    const controller = new AbortController();

    async function loadIntakes() {
      try {
        setError(null);
        setIntakes(
          await api<IntakeListItem[]>("/api/intakes", {
            signal: controller.signal,
          }),
        );
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Intakes could not be loaded.",
          );
        }
      }
    }

    void loadIntakes();
    return () => controller.abort();
  }, []);

  return (
    <AppShell>
      <div className="mx-auto max-w-7xl px-6 py-6">
        <section className="mb-6 rounded-md border border-[#d8deea] bg-white p-5 sm:p-6">
          <p className="text-sm font-medium text-[#64748b]">
            Steg 1: Samle input
          </p>
          <div className="mt-2 flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <h2 className="text-3xl font-semibold">Input til behandling</h2>
              <p className="mt-3 max-w-3xl text-base leading-7 text-[#475569]">
                Her samles henvendelser fra flere kanaler før AI foreslår
                struktur og en person godkjenner veien videre.
              </p>
            </div>
            <Link
              className="inline-flex w-fit rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
              href={
                firstProcessableIntake
                  ? `/intakes/${firstProcessableIntake.id}`
                  : "/"
              }
            >
              Behandle første input
            </Link>
          </div>
          <div className="mt-5 grid grid-cols-2 gap-3 lg:grid-cols-4">
            {intakeHighlights.map((item) => (
              <div
                className="rounded-md border border-[#e2e8f0] bg-[#f8fafc] px-3 py-2 text-sm font-semibold text-[#334155]"
                key={item}
              >
                {item}
              </div>
            ))}
          </div>
          <div className="mt-5">
            <WorkflowProgress activeStep={1} />
          </div>
        </section>

        {displayIntakes && displayIntakes.length > 0 ? (
          <div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h3 className="text-lg font-semibold">Velg input</h3>
              <p className="mt-1 text-sm text-[#64748b]">
                Viser de mest relevante demo-eksemplene. Åpne en rad for å se
                AI-forslag, manglende informasjon og neste handling.
              </p>
            </div>
            <p className="text-sm font-semibold text-[#475569]">
              {displayIntakes.length} input
            </p>
          </div>
        ) : null}

        {error ? (
          <ErrorState message={error} />
        ) : !displayIntakes ? (
          <LoadingState label="Loading intakes" />
        ) : displayIntakes.length === 0 ? (
          <EmptyState
            action={
              <Link
                className="inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
                href="/"
              >
                Til demooversikt
              </Link>
            }
            message="Ingen demo-klare input ble funnet i dette arbeidsområdet."
            title="Ingen input å vise"
          />
        ) : (
          <section className="overflow-hidden rounded-md border border-[#d8deea] bg-white">
            <div className="hidden grid-cols-[1fr_180px_150px_112px] gap-4 border-b border-[#d8deea] bg-[#f8fafc] px-5 py-3 text-xs font-semibold uppercase tracking-wide text-[#64748b] lg:grid">
              <span>Forespørsel</span>
              <span>Kunde</span>
              <span>Mottatt</span>
              <span className="text-right">Handling</span>
            </div>
            <div className="divide-y divide-[#e2e8f0]">
              {displayIntakes.map((intake) => (
                <article
                  className="grid gap-4 p-5 lg:grid-cols-[1fr_180px_150px_112px] lg:items-center"
                  key={intake.id}
                >
                  <div>
                    <div className="flex flex-wrap items-center gap-2">
                      <SourceBadge source={intake.source} />
                      <StatusBadge status={intake.status} />
                    </div>
                    <Link
                      className="mt-2 block text-lg font-semibold leading-7 text-[#162033] hover:text-[#2563eb]"
                      href={`/intakes/${intake.id}`}
                    >
                      {cleanDemoSubject(intake.subject)}
                    </Link>
                    <p className="mt-1 text-sm text-[#64748b] lg:hidden">
                      {buildIntakeMeta(intake)}
                    </p>
                  </div>
                  <div className="text-sm text-[#475569]">
                    <p className="font-semibold text-[#162033] lg:hidden">
                      Kunde
                    </p>
                    <p>{intake.customerName ?? "Ikke valgt"}</p>
                    <p className="mt-1 text-xs text-[#64748b]">
                      {[intake.category, intake.urgency]
                        .filter(Boolean)
                        .join(" - ") || "Ingen klassifisering ennå"}
                    </p>
                  </div>
                  <div className="text-sm text-[#475569]">
                    <p className="font-semibold text-[#162033] lg:hidden">
                      Mottatt
                    </p>
                    <p>{formatDateTime(intake.receivedAt)}</p>
                  </div>
                  <Link
                    className="inline-flex w-fit rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] lg:justify-self-end"
                    href={`/intakes/${intake.id}`}
                  >
                    Åpne
                  </Link>
                </article>
              ))}
            </div>
          </section>
        )}
      </div>
    </AppShell>
  );
}

function buildIntakeMeta(intake: IntakeListItem) {
  return (
    [intake.customerName, intake.category, intake.urgency]
      .filter(Boolean)
      .join(" - ") || "Ingen klassifisering ennå"
  );
}

function getDisplayIntakes(intakes: IntakeListItem[]) {
  const seen = new Set<string>();
  const uniqueIntakes: IntakeListItem[] = [];

  for (const intake of intakes) {
    const key = [
      cleanDemoSubject(intake.subject),
      intake.source,
      intake.customerName,
      intake.category,
    ].join("|");

    if (!seen.has(key)) {
      seen.add(key);
      uniqueIntakes.push(intake);
    }
  }

  return uniqueIntakes
    .sort((first, second) => {
      const firstDone = isFinishedIntake(first.status) ? 1 : 0;
      const secondDone = isFinishedIntake(second.status) ? 1 : 0;
      return firstDone - secondDone;
    })
    .slice(0, 8);
}

function isFinishedIntake(status: string) {
  const normalized = status.replace(/\s/g, "").toLowerCase();
  return normalized === "approved" || normalized === "convertedtocase";
}
