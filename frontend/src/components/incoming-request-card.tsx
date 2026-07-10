import type { DemoStoryRequest } from "@/lib/demo-story";
import { formatDateTime } from "@/lib/format";

type IncomingRequestCardProps = {
  request: DemoStoryRequest;
};

export function IncomingRequestCard({ request }: IncomingRequestCardProps) {
  const businessFields = [
    { label: "Kunde", value: request.customerName },
    {
      label: "Organisasjonsnummer",
      value: request.organizationNumber ?? "Ikke oppgitt",
    },
    {
      label: "Kundereferanse",
      value: request.customerReference ?? "Ikke oppgitt",
    },
    { label: "Mottaker", value: "Driftsteamet" },
  ];

  return (
    <section aria-labelledby="incoming-request-heading" className="py-10 sm:py-14">
      <div className="max-w-3xl">
        <p className="text-sm font-semibold text-[#315ea8]">Utgangspunktet</p>
        <h2
          className="mt-2 text-3xl font-semibold tracking-tight text-[#172033]"
          id="incoming-request-heading"
        >
          En servicehenvendelse kommer på e-post
        </h2>
        <p className="mt-3 text-base leading-7 text-[#526075]">
          Informasjonen er forståelig for en ansatt, men må fortsatt flyttes og
          kontrolleres i flere systemer.
        </p>
      </div>

      <article className="mt-7 overflow-hidden rounded-lg border border-[#cfd6e1] bg-white shadow-sm">
        <div className="border-b border-[#e2e6ec] bg-[#f7f9fc] px-5 py-4 sm:px-6">
          <div className="grid gap-3 text-sm sm:grid-cols-[7rem_1fr]">
            <EmailField label="Fra" value={request.sender} />
            <EmailField label="Emne" value={request.subject} />
            <EmailField label="Mottatt" value={formatDateTime(request.receivedAt)} />
          </div>
        </div>

        <div className="px-5 py-6 sm:px-6">
          <p className="whitespace-pre-line text-[15px] leading-7 text-[#2f3b4e]">
            {request.body}
          </p>

          <div className="mt-6 border-t border-[#e2e6ec] pt-5">
            <p className="text-xs font-semibold uppercase tracking-[0.12em] text-[#64748b]">
              Vedlegg
            </p>
            {request.attachments.length > 0 ? (
              <ul className="mt-3 flex flex-wrap gap-2">
                {request.attachments.map((attachment) => (
                  <li
                    className="rounded-md border border-[#d8dee8] bg-[#f8fafc] px-3 py-2 text-sm font-medium text-[#344258]"
                    key={attachment}
                  >
                    {attachment}
                  </li>
                ))}
              </ul>
            ) : (
              <p className="mt-2 text-sm text-[#64748b]">Ingen vedlegg registrert.</p>
            )}
          </div>
        </div>
      </article>

      <div className="mt-5 rounded-lg border border-[#d8dee8] bg-[#f2f5f9] p-5 sm:p-6">
        <h3 className="text-base font-semibold text-[#243147]">
          Forretningsinformasjon i henvendelsen
        </h3>
        <dl className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {businessFields.map((field) => (
            <div key={field.label}>
              <dt className="text-xs font-semibold uppercase tracking-[0.1em] text-[#64748b]">
                {field.label}
              </dt>
              <dd className="mt-1.5 text-sm font-semibold text-[#243147]">
                {field.value}
              </dd>
            </div>
          ))}
        </dl>
        <p className="mt-5 border-t border-[#d8dee8] pt-4 text-sm leading-6 text-[#526075]">
          Ønsket resultat: opprettet sak, dokumentstruktur og oppdatert status
          for driftsteamet.
        </p>
      </div>
    </section>
  );
}

function EmailField({ label, value }: { label: string; value: string }) {
  return (
    <div className="contents">
      <span className="font-medium text-[#64748b]">{label}</span>
      <span className="font-semibold text-[#2f3b4e]">{value}</span>
    </div>
  );
}
