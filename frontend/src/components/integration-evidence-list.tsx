import type { DemoStoryIntegration } from "@/lib/demo-story";

type IntegrationEvidenceListProps = {
  integrations: DemoStoryIntegration[];
};

const modeLabels: Record<string, string> = {
  implemented: "Implementert",
  "scenario-source": "Fiktiv scenariokilde",
  "public-data-capable": "Offentlig datakilde / lagret demoøyeblikk",
  "demo-adapter": "Demo-adapter",
};

const modeStyles: Record<string, string> = {
  implemented: "border-[#bddbc7] bg-[#edf8f0] text-[#24613f]",
  "scenario-source": "border-[#d8dee8] bg-[#f5f7fa] text-[#526075]",
  "public-data-capable": "border-[#bed2ec] bg-[#eef5fd] text-[#315a91]",
  "demo-adapter": "border-[#ddd1ad] bg-[#fff9e8] text-[#795d16]",
};

export function IntegrationEvidenceList({
  integrations,
}: IntegrationEvidenceListProps) {
  return (
    <section aria-labelledby="integrations-heading" className="py-10 sm:py-14">
      <div className="max-w-3xl">
        <p className="text-sm font-semibold text-[#315ea8]">Integrasjoner</p>
        <h2
          className="mt-2 text-3xl font-semibold tracking-tight text-[#172033]"
          id="integrations-heading"
        >
          Systemene kan være de samme
        </h2>
        <p className="mt-3 text-base leading-7 text-[#526075]">
          Poenget er å flytte og kontrollere informasjon mellom systemene
          bedriften allerede bruker, ikke å erstatte alt med én ny plattform.
        </p>
      </div>

      <div className="mt-7 grid gap-3">
        <IntegrationRow
          integration={{
            provider: "email",
            displayName: "Fiktiv Outlook-lignende e-post",
            mode: "scenario-source",
            status: "ScenarioSource",
            explanation: "En forhåndslastet fiktiv e-post er scenariokilden; Outlook er ikke tilkoblet.",
          }}
        />
        {integrations.map((integration) => (
          <IntegrationRow integration={integration} key={integration.provider} />
        ))}
        <IntegrationRow
          integration={{
            provider: "audit",
            displayName: "Sporbar hendelseslogg",
            mode: "implemented",
            status: "Stored",
            explanation: "Viktige handlinger lagres og kan etterprøves i demosaken.",
          }}
        />
      </div>

      <p className="mt-5 rounded-lg border border-[#ddd1ad] bg-[#fff9e8] p-4 text-sm leading-6 text-[#6f571a]">
        <strong>Demo-adapter</strong> betyr at dataflyt, status og feilhåndtering
        demonstreres uten å sende data til et ekte kundesystem.
      </p>
    </section>
  );
}

function IntegrationRow({
  integration,
}: {
  integration: DemoStoryIntegration;
}) {
  return (
    <article className="grid gap-3 rounded-lg border border-[#d8dee8] bg-white p-5 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-start">
      <div>
        <div className="flex flex-wrap items-center gap-2">
          <h3 className="text-base font-semibold text-[#243147]">
            {integration.displayName}
          </h3>
          <span className="text-xs font-medium text-[#64748b]">
            Status: {statusLabel(integration.status)}
          </span>
        </div>
        <p className="mt-2 text-sm leading-6 text-[#526075]">
          {integration.explanation}
        </p>
      </div>
      <span
        className={`w-fit rounded-full border px-2.5 py-1 text-xs font-semibold ${
          modeStyles[integration.mode] ?? modeStyles["demo-adapter"]
        }`}
      >
        {modeLabels[integration.mode] ?? integration.mode}
      </span>
    </article>
  );
}

function statusLabel(status: string) {
  const labels: Record<string, string> = {
    Connected: "Demo-klar",
    Disconnected: "Simulert – ikke tilkoblet",
    Error: "Feilstatus",
    ScenarioSource: "Scenariokilde",
    Stored: "Lagret",
  };
  return labels[status] ?? status;
}
