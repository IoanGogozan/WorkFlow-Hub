"use client";

import { useEffect, useState } from "react";

type DeploymentVersion = {
  commit: string;
  builtAt: string;
  environment: string;
  deploymentTarget: string;
};

type LoadState =
  | { status: "loading" }
  | { status: "ready"; version: DeploymentVersion }
  | { status: "unavailable" };

const repositoryUrl = "https://github.com/IoanGogozan/WorkFlow-Hub";

export function DeploymentStatusCard() {
  const [state, setState] = useState<LoadState>({ status: "loading" });

  useEffect(() => {
    const controller = new AbortController();

    fetch("/health/version", {
      cache: "no-store",
      headers: { Accept: "application/json" },
      signal: controller.signal,
    })
      .then(async (response) => {
        if (!response.ok) {
          throw new Error(`Version request failed with ${response.status}`);
        }

        return response.json() as Promise<DeploymentVersion>;
      })
      .then((version) => setState({ status: "ready", version }))
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === "AbortError") {
          return;
        }

        setState({ status: "unavailable" });
      });

    return () => controller.abort();
  }, []);

  return (
    <section aria-labelledby="deployment-status-heading" className="py-8">
      <div className="rounded-lg border border-[#d8deea] bg-white p-5 shadow-sm sm:p-6">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <p className="text-sm font-semibold text-[#315ea8]">Live deployment</p>
            <h3 className="mt-1 text-lg font-semibold text-[#162033]" id="deployment-status-heading">
              Verifiserbar versjon
            </h3>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-[#64748b]">
              Bygginformasjonen kobler den publiserte demoen til et eksakt commit i det offentlige repositoryet.
            </p>
          </div>
          <DeploymentBadge status={state.status} />
        </div>

        {state.status === "loading" ? (
          <p aria-live="polite" className="mt-5 text-sm text-[#64748b]" role="status">
            Laster versjonsinformasjon...
          </p>
        ) : null}

        {state.status === "unavailable" ? (
          <p className="mt-5 rounded-md bg-[#f8fafc] px-4 py-3 text-sm text-[#64748b]" role="status">
            Versjonsinformasjon er midlertidig utilgjengelig. Dette påvirker ikke selve demoen.
          </p>
        ) : null}

        {state.status === "ready" ? <DeploymentDetails version={state.version} /> : null}
      </div>
    </section>
  );
}

function DeploymentBadge({ status }: { status: LoadState["status"] }) {
  const label = status === "ready" ? "Ready" : status === "loading" ? "Checking" : "Unavailable";
  const style = status === "ready"
    ? "bg-[#dcfce7] text-[#166534] ring-[#86efac]"
    : status === "loading"
      ? "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]"
      : "bg-[#f1f5f9] text-[#475569] ring-[#cbd5e1]";

  return (
    <span className={`inline-flex w-fit rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ${style}`}>
      {label}
    </span>
  );
}

function DeploymentDetails({ version }: { version: DeploymentVersion }) {
  const commitIsKnown = /^[0-9a-f]{7,40}$/i.test(version.commit);
  const fields = [
    { label: "Target", value: displayValue(version.deploymentTarget) },
    { label: "Environment", value: displayValue(version.environment) },
    { label: "Built", value: formatBuildDate(version.builtAt) },
  ];

  return (
    <dl className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
      {fields.map((field) => (
        <div className="rounded-md bg-[#f8fafc] px-4 py-3" key={field.label}>
          <dt className="text-xs font-semibold uppercase tracking-[0.08em] text-[#64748b]">{field.label}</dt>
          <dd className="mt-1 break-words text-sm font-semibold text-[#243147]">{field.value}</dd>
        </div>
      ))}
      <div className="rounded-md bg-[#f8fafc] px-4 py-3">
        <dt className="text-xs font-semibold uppercase tracking-[0.08em] text-[#64748b]">Revision</dt>
        <dd className="mt-1 text-sm font-semibold text-[#243147]">
          {commitIsKnown ? (
            <a
              className="font-mono text-[#315ea8] underline decoration-[#b8ccea] underline-offset-4 hover:text-[#244a86]"
              href={`${repositoryUrl}/commit/${version.commit}`}
              rel="noreferrer"
              target="_blank"
              title={version.commit}
            >
              {version.commit.slice(0, 7)}
            </a>
          ) : (
            "Unknown"
          )}
        </dd>
      </div>
    </dl>
  );
}

function displayValue(value: string) {
  return value && value.toLowerCase() !== "unknown" ? value : "Unknown";
}

function formatBuildDate(value: string) {
  if (!value || value.toLowerCase() === "unknown") {
    return "Unknown";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "Unknown";
  }

  return new Intl.DateTimeFormat("en-GB", {
    dateStyle: "medium",
    timeStyle: "short",
    timeZone: "UTC",
  }).format(date) + " UTC";
}
