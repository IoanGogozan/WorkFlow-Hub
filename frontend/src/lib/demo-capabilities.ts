export type DemoCapabilityTone = "mock" | "real" | "demo";

export type DemoCapability = {
  label: string;
  detail: string;
  tone: DemoCapabilityTone;
};

export const aiCapability: DemoCapability = {
  label: "Mock AI",
  detail: "Deterministic demo suggestions. No external model call.",
  tone: "mock",
};

export const documentAiCapability: DemoCapability = {
  label: "Mock AI",
  detail: "Deterministic document classification for demo files.",
  tone: "mock",
};

const integrationCapabilities: Record<string, DemoCapability> = {
  brreg: {
    label: "Real-capable",
    detail: "Uses the Brreg-capable API path; safe demo data only.",
    tone: "real",
  },
  "microsoft-graph": {
    label: "Mock Microsoft",
    detail: "SharePoint/Microsoft Graph behavior is simulated in this demo.",
    tone: "mock",
  },
  "powerbi-fabric": {
    label: "Mock Fabric",
    detail: "Power BI/Fabric status is simulated; CSV/JSON export is functional.",
    tone: "mock",
  },
  tripletex: {
    label: "Mock accounting",
    detail: "Accounting/project sync is simulated. No Tripletex tenant is used.",
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
