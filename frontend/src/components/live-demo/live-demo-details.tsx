const details = [
  {
    title: "Slik ser den manuelle prosessen ofte ut",
    body: "Informasjon leses, kopieres mellom systemer, kontrolleres, lagres og følges opp manuelt. Live-demoen viser den samme avgrensede flyten samlet i fire steg.",
  },
  {
    title: "Mulig tidsbesparelse",
    body: "Tidseffekt avhenger av volum, datakvalitet og eksisterende systemer. Den eksisterende kalkulatoren kan brukes som en transparent eksempelberegning.",
  },
  {
    title: "Teknisk forklaring",
    body: "Den planlagte løsningen bruker tenant-avgrensede demoøkter, vedvarende steg, auditerbare bevis og retry uten dupliserte artefakter.",
  },
  {
    title: "Avgrensninger",
    body: "Dette er fiktive data. Denne forhåndsvisningen utfører ingen eksterne kall. Fremtidige SharePoint- og ERP-bevis vises bare når de er reelt gjennomført i Norvix sitt demomiljø.",
  },
];

export function LiveDemoDetails() {
  return (
    <section className="mt-6 space-y-3" aria-label="Mer om live-demoen">
      {details.map((detail) => (
        <details
          className="rounded-lg border border-[#dce1e8] bg-white px-4 py-3 text-sm text-[#526075]"
          key={detail.title}
        >
          <summary className="cursor-pointer font-semibold text-[#172033]">
            {detail.title}
          </summary>
          <p className="pt-3 leading-6">{detail.body}</p>
        </details>
      ))}
    </section>
  );
}
