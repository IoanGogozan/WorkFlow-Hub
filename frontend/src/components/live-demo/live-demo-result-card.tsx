import { CLIENT_DEMO_CONTACT_URL } from "@/lib/site-config";

export function LiveDemoResultCard() {
  return (
    <section
      aria-labelledby="live-demo-result-heading"
      className="mt-6 rounded-xl bg-[#172033] px-5 py-7 text-white sm:px-8 sm:py-9"
    >
      <p className="text-sm font-semibold text-[#9fc2ff]">Statisk resultatvisning</p>
      <h2
        className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl"
        id="live-demo-result-heading"
      >
        Fullført på 8,4 sekunder
      </h2>
      <ul className="mt-5 grid gap-3 text-sm leading-6 text-[#e5edf9] sm:grid-cols-2">
        <li>✓ Firmadata kontrollert i Brreg</li>
        <li>✓ Sak LIVE-2026-0142 opprettet</li>
        <li>✓ Fiktiv PDF opprettet og lagret</li>
        <li>✓ SharePoint-dokument referert</li>
        <li>✓ ERP demo receiver kvitterte</li>
        <li>✓ 7 hendelser lagret i loggen</li>
      </ul>
      <a
        className="mt-7 inline-flex rounded-md bg-white px-5 py-3 text-sm font-semibold text-[#172033] hover:bg-[#eef2f7] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white"
        href={CLIENT_DEMO_CONTACT_URL}
      >
        Beskriv prosessen deres
      </a>
    </section>
  );
}
