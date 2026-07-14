import { formatDateTime } from "@/lib/format";
import type { LiveDemoEvidenceErp } from "@/lib/live-demo-evidence";

type ErpReceiverEvidenceProps = {
  evidence: LiveDemoEvidenceErp | null;
};

export function ErpReceiverEvidence({ evidence }: ErpReceiverEvidenceProps) {
  return (
    <section
      aria-labelledby="erp-evidence-heading"
      className="rounded-xl border border-[#d8deea] bg-white p-6 shadow-sm sm:p-8"
      id="erp"
    >
      <div className="flex flex-wrap items-center gap-2">
        <h2 className="text-2xl font-semibold text-[#162033]" id="erp-evidence-heading">
          Norvix ERP demo receiver
        </h2>
        <span className="rounded-md bg-[#e0f2fe] px-2.5 py-1 text-xs font-semibold text-[#075985]">
          Selvhostet
        </span>
      </div>

      {!evidence ? (
        <p className="mt-4 text-sm leading-6 text-[#64748b]">
          ERP-mottakeren var ikke aktivert for denne kjøringen.
        </p>
      ) : (
        <>
          <p className="mt-4 text-base font-semibold text-[#166534]">
            {evidence.status === "Received" ? "Melding mottatt" : "Meldingen er ikke mottatt"}
          </p>
          <dl className="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <EvidenceValue label="Kvittering" value={evidence.externalReceiptId ?? "Ikke opprettet"} mono />
            <EvidenceValue label="Forsøk" value={evidence.attempts.toString()} />
            <EvidenceValue label="Varighet" value={formatDuration(evidence.lastDurationMs)} />
            <EvidenceValue label="Idempotensnøkkel" value={evidence.idempotencyKey ?? "Ikke tilgjengelig"} mono />
          </dl>

          {evidence.history.some((attempt) => attempt.status === "Failed") && evidence.status === "Received" ? (
            <p className="mt-5 rounded-lg border border-[#fde68a] bg-[#fffbeb] p-4 text-sm leading-6 text-[#854d0e]">
              Første forsøk feilet kontrollert. Ny kjøring fullførte uten duplikater.
            </p>
          ) : null}

          {evidence.history.length > 0 ? (
            <ol aria-label="ERP-forsøk" className="mt-5 grid gap-3">
              {evidence.history.map((attempt) => (
                <li className="rounded-lg bg-[#f8fafc] p-4 text-sm" key={`${attempt.attempt}-${attempt.timestamp}`}>
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <p className="font-semibold text-[#162033]">
                      Forsøk {attempt.attempt}: {attempt.status === "Received" ? "Mottatt" : "Feilet kontrollert"}
                    </p>
                    <time className="text-xs text-[#64748b]" dateTime={attempt.timestamp}>
                      {formatDateTime(attempt.timestamp)}
                    </time>
                  </div>
                  <p className="mt-2 leading-6 text-[#526075]">{attempt.message}</p>
                </li>
              ))}
            </ol>
          ) : null}
        </>
      )}
    </section>
  );
}

function EvidenceValue({
  label,
  mono = false,
  value,
}: {
  label: string;
  mono?: boolean;
  value: string;
}) {
  return (
    <div>
      <dt className="text-xs font-semibold uppercase tracking-wide text-[#64748b]">{label}</dt>
      <dd className={`mt-1 break-words text-sm font-semibold text-[#162033] ${mono ? "font-mono" : ""}`}>
        {value}
      </dd>
    </div>
  );
}

function formatDuration(durationMs: number | null) {
  if (durationMs === null) return "Ikke tilgjengelig";
  if (durationMs < 1000) return `${durationMs} ms`;
  return `${(durationMs / 1000).toLocaleString("nb-NO", { maximumFractionDigits: 1 })} s`;
}
