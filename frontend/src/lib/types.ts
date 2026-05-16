export type MetricsOverview = {
  newIntakes: number;
  openCases: number;
  documentsNeedingReview: number;
  deliveryLinks: number;
  integrationFailures: number;
};

export type IntakeListItem = {
  id: string;
  source: string;
  status: string;
  subject: string;
  customerName: string | null;
  category: string | null;
  urgency: string | null;
  receivedAt: string;
  createdAt: string;
};

export type IntakeItem = IntakeListItem & {
  tenantId: string;
  body: string;
  organizationNumber: string | null;
};

export type AiIntakeSuggestion = {
  customerName: string | null;
  organizationNumber: string | null;
  category: string | null;
  urgency: string | null;
  suggestedTasks: string[];
  summary: string;
  missingInformation: string[];
  confidence: number;
};

export type AiAnalysisRun = {
  id: string;
  entityId: string;
  entityType: string;
  provider: string;
  model: string;
  promptVersion: string;
  confidence: number;
  status: string;
  suggestion: AiIntakeSuggestion;
  createdAt: string;
};

export type ReviewTask = {
  id: string;
  entityType: string;
  entityId: string;
  reviewType: string;
  status: string;
  aiAnalysisRunId: string | null;
  createdAt: string;
};

export type IntegrationConnection = {
  id: string;
  provider: string;
  displayName: string;
  status: string;
  connectedAt: string | null;
  lastSyncAt: string | null;
  lastSuccessfulSyncAt: string | null;
  lastFailedSyncAt: string | null;
  failedSyncs: number;
  lastError: string | null;
};

export type IntegrationSyncRun = {
  id: string;
  connectionId: string;
  provider: string;
  status: string;
  triggeredBy: string;
  retriedFromSyncRunId: string | null;
  startedAt: string;
  completedAt: string | null;
  itemsProcessed: number;
  errorMessage: string | null;
};

export type CaseListItem = {
  id: string;
  caseNumber: string;
  title: string;
  status: string;
  ownerUserId: string | null;
  dueDate: string | null;
  createdAt: string;
};

export type CaseDetail = CaseListItem & {
  tenantId: string;
  description: string | null;
  sourceIntakeItemId: string | null;
};

export type CaseActivity = {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  actorUserId: string | null;
  createdAt: string;
};

export type CaseTask = {
  id: string;
  caseId: string;
  title: string;
  description: string | null;
  status: string;
  dueDate: string | null;
  createdAt: string;
};

export type CaseNote = {
  id: string;
  caseId: string;
  body: string;
  visibility: string;
  createdAt: string;
};

export type DocumentRecord = {
  id: string;
  tenantId: string;
  title: string;
  status: string;
  documentType: string | null;
  currentVersionId: string | null;
  caseId: string | null;
  expiryDate: string | null;
  createdAt: string;
};

export type DocumentClassification = {
  aiAnalysisRunId: string;
  documentType: string;
  expiryDate: string | null;
  summary: string;
  confidence: number;
};

export type DeliveryPackageItem = {
  id: string;
  documentId: string;
  displayName: string;
};

export type DeliveryLink = {
  id: string;
  expiresAt: string;
  revokedAt: string | null;
  recipientEmail: string | null;
  token: string | null;
};

export type DeliveryPackage = {
  id: string;
  caseId: string;
  title: string;
  status: string;
  summaryPdfDocumentId: string | null;
  summaryGeneratedAt: string | null;
  items: DeliveryPackageItem[];
  links: DeliveryLink[];
};

export type PublicDeliveryDocument = {
  documentId: string;
  title: string;
  documentType: string | null;
};

export type PublicDeliveryPackage = {
  title: string;
  caseTitle: string;
  expiresAt: string;
  documents: PublicDeliveryDocument[];
};
