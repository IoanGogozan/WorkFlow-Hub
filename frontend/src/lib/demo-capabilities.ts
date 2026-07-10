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
    label: "Simulert dokumentarkiv",
    detail: "SharePoint-lignende dokumentflyt er simulert i demoen.",
    tone: "mock",
  },
  "powerbi-fabric": {
    label: "Rapportering simulert",
    detail: "Power BI/Fabric-lignende rapportering er simulert i demoen.",
    tone: "mock",
  },
  tripletex: {
    label: "Regnskap simulert",
    detail: "Regnskap og prosjektflyt er simulert uten Tripletex-tenant.",
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
