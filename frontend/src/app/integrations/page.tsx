"use client";

import { useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import {
  DemoCapabilityBadge,
} from "@/components/demo-capability-badge";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { IntegrationFlowMap } from "@/components/integration-flow-map";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
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
              : "Integrasjoner kunne ikke lastes.",
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
        await api(`/api/integrations/${provider}/connect`, {
          method: "POST",
          body: { settingsJson: "{}" },
        });
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
          <div className="mt-5">
              <WorkflowProgress activeStep={4} />
          </div>
        </div>

        {error ? (
          <ErrorState message={error} />
        ) : !integrations ? (
          <LoadingState label="Laster integrasjoner" />
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

            {actionError ? <ErrorState message={actionError} /> : null}
            {integrations.length === 0 ? (
              <EmptyState
                message="API-et returnerte ingen integrasjoner."
                title="Ingen integrasjoner"
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
                        <div className="mt-4 grid gap-4 text-sm md:grid-cols-2">
                          <BenefitBlock
                            label="Hva demonstreres"
                            value={demonstratedValue(integration.provider)}
                          />
                          <BenefitBlock
                            label="Manuell jobb som reduseres"
                            value={reducedManualWork(integration.provider)}
                          />
                        </div>
                        {integration.lastError ? (
                          <p className="mt-2 text-sm font-medium text-[#b91c1c]">
                            {integration.lastError}
                          </p>
                        ) : null}
                      </div>
                      <button
                        className="rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
                        disabled={busyProvider === integration.provider}
                        onClick={() => runAction(integration.provider, "sync")}
                        type="button"
                      >
                        {busyProvider === integration.provider
                          ? "Simulerer..."
                          : "Simuler dataflyt"}
                      </button>
                    </div>

                    <dl className="mt-5 grid gap-4 text-sm sm:grid-cols-3">
                      <IntegrationValue
                        label="Demo-status"
                        value={demoReadiness(integration.provider)}
                      />
                      <IntegrationValue
                        label="Sist simulert"
                        value={formatDateTime(integration.lastSyncAt)}
                      />
                      <IntegrationValue
                        label="Sist fullført"
                        value={formatDateTime(integration.lastSuccessfulSyncAt)}
                      />
                    </dl>

                    {failedRuns.length > 0 ? (
                      <div className="mt-5 rounded-md border border-[#fecaca] bg-[#fef2f2] p-4">
                        <p className="text-sm font-semibold text-[#991b1b]">
                          Feilede simuleringer
                        </p>
                        <div className="mt-3 space-y-3">
                          {failedRuns.slice(0, 3).map((run) => (
                            <div
                              className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"
                              key={run.id}
                            >
                              <p className="text-sm text-[#7f1d1d]">
                                {formatDateTime(run.startedAt)} ·{" "}
                                {run.errorMessage ?? "Ingen feilmelding"}
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
                                Prøv igjen
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

function BenefitBlock({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="font-semibold text-[#334155]">{label}</p>
      <p className="mt-1 leading-6 text-[#475569]">{value}</p>
    </div>
  );
}

function demonstratedValue(provider: string) {
  const normalized = provider.toLowerCase();

  if (normalized.includes("brreg")) {
    return "Validerer firmadata basert på organisasjonsnummer.";
  }

  if (normalized.includes("microsoft") || normalized.includes("sharepoint")) {
    return "Oppretter dokumentstruktur og metadata i SharePoint-lignende arkiv.";
  }

  if (normalized.includes("tripletex")) {
    return "Forbereder kunde-, prosjekt- og fakturagrunnlag for regnskap.";
  }

  if (normalized.includes("fabric") || normalized.includes("power")) {
    return "Oppdaterer rapporteringsgrunnlag når saken og leveransen endres.";
  }

  return "Sender godkjente data videre til et tilknyttet system.";
}

function reducedManualWork(provider: string) {
  const normalized = provider.toLowerCase();

  if (normalized.includes("brreg")) {
    return "Slå opp firma, kopiere navn og kontrollere organisasjonsnummer manuelt.";
  }

  if (normalized.includes("microsoft") || normalized.includes("sharepoint")) {
    return "Opprette mapper, navngi filer og kopiere metadata manuelt.";
  }

  if (normalized.includes("tripletex")) {
    return "Kopiere kunde, referanse og leveransedata til økonomisystem.";
  }

  if (normalized.includes("fabric") || normalized.includes("power")) {
    return "Lage statusrapporter og oppdatere tall manuelt.";
  }

  return "Kopiere data mellom verktøy.";
}

function demoReadiness(provider: string) {
  return provider.toLowerCase() === "brreg" ? "Demo-klar" : "Simulert";
}
