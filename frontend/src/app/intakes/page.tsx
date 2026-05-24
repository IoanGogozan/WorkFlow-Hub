"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { DemoGuidePanel } from "@/components/demo-guide-panel";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { SourceBadge } from "@/components/source-badge";
import { StatusBadge } from "@/components/status-badge";
import { WhyThisMatters } from "@/components/why-this-matters";
import { api } from "@/lib/api";
import { formatDateTime } from "@/lib/format";
import type { IntakeListItem } from "@/lib/types";

export default function IntakesPage() {
  const [intakes, setIntakes] = useState<IntakeListItem[] | null>(null);
  const [error, setError] = useState<string | null>(null);

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
        <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <p className="text-sm font-medium text-[#64748b]">
              Steg 1: Input fra flere kilder
            </p>
            <h2 className="mt-2 text-3xl font-semibold">Input sources</h2>
            <p className="mt-3 max-w-3xl text-sm leading-6 text-[#475569]">
              I en ekte løsning kan disse komme fra e-post, skjema, API,
              dokumentopplasting eller manuell registrering. I demoen er de
              forhåndsgenerert for at du raskt skal se flyten.
            </p>
          </div>
          <Link
            className="inline-flex w-fit rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
            href="/intakes/new"
          >
            Ny input
          </Link>
        </div>

        <div className="mb-6">
          <WhyThisMatters>
            <p>
              Forespørsler kommer ofte fra e-post, skjema, API eller telefon.
              Uten integrasjon må ansatte kopiere data manuelt mellom innboks,
              Excel, mapper og interne systemer.
            </p>
            <p>
              Denne delen viser hvordan flere innganger kan samles i én
              strukturert arbeidsflyt.
            </p>
          </WhyThisMatters>
        </div>

        {error ? (
          <ErrorState message={error} />
        ) : !intakes ? (
          <LoadingState label="Loading intakes" />
        ) : intakes.length === 0 ? (
          <EmptyState
            action={
              <Link
                className="inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
                href="/intakes/new"
              >
                Create intake
              </Link>
            }
                message="Opprett første forespørsel for å starte arbeidsflyten."
                title="Ingen input ennå"
          />
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1fr_340px]">
            <section className="overflow-hidden rounded-md border border-[#d8deea] bg-white">
              <div className="divide-y divide-[#e2e8f0]">
                {intakes.map((intake) => (
                  <article
                    className="grid gap-4 p-5 md:grid-cols-[1fr_180px_140px]"
                    key={intake.id}
                  >
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <SourceBadge source={intake.source} />
                        <StatusBadge status={intake.status} />
                      </div>
                      <Link
                        className="mt-2 block text-lg font-semibold text-[#162033] hover:text-[#2563eb]"
                        href={`/intakes/${intake.id}`}
                      >
                        {intake.subject}
                      </Link>
                      <p className="mt-2 text-sm text-[#64748b]">
                        {[intake.customerName, intake.category, intake.urgency]
                          .filter(Boolean)
                          .join(" - ") || "Ingen klassifisering ennå"}
                      </p>
                    </div>
                    <div className="text-sm text-[#475569]">
                      <p className="font-medium text-[#162033]">Mottatt</p>
                      <p className="mt-1">{formatDateTime(intake.receivedAt)}</p>
                    </div>
                    <div className="md:text-right">
                      <Link
                        className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                        href={`/intakes/${intake.id}`}
                      >
                        Åpne input
                      </Link>
                    </div>
                  </article>
                ))}
              </div>
            </section>

            <DemoGuidePanel
              activeStep={2}
              nextDescription="Velg en forespørsel og kjør AI-forslag. Demoen viser at AI bare foreslår struktur, mens mennesket godkjenner."
              nextHref={`/intakes/${intakes[0].id}`}
              nextLabel="Åpne input"
            />
          </div>
        )}
      </div>
    </AppShell>
  );
}
