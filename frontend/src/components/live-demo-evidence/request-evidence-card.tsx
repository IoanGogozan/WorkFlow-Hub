import { formatDateTime } from "@/lib/format";
import type { LiveDemoEvidenceRequest } from "@/lib/live-demo-evidence";

type RequestEvidenceCardProps = {
  request: LiveDemoEvidenceRequest | null;
};

export function RequestEvidenceCard({ request }: RequestEvidenceCardProps) {
  return (
    <article className="rounded-xl border border-[#d8deea] bg-white p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h3 className="text-xl font-semibold text-[#162033]">Henvendelse</h3>
        <span className="inline-flex rounded-md bg-[#fef3c7] px-2.5 py-1 text-xs font-semibold text-[#92400e] ring-1 ring-[#fcd34d]">
          Fiktiv henvendelse
        </span>
      </div>

      {request ? (
        <>
          <dl className="mt-5 grid gap-4 sm:grid-cols-2">
            <EvidenceField label="Tittel" value={request.title} />
            <EvidenceField label="Kundereferanse" value={request.customerReference} mono />
            <EvidenceField label="Kilde" value={request.sourceLabel} />
            <EvidenceField label="Registrert" value={formatDateTime(request.createdAt)} />
          </dl>
          <div className="mt-5 border-t border-[#e2e8f0] pt-5">
            <p className="text-xs font-semibold uppercase tracking-wide text-[#64748b]">Innhold</p>
            <p className="mt-2 line-clamp-4 text-sm leading-6 text-[#334155]">{request.body}</p>
          </div>
        </>
      ) : (
        <p className="mt-5 text-sm text-[#64748b]">Henvendelsen er ikke registrert ennå.</p>
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
