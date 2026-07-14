import { formatDateTime } from "@/lib/format";
import type { LiveDemoEvidenceBrreg } from "@/lib/live-demo-evidence";

type BrregEvidenceCardProps = {
  brreg: LiveDemoEvidenceBrreg | null;
};

export function BrregEvidenceCard({ brreg }: BrregEvidenceCardProps) {
  const isLive = brreg?.mode.toLowerCase() === "live";

  return (
    <article className="rounded-xl border border-[#d8deea] bg-white p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h3 className="text-xl font-semibold text-[#162033]">Brreg-kontroll</h3>
        {brreg ? <ModeBadge isLive={isLive} /> : null}
      </div>

      {brreg ? (
        <>
          <dl className="mt-5 grid gap-4 sm:grid-cols-2">
            <EvidenceField label="Organisasjon" value={brreg.organizationName} />
            <EvidenceField label="Organisasjonsnummer" value={brreg.organizationNumber} mono />
            <EvidenceField label="Oppslagstid" value={formatDuration(brreg.lookupDurationMs)} />
            <EvidenceField
              label="Kildetidspunkt"
              value={brreg.sourceUpdatedAt ? formatDateTime(brreg.sourceUpdatedAt) : "Ikke oppgitt"}
            />
          </dl>
          <p
            className={`mt-5 rounded-md p-4 text-sm leading-6 ${
              isLive ? "bg-[#ecfdf5] text-[#166534]" : "bg-[#fff7ed] text-[#9a3412]"
            }`}
          >
            {isLive
              ? brreg.statusMessage
              : "Brreg var ikke tilgjengelig for denne kjøringen. Resultatet kommer fra et tydelig merket, lagret fallback-snapshot."}
          </p>
        </>
      ) : (
        <p className="mt-5 text-sm text-[#64748b]">Brreg-kontrollen er ikke gjennomført ennå.</p>
      )}
    </article>
  );
}

function ModeBadge({ isLive }: { isLive: boolean }) {
  return (
    <span
      className={`inline-flex rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ${
        isLive
          ? "bg-[#dcfce7] text-[#166534] ring-[#86efac]"
          : "bg-[#ffedd5] text-[#9a3412] ring-[#fdba74]"
      }`}
    >
      {isLive ? "Live" : "Fallback"}
    </span>
  );
}

function EvidenceField({
  label,
  value,
  mono = false,
}: {
  label: string;
  value: string;
  mono?: boolean;
}) {
  return (
    <div>
      <dt className="text-xs font-semibold uppercase tracking-wide text-[#64748b]">{label}</dt>
      <dd className={`mt-1 break-words text-sm text-[#162033] ${mono ? "font-mono" : "font-medium"}`}>
        {value}
      </dd>
    </div>
  );
}

function formatDuration(durationMs: number | null) {
  if (durationMs === null) {
    return "Ikke tilgjengelig";
  }

  return durationMs < 1000
    ? `${durationMs} ms`
    : `${new Intl.NumberFormat("nb-NO", { maximumFractionDigits: 1 }).format(durationMs / 1000)} s`;
}
