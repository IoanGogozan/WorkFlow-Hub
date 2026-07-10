export type DemoStoryRequest = {
  source: string;
  sender: string;
  subject: string;
  body: string;
  customerName: string;
  organizationNumber: string | null;
  customerReference: string | null;
  attachments: string[];
  receivedAt: string;
};

export type DemoStoryOutcome = {
  caseNumber: string;
  caseTitle: string;
  customerName: string;
  linkedDocumentCount: number;
  deliveryPackageTitle: string;
  deliveryPackageStatus: string;
  auditEventCount: number;
};

export type DemoStoryEvidenceStep = {
  key: string;
  sequence: number;
  title: string;
  description: string;
  system: string;
  evidenceMode: string;
  evidenceLabel: string;
  evidenceHref: string | null;
};

export type DemoStoryIntegration = {
  provider: string;
  displayName: string;
  mode: string;
  status: string;
  explanation: string;
};

export type DemoStoryTechnicalLinks = {
  intakeHref: string;
  caseHref: string;
  primaryDocumentHref: string | null;
  deliveryPackageHref: string | null;
  integrationsHref: string;
};

export type DemoStory = {
  scenarioKey: string;
  request: DemoStoryRequest;
  outcome: DemoStoryOutcome;
  evidenceSteps: DemoStoryEvidenceStep[];
  integrations: DemoStoryIntegration[];
  technicalLinks: DemoStoryTechnicalLinks;
};
