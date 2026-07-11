import { CLIENT_DEMO_CONTACT_URL } from "@/lib/site-config";

export function LiveDemoHero() {
  return (
    <section className="max-w-4xl pb-10 pt-4 sm:pb-14 sm:pt-10">
      <p className="text-sm font-semibold text-[#315ea8]">
        Live integrasjon med fiktive data
      </p>
      <h1 className="mt-4 text-4xl font-semibold leading-tight tracking-tight text-[#172033] sm:text-5xl">
        Fra henvendelse til sak og SharePoint – på sekunder
      </h1>
      <p className="mt-5 max-w-3xl text-base leading-7 text-[#526075] sm:text-lg">
        Se hvordan Norvix kan kontrollere informasjon, opprette sak og
        dokumentasjon og sende data videre mellom systemer – uten gjentatt
        registrering.
      </p>

      <div className="mt-7 flex flex-col items-stretch gap-3 sm:flex-row sm:items-center">
        <a
          className="inline-flex justify-center rounded-md bg-[#315ea8] px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-[#244a86] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
          href="#live-preview-run"
        >
          Kjør live demo
        </a>
        <a
          className="inline-flex justify-center rounded-md border border-[#9aa8bb] px-5 py-3 text-sm font-semibold text-[#172033] hover:bg-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
          href={CLIENT_DEMO_CONTACT_URL}
        >
          Beskriv prosessen deres
        </a>
      </div>

      <p className="mt-6 text-sm leading-6 text-[#64748b]">
        Fiktive data. Dette er en statisk forhåndsvisning av det planlagte,
        avgrensede demomiljøet.
      </p>
    </section>
  );
}
