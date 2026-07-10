const manualSteps = [
  "Lese og tolke e-posten.",
  "Kopiere kundeinformasjon og kundereferanse.",
  "Kontrollere virksomheten i Brreg.",
  "Opprette sak eller prosjekt i det interne systemet.",
  "Opprette riktig mappe- og dokumentstruktur.",
  "Lagre, navngi og plassere vedleggene.",
  "Oppdatere status og rapporteringsgrunnlag.",
  "Forberede bekreftelse eller leveringsgrunnlag.",
  "Registrere hva som ble gjort og av hvem.",
];

export function ManualProcessPanel() {
  return (
    <section aria-labelledby="manual-process-heading" className="py-10 sm:py-14">
      <div className="rounded-xl bg-[#172033] px-5 py-8 text-white sm:px-8 sm:py-10">
        <div className="grid gap-8 lg:grid-cols-[minmax(0,0.8fr)_minmax(0,1.2fr)] lg:gap-12">
          <div>
            <p className="text-sm font-semibold text-[#9fc2ff]">Før automatisering</p>
            <h2
              className="mt-2 text-3xl font-semibold tracking-tight"
              id="manual-process-heading"
            >
              Slik gjøres det ofte manuelt
            </h2>
            <p className="mt-4 text-base leading-7 text-[#cbd5e1]">
              Hver handling er liten, men samlet skaper de avbrudd, gjentatt
              registrering og risiko for at informasjon havner feil.
            </p>

            <div className="mt-7 rounded-lg border border-[#43516a] bg-[#222d42] p-5">
              <p className="text-xs font-semibold uppercase tracking-[0.12em] text-[#9fb0c8]">
                Eksempel på manuelt tidsbruk
              </p>
              <p className="mt-2 text-3xl font-semibold">12–20 minutter</p>
              <p className="mt-2 text-sm text-[#cbd5e1]">per henvendelse</p>
            </div>
          </div>

          <ol className="grid gap-3" aria-label="Manuelle handlinger">
            {manualSteps.map((step, index) => (
              <li
                className="flex gap-4 rounded-lg border border-[#3b4960] bg-[#222d42] px-4 py-3.5"
                key={step}
              >
                <span
                  aria-hidden="true"
                  className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-[#315ea8] text-xs font-semibold"
                >
                  {index + 1}
                </span>
                <span className="pt-0.5 text-sm leading-6 text-[#e2e8f0]">{step}</span>
              </li>
            ))}
          </ol>
        </div>

        <p className="mt-8 border-t border-[#43516a] pt-5 text-sm leading-6 text-[#cbd5e1]">
          Eksempelanslag basert på en typisk manuell prosess. Faktisk tidsbruk og
          mulig reduksjon må måles i kundens egen, avgrensede pilot.
        </p>
      </div>
    </section>
  );
}
