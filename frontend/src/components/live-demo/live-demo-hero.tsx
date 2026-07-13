"use client";

import { CLIENT_DEMO_CONTACT_URL } from "@/lib/site-config";

type LiveDemoHeroProps = {
  disabled: boolean;
  isStarting: boolean;
  onStart: () => void;
};

export function LiveDemoHero({ disabled, isStarting, onStart }: LiveDemoHeroProps) {
  return (
    <section className="max-w-4xl pb-10 pt-4 sm:pb-14 sm:pt-10">
      <p className="text-sm font-semibold text-[#315ea8]">
        Live integrasjon med fiktive data
      </p>
      <h1 className="mt-4 text-4xl font-semibold leading-tight tracking-tight text-[#172033] sm:text-5xl">
        Fra henvendelse til sak, dokument og systemoppdatering
      </h1>
      <p className="mt-5 max-w-3xl text-base leading-7 text-[#526075] sm:text-lg">
        Se en ny kjøring bli kontrollert, opprettet og synkronisert i Norvix
        sitt selvhostede demomiljø.
      </p>

      <div className="mt-7 flex flex-col items-stretch gap-3 sm:flex-row sm:items-center">
        <button
          className="inline-flex justify-center rounded-md bg-[#315ea8] px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-[#244a86] disabled:cursor-not-allowed disabled:bg-[#8295b2] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
          disabled={disabled}
          onClick={onStart}
          type="button"
        >
          {isStarting ? "Starter live demo …" : "Kjør live demo"}
        </button>
        <a
          className="inline-flex justify-center rounded-md border border-[#9aa8bb] px-5 py-3 text-sm font-semibold text-[#172033] hover:bg-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
          href={CLIENT_DEMO_CONTACT_URL}
        >
          Beskriv prosessen deres
        </a>
      </div>

      <div className="mt-6 max-w-3xl rounded-lg border border-[#dce1e8] bg-white px-4 py-3 text-sm leading-6 text-[#526075]">
        <p>
          Brreg kan kontrolleres live. SharePoint vises i en funksjonell lokal simulator.
        </p>
        <p className="mt-1 font-medium text-[#854d0e]">
          ERP demo receiver er ikke tilgjengelig i denne versjonen ennå.
        </p>
      </div>
    </section>
  );
}
