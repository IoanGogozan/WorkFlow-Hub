"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { DemoCapabilityBadge } from "@/components/demo-capability-badge";
import { DemoGuidePanel } from "@/components/demo-guide-panel";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { SourceBadge } from "@/components/source-badge";
import { StatusBadge } from "@/components/status-badge";
import { WhyThisMatters } from "@/components/why-this-matters";
import { api } from "@/lib/api";
import {
  aiCapability,
  getIntegrationCapability,
} from "@/lib/demo-capabilities";
import type {
  IntakeListItem,
  IntegrationConnection,
  MetricsOverview,
  ReviewTask,
} from "@/lib/types";

type DashboardData = {
  metrics: MetricsOverview;
  intakes: IntakeListItem[];
  integrations: IntegrationConnection[];
  reviewTasks: ReviewTask[];
};

const metricConfig = [
  {
    key: "newIntakes",
    label: "Nye input",
    tone: "border-l-[#2563eb]",
  },
  {
    key: "openCases",
    label: "Åpne saker",
    tone: "border-l-[#047857]",
  },
  {
    key: "documentsNeedingReview",
    label: "Dokumenter til kontroll",
    tone: "border-l-[#b45309]",
  },
  {
    key: "integrationFailures",
    label: "Integrasjonsfeil",
    tone: "border-l-[#be123c]",
  },
] satisfies {
  key: keyof MetricsOverview;
  label: string;
  tone: string;
}[];

export default function Home() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [syncingProvider, setSyncingProvider] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadDashboard() {
      try {
        setError(null);
        const [metrics, intakes, integrations, reviewTasks] = await Promise.all([
          api<MetricsOverview>("/api/metrics/overview", {
            signal: controller.signal,
          }),
          api<IntakeListItem[]>("/api/intakes", { signal: controller.signal }),
          api<IntegrationConnection[]>("/api/integrations", {
            signal: controller.signal,
          }),
          api<ReviewTask[]>("/api/review-tasks", { signal: controller.signal }),
        ]);

        setData({ metrics, intakes, integrations, reviewTasks });
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Dashboard data could not be loaded.",
          );
        }
      }
    }

    void loadDashboard();

    return () => controller.abort();
  }, []);

  async function syncIntegration(provider: string) {
    try {
      setActionError(null);
      setSyncingProvider(provider);
      await api(`/api/integrations/${provider}/sync`, { method: "POST" });
      const integrations = await api<IntegrationConnection[]>("/api/integrations");
      setData((current) => (current ? { ...current, integrations } : current));
    } catch (syncError) {
      setActionError(
        syncError instanceof Error
          ? syncError.message
          : "Integration sync could not be started.",
      );
    } finally {
      setSyncingProvider(null);
    }
  }

  return (
    <AppShell>
      <div className="mx-auto grid max-w-7xl gap-6 px-6 py-6 lg:grid-cols-[1fr_340px]">
        {error ? (
          <div className="lg:col-span-2">
            <ErrorState message={error} />
          </div>
        ) : !data ? (
          <div className="lg:col-span-2">
            <LoadingState label="Loading dashboard" />
          </div>
        ) : (
          <DashboardContent
            actionError={actionError}
            data={data}
            onSyncIntegration={syncIntegration}
            syncingProvider={syncingProvider}
          />
        )}
      </div>
    </AppShell>
  );
}

type DashboardContentProps = {
  data: DashboardData;
  actionError: string | null;
  syncingProvider: string | null;
  onSyncIntegration: (provider: string) => void;
};

function DashboardContent({
  data,
  actionError,
  syncingProvider,
  onSyncIntegration,
}: DashboardContentProps) {
  const latestIntakes = useMemo(() => data.intakes.slice(0, 5), [data.intakes]);
  const pendingReviews = data.reviewTasks.filter(
    (task) => task.status.toLowerCase() !== "approved",
  );

  return (
    <>
      <section aria-labelledby="dashboard-heading" className="space-y-6">
        <div>
          <p className="text-sm font-medium text-[#64748b]">
            Fra e-post og skjema til sak, dokumentasjon og rapportering
          </p>
          <div className="mt-2 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <h2
              id="dashboard-heading"
              className="text-3xl font-semibold text-[#162033]"
            >
              Demooversikt
            </h2>
            <Link
              className="inline-flex w-fit rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
              href="/intakes/new"
            >
              Ny input
            </Link>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {metricConfig.map((metric) => (
            <article
              key={metric.key}
              className={`border-l-4 ${metric.tone} rounded-md border-y border-r border-[#d8deea] bg-white p-4`}
            >
              <p className="text-sm font-medium text-[#64748b]">
                {metric.label}
              </p>
              <p className="mt-3 text-3xl font-semibold text-[#162033]">
                {data.metrics[metric.key]}
              </p>
            </article>
          ))}
        </div>

        <section
          aria-labelledby="intake-heading"
          className="rounded-md border border-[#d8deea] bg-white"
        >
          <div className="flex items-center justify-between gap-3 border-b border-[#d8deea] px-5 py-4">
            <h3 id="intake-heading" className="text-lg font-semibold">
              Input sources
            </h3>
            <Link
              className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
              href="/intakes"
            >
              Se alle
            </Link>
          </div>
          {latestIntakes.length === 0 ? (
            <div className="p-5">
              <EmptyState
                action={
                  <Link
                    className="inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
                    href="/intakes/new"
                  >
                    Opprett input
                  </Link>
                }
                message="Opprett første forespørsel for å starte demo-flyten."
                title="Ingen input ennå"
              />
            </div>
          ) : (
            <div className="divide-y divide-[#e2e8f0]">
              {latestIntakes.map((item) => (
                <article
                  key={item.id}
                  className="grid gap-3 p-5 md:grid-cols-4"
                >
                  <div>
                    <SourceBadge source={item.source} />
                    <Link
                      className="mt-1 block font-semibold text-[#162033] hover:text-[#2563eb]"
                      href={`/intakes/${item.id}`}
                    >
                      {item.subject}
                    </Link>
                  </div>
                  <p className="text-sm leading-6 text-[#475569] md:col-span-2">
                    {buildIntakeDetail(item)}
                  </p>
                  <div className="md:text-right">
                    <StatusBadge status={item.status} />
                  </div>
                </article>
              ))}
            </div>
          )}
        </section>

        <WhyThisMatters>
          <p>
            Demooversikten viser hvordan forespørsler, AI-forslag,
            menneskelig godkjenning, dokumenter og integrasjoner henger sammen
            i samme arbeidsflyt.
          </p>
          <p>
            Målet er å vise hvor manuell kopiering, statusoppfølging og
            rapportering kan reduseres uten å bytte ut alle systemene rundt.
          </p>
        </WhyThisMatters>
      </section>

      <aside className="space-y-6" aria-label="Operational side panel">
        <DemoGuidePanel
          activeStep={2}
          nextDescription="Åpne en forespørsel fra innboksen og kjør AI-forslag for å se hvordan input blir strukturert."
          nextHref="/intakes"
          nextLabel="Åpne input"
        />

        <section className="rounded-md border border-[#d8deea] bg-white p-5">
          <div className="flex flex-wrap items-center gap-2">
            <h3 className="text-lg font-semibold">
              AI-forslag og godkjenning
            </h3>
            <DemoCapabilityBadge capability={aiCapability} />
          </div>
          <p className="mt-3 text-sm leading-6 text-[#475569]">
            {pendingReviews.length === 0
              ? "Ingen AI-forslag venter på godkjenning akkurat nå."
              : `${pendingReviews.length} forslag venter på menneskelig godkjenning.`}
          </p>
          <Link
            className="mt-4 inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
            href="/intakes"
          >
            Åpne review
          </Link>
        </section>

        <section className="rounded-md border border-[#d8deea] bg-white">
          <div className="flex items-center justify-between gap-3 border-b border-[#d8deea] px-5 py-4">
            <h3 className="text-lg font-semibold">Integrasjoner</h3>
            <Link
              className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
              href="/integrations"
            >
              Administrer
            </Link>
          </div>
          {actionError ? (
            <div className="border-b border-[#fecaca] bg-[#fef2f2] px-5 py-3 text-sm text-[#991b1b]">
              {actionError}
            </div>
          ) : null}
          {data.integrations.length === 0 ? (
            <div className="p-5">
              <EmptyState
                message="No integration connections were returned by the API."
                title="No integrations"
              />
            </div>
          ) : (
            <div className="divide-y divide-[#e2e8f0]">
              {data.integrations.map((integration) => (
                <div key={integration.id} className="p-5">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="font-medium">{integration.displayName}</p>
                      <div className="mt-2">
                        <DemoCapabilityBadge
                          capability={getIntegrationCapability(
                            integration.provider,
                          )}
                        />
                      </div>
                    </div>
                    <StatusBadge status={integration.status} />
                  </div>
                  <p className="mt-2 text-sm text-[#64748b]">
                    {integration.lastError ?? "Ready for configured sync runs."}
                  </p>
                  <div className="mt-3 grid grid-cols-2 gap-3 text-xs text-[#475569]">
                    <span>Last sync: {formatDate(integration.lastSyncAt)}</span>
                    <span
                      className={
                        integration.failedSyncs === 0
                          ? "text-[#166534]"
                          : "font-semibold text-[#b91c1c]"
                      }
                    >
                      Failed: {integration.failedSyncs}
                    </span>
                  </div>
                  <button
                    className="mt-3 rounded-md border border-[#cbd5e1] bg-white px-3 py-1.5 text-xs font-semibold text-[#334155] hover:bg-[#eef2ff] disabled:cursor-not-allowed disabled:opacity-60"
                    disabled={syncingProvider === integration.provider}
                    onClick={() => onSyncIntegration(integration.provider)}
                    type="button"
                  >
                    {syncingProvider === integration.provider
                      ? "Syncing..."
                      : "Run sync"}
                  </button>
                </div>
              ))}
            </div>
          )}
        </section>
      </aside>
    </>
  );
}

function buildIntakeDetail(item: IntakeListItem) {
  const parts = [item.customerName, item.category, item.urgency].filter(Boolean);
  return parts.length > 0
    ? parts.join(" - ")
    : `Received ${formatDate(item.receivedAt)}`;
}

function formatDate(value: string | null) {
  if (!value) {
    return "Never";
  }

  return new Intl.DateTimeFormat("en", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
