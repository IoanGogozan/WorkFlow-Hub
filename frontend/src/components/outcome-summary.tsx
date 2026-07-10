import type { DemoStoryOutcome } from "@/lib/demo-story";

type OutcomeSummaryProps = {
  outcome: DemoStoryOutcome;
};

const comparisonRows = [
  ["8–9 manuelle handlinger", "1 kontrollpunkt"],
  ["Kopiering mellom systemer", "Data gjenbrukes"],
  ["Dokumenter flyttes manuelt", "Dokumenter knyttes til saken"],
  ["Status oppdateres flere steder", "Statusgrunnlaget samles"],
  ["Vanskelig å se hvem som gjorde hva", "Sporbar hendelseslogg"],
];

export function OutcomeSummary({ outcome }: OutcomeSummaryProps) {
  const resultItems = [
    { label: "Sak", value: outcome.caseNumber },
    { label: "Kunde", value: outcome.customerName },
    {
      label: "Dokumenter",
      value: `${outcome.linkedDocumentCount} knyttet til saken`,
    },
    {
      label: "Leveringsgrunnlag",
      value: packageStatusLabel(outcome.deliveryPackageStatus),
    },
    { label: "Sporbarhet", value: `${outcome.auditEventCount} hendelser` },
  ];

  return (
    <section
      aria-labelledby="outcome-heading"
      className="scroll-mt-6 py-10 sm:py-14"
      id="resultat"
    >
      <div className="rounded-xl border border-[#cddfd3] bg-[#f3faf5] p-5 sm:p-8">
        <p className="text-sm font-semibold text-[#24613f]">Fullført arbeidsflyt</p>
        <h2
          className="mt-2 text-3xl font-semibold tracking-tight text-[#172033]"
          id="outcome-heading"
        >
          Resultatet
        </h2>
        <p className="mt-3 max-w-3xl text-base leading-7 text-[#526075]">
          Henvendelsen er gjort om til et samlet og kontrollerbart
          arbeidsgrunnlag. Tallene nedenfor kommer fra den fiktive demosaken.
        </p>

        <dl className="mt-7 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          {resultItems.map((item) => (
            <div
              className="rounded-lg border border-[#d7e6dc] bg-white p-4"
              key={item.label}
            >
              <dt className="text-xs font-semibold uppercase tracking-[0.1em] text-[#64748b]">
                {item.label}
              </dt>
              <dd className="mt-2 text-sm font-semibold leading-6 text-[#243147]">
                {item.value}
              </dd>
            </div>
          ))}
        </dl>

        <div className="mt-5 rounded-lg border border-[#d7e6dc] bg-white p-5">
          <p className="text-sm font-semibold text-[#243147]">
            Menneskelig kontrollpunkt
          </p>
          <p className="mt-2 text-sm leading-6 text-[#526075]">
            En ansatt kontrollerer at kunde, referanse og leveringsmottaker er
            riktige før informasjon sendes videre. Automatiseringen reduserer
            gjentatt registrering, men fjerner ikke faglig ansvar.
          </p>
        </div>
      </div>

      <div className="mt-8 overflow-hidden rounded-xl border border-[#d8dee8] bg-white">
        <div className="border-b border-[#d8dee8] px-5 py-4 sm:px-6">
          <h3 className="text-xl font-semibold text-[#243147]">Før og etter</h3>
        </div>
        <div className="hidden grid-cols-2 border-b border-[#d8dee8] bg-[#f5f7fa] text-sm font-semibold text-[#344258] sm:grid">
          <div className="border-r border-[#d8dee8] px-4 py-3 sm:px-6">Før</div>
          <div className="px-4 py-3 sm:px-6">Etter</div>
        </div>
        <div>
          {comparisonRows.map(([before, after]) => (
            <div
              className="grid border-b border-[#e6e9ee] text-sm last:border-b-0 sm:grid-cols-2"
              key={before}
            >
              <p className="border-b border-[#e6e9ee] px-4 py-4 leading-6 text-[#64748b] sm:border-b-0 sm:border-r sm:px-6">
                <span className="mb-1 block text-xs font-semibold uppercase tracking-[0.1em] text-[#64748b] sm:hidden">
                  Før
                </span>
                {before}
              </p>
              <p className="px-4 py-4 font-medium leading-6 text-[#2f4c3a] sm:px-6">
                <span className="mb-1 block text-xs font-semibold uppercase tracking-[0.1em] text-[#4f6f5b] sm:hidden">
                  Etter
                </span>
                {after}
              </p>
            </div>
          ))}
        </div>
      </div>

      <p className="mt-4 text-sm leading-6 text-[#64748b]">
        Sammenligningen beskriver arbeidsmåten i demoeksempelet. Den lover ikke
        at alle feil eller manuelle vurderinger forsvinner i en reell prosess.
      </p>
    </section>
  );
}

function packageStatusLabel(status: string) {
  const labels: Record<string, string> = {
    Draft: "Utkast",
    Ready: "Klar",
    Delivered: "Levert",
  };
  return labels[status] ?? status;
}
