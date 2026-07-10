import Link from "next/link";
import { CLIENT_DEMO_CONTACT_URL } from "@/lib/site-config";

export function ClientDemoCta() {
  return (
    <section aria-labelledby="client-demo-cta-heading" className="pb-10 pt-6 sm:pb-16 sm:pt-10">
      <div className="rounded-xl bg-[#172033] px-5 py-9 text-white sm:px-8 sm:py-12 lg:px-12">
        <div className="max-w-3xl">
          <p className="text-sm font-semibold text-[#9fc2ff]">Neste steg</p>
          <h2
            className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl"
            id="client-demo-cta-heading"
          >
            Har dere en lignende manuell prosess?
          </h2>
          <p className="mt-4 text-base leading-7 text-[#cbd5e1] sm:text-lg">
            Norvix kan kartlegge én avgrenset arbeidsflyt og vise hvor data kan
            overføres, kontrolleres og dokumenteres automatisk rundt systemene
            dere allerede bruker.
          </p>
        </div>

        <div className="mt-7 flex flex-col items-start gap-3 sm:flex-row sm:items-center">
          <a
            className="inline-flex rounded-md bg-white px-5 py-3 text-sm font-semibold text-[#172033] shadow-sm hover:bg-[#eef2f7] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white"
            href={CLIENT_DEMO_CONTACT_URL}
          >
            Beskriv prosessen deres
          </a>
          <Link
            className="inline-flex rounded-md border border-[#56647b] px-5 py-3 text-sm font-semibold text-white hover:bg-[#222d42] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white"
            href="/technical"
          >
            Se tekniske detaljer
          </Link>
        </div>

        <p className="mt-6 max-w-2xl text-sm leading-6 text-[#9fb0c8]">
          Et første steg kan være en kort gjennomgang av én prosess, uten
          forpliktelse til å bytte eksisterende fagsystemer.
        </p>
      </div>
    </section>
  );
}
