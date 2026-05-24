"use client";

import { useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import {
  DemoCapabilityBadge,
  DemoCapabilityNote,
} from "@/components/demo-capability-badge";
import { DemoGuidePanel } from "@/components/demo-guide-panel";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { IntegrationFlowMap } from "@/components/integration-flow-map";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { WhyThisMatters } from "@/components/why-this-matters";
import { api } from "@/lib/api";
import { getIntegrationCapability } from "@/lib/demo-capabilities";
import { formatDateTime } from "@/lib/format";
import type { IntegrationConnection, IntegrationSyncRun } from "@/lib/types";

async function fetchSyncRuns(
  items: IntegrationConnection[],
  signal?: AbortSignal,
) {
  const pairs = await Promise.all(
    items.map(async (integration) => [
      integration.provider,
      await api<IntegrationSyncRun[]>(
        `/api/integrations/${integration.provider}/sync-runs`,
        { signal },
      ),
    ] as const),
  );

  return Object.fromEntries(pairs) as Record<string, IntegrationSyncRun[]>;
}

export default function IntegrationsPage() {
  const [integrations, setIntegrations] = useState<IntegrationConnection[] | null>(
    null,
  );
  const [syncRuns, setSyncRuns] = useState<Record<string, IntegrationSyncRun[]>>(
    {},
  );
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [busyProvider, setBusyProvider] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadIntegrations() {
      try {
        setError(null);
        const items = await api<IntegrationConnection[]>("/api/integrations", {
          signal: controller.signal,
        });
        setIntegrations(items);
        setSyncRuns(await fetchSyncRuns(items, controller.signal));
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Integrations could not be loaded.",
          );
        }
      }
    }

    void loadIntegrations();
    return () => controller.abort();
  }, []);

  async function refresh() {
    const items = await api<IntegrationConnection[]>("/api/integrations");
    setIntegrations(items);
    setSyncRuns(await fetchSyncRuns(items));
  }

  async function runAction(
    provider: string,
    action: "connect" | "disconnect" | "sync",
    syncRunId?: string,
  ) {
    try {
      setBusyProvider(provider);
      setActionError(null);

      if (action === "connect") {
        await api(`/api/integrations/${provider}/connect`, {
          method: "POST",
          body: { settingsJson: "{}" },
        });
      } else if (action === "disconnect") {
        await api(`/api/integrations/${provider}/disconnect`, {
          method: "POST",
        });
      } else if (syncRunId) {
        await api(`/api/integrations/${provider}/sync-runs/${syncRunId}/retry`, {
          method: "POST",
        });
      } else {
        await api(`/api/integrations/${provider}/sync`, { method: "POST" });
      }

      await refresh();
    } catch (actionFailure) {
      setActionError(
        actionFailure instanceof Error
          ? actionFailure.message
          : "Integration action failed.",
      );
    } finally {
      setBusyProvider(null);
    }
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-7xl px-6 py-6">
        <div className="mb-6">
          <p className="text-sm font-medium text-[#64748b]">
            Systemer som snakker sammen
          </p>
          <h2 className="mt-2 text-3xl font-semibold">Integrasjoner</h2>
          <p className="mt-3 max-w-3xl text-sm leading-6 text-[#475569]">
            Demoen viser hvordan godkjente data kan flyte videre til
            dokumentarkiv, regnskap, rapportering, kundeportal og audit log
            uten å bruke ekte Microsoft-, regnskaps- eller Fabric-miljøer.
          </p>
        </div>

        {error ? (
          <ErrorState message={error} />
        ) : !integrations ? (
          <LoadingState label="Loading integrations" />
        ) : (
          <div className="space-y-5">
            <IntegrationFlowMap />

            <WhyThisMatters>
              <p>
                Norvix erstatter ikke systemene firmaet allerede bruker.
                Demoen viser hvordan data kan flyte videre til dokumentarkiv,
                regnskap, rapportering og kundeportal.
              </p>
            </WhyThisMatters>

            <DemoGuidePanel
              activeStep={6}
              nextDescription="Kjør en demo-sync for å se hvordan godkjente data sendes videre. Gå deretter til oppsummeringen."
              nextHref="/summary"
              nextLabel="Se oppsummering"
            />

            {actionError ? <ErrorState message={actionError} /> : null}
            {integrations.length === 0 ? (
              <EmptyState
                message="No integration connections were returned by the API."
                title="No integrations"
              />
            ) : (
              integrations.map((integration) => {
                const capability = getIntegrationCapability(
                  integration.provider,
                );
                const failedRuns = (syncRuns[integration.provider] ?? []).filter(
                  (run) => run.status.toLowerCase() === "failed",
                );

                return (
                  <section
                    className="rounded-md border border-[#d8deea] bg-white p-5"
                    key={integration.id}
                  >
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                      <div>
                        <div className="flex flex-wrap items-center gap-2">
                          <h3 className="text-xl font-semibold">
                            {integration.displayName}
                          </h3>
                          <StatusBadge status={integration.status} />
                          <DemoCapabilityBadge capability={capability} />
                        </div>
                        <p className="mt-2 text-sm text-[#64748b]">
                          Provider: {integration.provider}
                        </p>
                        <p className="mt-3 max-w-3xl text-sm leading-6 text-[#475569]">
                          {businessExplanation(integration.provider)}
                        </p>
                        {integration.lastError ? (
                          <p className="mt-2 text-sm font-medium text-[#b91c1c]">
                            {integration.lastError}
                          </p>
                        ) : null}
                      </div>
                      <div className="flex flex-wrap gap-2">
                        <button
                          className="rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm font-semibold text-[#334155] hover:bg-[#eef2ff] disabled:cursor-not-allowed disabled:opacity-60"
                          disabled={busyProvider === integration.provider}
                          onClick={() =>
                            runAction(integration.provider, "connect")
                          }
                          type="button"
                        >
                          Connect
                        </button>
                        <button
                          className="rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm font-semibold text-[#334155] hover:bg-[#eef2ff] disabled:cursor-not-allowed disabled:opacity-60"
                          disabled={busyProvider === integration.provider}
                          onClick={() => runAction(integration.provider, "sync")}
                          type="button"
                        >
                          Sync
                        </button>
                        <button
                          className="rounded-md border border-[#fca5a5] bg-[#fef2f2] px-3 py-2 text-sm font-semibold text-[#b91c1c] hover:bg-[#fee2e2] disabled:cursor-not-allowed disabled:opacity-60"
                          disabled={busyProvider === integration.provider}
                          onClick={() =>
                            runAction(integration.provider, "disconnect")
                          }
                          type="button"
                        >
                          Disconnect
                        </button>
                      </div>
                    </div>

                    <div className="mt-5">
                      <DemoCapabilityNote capability={capability} />
                    </div>

                    <dl className="mt-5 grid gap-4 text-sm sm:grid-cols-4">
                      <IntegrationValue
                        label="Connected"
                        value={formatDateTime(integration.connectedAt)}
                      />
                      <IntegrationValue
                        label="Last sync"
                        value={formatDateTime(integration.lastSyncAt)}
                      />
                      <IntegrationValue
                        label="Last success"
                        value={formatDateTime(integration.lastSuccessfulSyncAt)}
                      />
                      <IntegrationValue
                        label="Failed syncs"
                        value={String(integration.failedSyncs)}
                      />
                    </dl>

                    {failedRuns.length > 0 ? (
                      <div className="mt-5 rounded-md border border-[#fecaca] bg-[#fef2f2] p-4">
                        <p className="text-sm font-semibold text-[#991b1b]">
                          Failed runs
                        </p>
                        <div className="mt-3 space-y-3">
                          {failedRuns.slice(0, 3).map((run) => (
                            <div
                              className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"
                              key={run.id}
                            >
                              <p className="text-sm text-[#7f1d1d]">
                                {formatDateTime(run.startedAt)} ·{" "}
                                {run.errorMessage ?? "No error message"}
                              </p>
                              <button
                                className="rounded-md bg-[#b91c1c] px-3 py-1.5 text-xs font-semibold text-white hover:bg-[#991b1b] disabled:cursor-not-allowed disabled:opacity-60"
                                disabled={busyProvider === integration.provider}
                                onClick={() =>
                                  runAction(
                                    integration.provider,
                                    "sync",
                                    run.id,
                                  )
                                }
                                type="button"
                              >
                                Retry
                              </button>
                            </div>
                          ))}
                        </div>
                      </div>
                    ) : null}
                  </section>
                );
              })
            )}
          </div>
        )}
      </div>
    </AppShell>
  );
}

function IntegrationValue({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="font-semibold text-[#334155]">{label}</dt>
      <dd className="mt-1 text-[#64748b]">{value}</dd>
    </div>
  );
}

function businessExplanation(provider: string) {
  const normalized = provider.toLowerCase();

  if (normalized.includes("brreg")) {
    return "Henter eller validerer firmadata basert på org.nr. og erstatter manuelt oppslag og copy/paste.";
  }

  if (normalized.includes("sharepoint") || normalized.includes("document")) {
    return "Oppretter struktur for dokumenter og metadata og erstatter manuell mappehåndtering.";
  }

  if (normalized.includes("accounting") || normalized.includes("erp")) {
    return "Forbereder fakturagrunnlag og erstatter manuell overføring av kunde, referanse og leveransedata.";
  }

  if (normalized.includes("fabric") || normalized.includes("power")) {
    return "Oppdaterer rapportering automatisk og erstatter manuell statusrapportering.";
  }

  return "Sender godkjente data videre til et tilknyttet system og reduserer manuell kopiering mellom verktøy.";
}
