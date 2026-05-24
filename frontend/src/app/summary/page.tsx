import Link from "next/link";
import { AppShell } from "@/components/app-shell";

const timeline = [
  "Input mottatt fra flere kilder",
  "AI foreslo struktur",
  "Menneske godkjente data",
  "Sak ble opprettet",
  "Dokumenter ble knyttet til saken",
  "Integrasjoner ble synkronisert",
  "Leveringspakke ble opprettet",
  "Audit trail ble lagret",
];

const reducedSteps = [
  "Lese og tolke e-post",
  "Kopiere data mellom systemer",
  "Slå opp firmadata manuelt",
  "Opprette mapper og status manuelt",
  "Sende dokumenter som vedlegg",
  "Lage rapporter manuelt",
];

export default function SummaryPage() {
  return (
    <AppShell>
      <div className="mx-auto max-w-6xl px-6 py-6">
        <section className="rounded-md border border-[#d8deea] bg-white p-6 sm:p-8">
          <p className="text-sm font-semibold text-[#4f46e5]">
            Demo summary
          </p>
          <h2 className="mt-3 text-3xl font-semibold text-[#162033]">
            Du har fullført en integrert arbeidsflyt
          </h2>
          <p className="mt-4 max-w-3xl text-sm leading-6 text-[#475569]">
            Denne siden oppsummerer hva demoen viser: input blir strukturert,
            godkjent, gjort om til sak, distribuert via integrasjoner og levert
            ryddig til kunde.
          </p>
        </section>

        <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_360px]">
          <section className="rounded-md border border-[#d8deea] bg-white p-6">
            <h3 className="text-lg font-semibold text-[#162033]">
              Fullført flyt
            </h3>
            <ol className="mt-4 space-y-3">
              {timeline.map((item) => (
                <li className="flex items-start gap-3 text-sm" key={item}>
                  <span className="mt-0.5 inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-[#047857] px-1 text-[10px] font-semibold text-white">
                    OK
                  </span>
                  <span className="leading-6 text-[#475569]">{item}</span>
                </li>
              ))}
            </ol>
          </section>

          <aside className="space-y-6">
            <section className="rounded-md border border-[#d8deea] bg-white p-5">
              <h3 className="text-lg font-semibold text-[#162033]">
                Typiske manuelle steg som reduseres
              </h3>
              <ul className="mt-3 list-disc space-y-2 pl-5 text-sm leading-6 text-[#475569]">
                {reducedSteps.map((step) => (
                  <li key={step}>{step}</li>
                ))}
              </ul>
            </section>

            <section className="rounded-md border border-[#bfdbfe] bg-[#eff6ff] p-5">
              <h3 className="text-lg font-semibold text-[#162033]">
                Estimert administrasjon spart
              </h3>
              <p className="mt-3 text-3xl font-semibold text-[#1d4ed8]">
                30-45 min
              </p>
              <p className="mt-3 text-sm leading-6 text-[#475569]">
                Tidsbesparelsen varierer etter prosess, men demoen viser hvor
                copy/paste, manuell kontroll og rapportering kan reduseres.
              </p>
            </section>

            <section className="rounded-md border border-[#d8deea] bg-white p-5">
              <h3 className="text-lg font-semibold text-[#162033]">
                Har dere en lignende manuell prosess?
              </h3>
              <p className="mt-3 text-sm leading-6 text-[#475569]">
                Norvix kan bygge en integrert arbeidsflyt rundt systemene dere
                allerede bruker.
              </p>
              <Link
                className="mt-4 inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
                href="mailto:contact@norvix.no"
              >
                Kontakt Norvix
              </Link>
            </section>
          </aside>
        </div>
      </div>
    </AppShell>
  );
}
