import { LiveDemoDetails } from "@/components/live-demo/live-demo-details";
import {
  LiveDemoStageCard,
  type LiveDemoStage,
} from "@/components/live-demo/live-demo-stage-card";
import { LiveDemoResultCard } from "@/components/live-demo/live-demo-result-card";

const previewStages: LiveDemoStage[] = [
  {
    title: "Mottatt",
    status: "Fullført",
    duration: "0,2 sek",
    provider: "Norvix WorkFlow Hub",
    summary: "Fiktiv henvendelse registrert med sporbar kjøring.",
    evidence: "RUN-LIVE-0142",
  },
  {
    title: "Kontrollert",
    status: "Fullført",
    duration: "1,1 sek",
    provider: "Brreg · forhåndsvisning",
    summary: "Firmadata vist som et eksempel på planlagt kontroll.",
    evidence: "BRREG-EXAMPLE",
  },
  {
    title: "Opprettet",
    status: "Fullført",
    duration: "3,6 sek",
    provider: "Norvix WorkFlow Hub",
    summary: "Sak og fiktiv PDF vises med stabile, interne referanser.",
    evidence: "LIVE-2026-0142",
  },
  {
    title: "Synkronisert",
    status: "Fullført",
    duration: "3,5 sek",
    provider: "SharePoint + ERP demo receiver · forhåndsvisning",
    summary: "Eksterne bevis er eksempelreferanser, ikke aktive tilkoblinger.",
    evidence: "ERP-RECEIPT-0142",
  },
];

export function LiveDemoRunPanel() {
  return (
    <section aria-labelledby="live-preview-run-heading" id="live-preview-run">
      <div className="rounded-xl border border-[#dce1e8] bg-[#fdfefe] p-5 sm:p-8">
        <p className="text-sm font-semibold text-[#315ea8]">Forhåndsvisning av resultat</p>
        <h2
          className="mt-2 text-2xl font-semibold tracking-tight text-[#172033] sm:text-3xl"
          id="live-preview-run-heading"
        >
          Én ny henvendelse, fire tydelige steg
        </h2>
        <p className="mt-3 max-w-2xl text-base leading-7 text-[#526075]">
          Fixture-data viser den planlagte presentasjonen. Ingen integrasjon,
          sak eller ekstern melding opprettes fra denne siden ennå.
        </p>
        <ol className="mt-6 grid gap-3 lg:grid-cols-2">
          {previewStages.map((stage) => (
            <LiveDemoStageCard key={stage.title} stage={stage} />
          ))}
        </ol>
        <LiveDemoResultCard />
      </div>
      <LiveDemoDetails />
    </section>
  );
}
