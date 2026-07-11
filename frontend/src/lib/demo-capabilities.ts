export type DemoCapabilityTone = "mock" | "real" | "demo";

export type DemoCapability = {
  label: string;
  detail: string;
  tone: DemoCapabilityTone;
};

export const aiCapability: DemoCapability = {
  label: "Valgfri AI-støtte",
  detail: "Forutsigbare demoforslag uten ekstern modellkall.",
  tone: "mock",
};

export const documentAiCapability: DemoCapability = {
  label: "Valgfri AI-støtte",
  detail: "Forutsigbar dokumentklassifisering for demofiler.",
  tone: "mock",
};

const integrationCapabilities: Record<string, DemoCapability> = {
  brreg: {
    label: "Kan kobles mot ekte system",
    detail: "Bruker API-stien som kan kobles mot Brønnøysundregistrene.",
    tone: "real",
  },
  "microsoft-graph": {
    label: "Simulert – ikke tilkoblet",
    detail: "SharePoint-lignende dokumentflyt vises uten Microsoft Graph-tilkobling.",
    tone: "mock",
  },
  "powerbi-fabric": {
    label: "Simulert – ikke tilkoblet",
    detail: "Power BI/Fabric-lignende rapportering vises uten ekstern tilkobling.",
    tone: "mock",
  },
  tripletex: {
    label: "Simulert – ikke tilkoblet",
    detail: "Regnskap og prosjektflyt vises uten Tripletex-tenant eller ekstern tilkobling.",
    tone: "mock",
  },
};

export function getIntegrationCapability(provider: string): DemoCapability {
  return (
    integrationCapabilities[provider] ?? {
      label: "Demo integration",
      detail: "Demo-safe integration status.",
      tone: "demo",
    }
  );
}

export function capabilityToneClasses(tone: DemoCapabilityTone) {
  switch (tone) {
    case "real":
      return "border-[#bbf7d0] bg-[#f0fdf4] text-[#166534]";
    case "mock":
      return "border-[#fde68a] bg-[#fffbeb] text-[#92400e]";
    default:
      return "border-[#bfdbfe] bg-[#eff6ff] text-[#1d4ed8]";
  }
}
