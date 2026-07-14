"use client";

import { CLIENT_DEMO_CONTACT_URL } from "@/lib/site-config";
import type { LiveDemoCapabilities } from "@/lib/live-demo";

type LiveDemoHeroProps = {
  capabilities: LiveDemoCapabilities | null;
  disabled: boolean;
  isStarting: boolean;
  onStart: () => void;
};

export function LiveDemoHero({ capabilities, disabled, isStarting, onStart }: LiveDemoHeroProps) {
  const trustLines = capabilities ? createTrustLines(capabilities) : [];
  return (
    <section className="max-w-4xl pb-10 pt-4 sm:pb-14 sm:pt-10">
      <p className="text-sm font-semibold text-[#315ea8]">
        Live integrasjon med fiktive data
      </p>
      <h1 className="mt-4 text-4xl font-semibold leading-tight tracking-tight text-[#172033] sm:text-5xl">
        Fra henvendelse til sak, dokument og systemsynkronisering
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

      <p className="mt-4 max-w-3xl text-sm leading-6 text-[#526075]">
        Brreg kontrolleres live når tjenesten er tilgjengelig. Dokumentflyten
        vises i en lokal SharePoint-simulator.
      </p>

      {trustLines.length > 0 ? (
        <div
          aria-label="Aktive demo-integrasjoner"
          className="mt-6 max-w-3xl rounded-lg border border-[#dce1e8] bg-white px-4 py-3 text-sm leading-6 text-[#526075]"
        >
          {trustLines.map((line) => <p key={line}>{line}</p>)}
        </div>
      ) : null}
    </section>
  );
}

function createTrustLines(capabilities: LiveDemoCapabilities) {
  return [
    capabilities.brregLiveEnabled ? "Brreg: live ved tilgjengelig tjeneste" : null,
    capabilities.sharePointSimulatorEnabled ? "SharePoint: lokal simulator" : null,
    capabilities.erpReceiverEnabled ? "ERP: separat selvhostet demo receiver" : null,
  ].filter((line): line is string => line !== null);
}
