import Link from "next/link";
import { StatusBadge } from "@/components/status-badge";
import { formatDateTime } from "@/lib/format";
import type { LiveDemoEvidenceRun } from "@/lib/live-demo-evidence";

type EvidenceOverviewProps = {
  run: LiveDemoEvidenceRun;
};

export function EvidenceOverview({ run }: EvidenceOverviewProps) {
  const details = [
    ["Kjøring", shortenIdentifier(run.runId)],
    ["Startet", run.startedAt ? formatDateTime(run.startedAt) : "Ikke startet"],
    ["Fullført", run.completedAt ? formatDateTime(run.completedAt) : "Ikke fullført"],
    ["Varighet", formatDuration(run.totalDurationMs)],
    ["Nye forsøk", run.retryCount.toString()],
    ["Korrelasjon", shortenIdentifier(run.correlationId)],
  ];

  return (
    <header className="rounded-xl border border-[#d8deea] bg-white p-6 shadow-sm sm:p-8">
      <div className="flex flex-col gap-5 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <p className="text-sm font-semibold text-[#4f46e5]">Kjøringsbevis</p>
            <span className="inline-flex rounded-md bg-[#fef3c7] px-2.5 py-1 text-xs font-semibold text-[#92400e] ring-1 ring-[#fcd34d]">
              Fiktive data
            </span>
            <StatusBadge status={run.status} />
          </div>
          <h2 className="mt-4 text-3xl font-semibold text-[#162033]">
            Verifiserbar dokumentasjon for én kjøring
          </h2>
          <p className="mt-3 max-w-3xl text-sm leading-6 text-[#475569]">
            {run.scenarioLabel}. Resultatene nedenfor tilhører bare denne kjøringen.
          </p>
        </div>
        <Link
          className="inline-flex w-fit shrink-0 rounded-md border border-[#bfdbfe] bg-[#eff6ff] px-4 py-2 text-sm font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
          href="/"
        >
          Tilbake til live-demo
        </Link>
      </div>

      <dl className="mt-7 grid gap-4 border-t border-[#e2e8f0] pt-6 sm:grid-cols-2 lg:grid-cols-3">
        {details.map(([label, value]) => (
          <div key={label}>
            <dt className="text-xs font-semibold uppercase tracking-wide text-[#64748b]">{label}</dt>
            <dd className="mt-1 break-words font-mono text-sm text-[#162033]">{value}</dd>
          </div>
        ))}
      </dl>
    </header>
  );
}

function shortenIdentifier(value: string) {
  if (value.length <= 16) {
    return value;
  }

  return `${value.slice(0, 8)}…${value.slice(-4)}`;
}

function formatDuration(durationMs: number | null) {
  if (durationMs === null) {
    return "Ikke tilgjengelig";
  }

  if (durationMs < 1000) {
    return `${durationMs} ms`;
  }

  return `${new Intl.NumberFormat("nb-NO", { maximumFractionDigits: 1 }).format(durationMs / 1000)} s`;
}
