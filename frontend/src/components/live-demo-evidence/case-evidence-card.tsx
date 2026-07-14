import Link from "next/link";
import { StatusBadge } from "@/components/status-badge";
import { formatDateTime } from "@/lib/format";
import type { LiveDemoEvidenceCase } from "@/lib/live-demo-evidence";

type CaseEvidenceCardProps = {
  caseEvidence: LiveDemoEvidenceCase | null;
};

export function CaseEvidenceCard({ caseEvidence }: CaseEvidenceCardProps) {
  return (
    <article className="rounded-xl border border-[#d8deea] bg-white p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h3 className="text-xl font-semibold text-[#162033]">Opprettet sak</h3>
        {caseEvidence ? <StatusBadge status={caseEvidence.status} /> : null}
      </div>

      {caseEvidence ? (
        <>
          <dl className="mt-5 grid gap-4 sm:grid-cols-2">
            <EvidenceField label="Saksnummer" value={caseEvidence.caseNumber} mono />
            <EvidenceField label="Kunde" value={caseEvidence.customerName} />
            <EvidenceField label="Tittel" value={caseEvidence.title} />
            <EvidenceField label="Opprettet" value={formatDateTime(caseEvidence.createdAt)} />
          </dl>
          <Link
            className="mt-6 inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb] focus-visible:outline-offset-2"
            href={caseEvidence.caseHref}
          >
            Åpne saken
          </Link>
        </>
      ) : (
        <p className="mt-5 text-sm text-[#64748b]">Saken er ikke opprettet ennå.</p>
      )}
    </article>
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
