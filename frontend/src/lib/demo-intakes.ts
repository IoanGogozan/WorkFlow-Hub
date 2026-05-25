import type { IntakeListItem } from "@/lib/types";

const demoSubjectPrefixes = /^(E-post|Skjema|API|Dokument|Manuell):/i;
const technicalSubjectPatterns = [
  /\be2e\b/i,
  /\bsmoke\b/i,
  /\bphase\s*\d+/i,
  /\btest\b/i,
  /\bplaywright\b/i,
  /\bselenium\b/i,
  /\bcleanup\b/i,
  /\bmetric intake\b/i,
  /\bai review request\b/i,
];

const sourcePriority: Record<string, number> = {
  mockemail: 1,
  email: 1,
  mockform: 2,
  webform: 2,
  api: 3,
  mockdocumentupload: 4,
  documentupload: 4,
  manual: 5,
};

const subjectLabels: Record<string, string> = {
  "service request - pump station inspection":
    "Serviceforespørsel: inspeksjon av pumpestasjon",
  "new maintenance order from field system":
    "Vedlikeholdsordre fra feltsystem",
};

const generatedSubjectLabels = [
  {
    pattern: /^new service request\b/i,
    label: "Serviceforespørsel: dokumentasjon",
  },
  {
    pattern: /^case workspace request\b/i,
    label: "Saksforespørsel: opprett arbeidsområde",
  },
];

export const demoPreviewFallback: IntakeListItem[] = [
  {
    id: "preview-email",
    source: "MockEmail",
    status: "ConvertedToCase",
    subject: "E-post: Serviceforespørsel: inspeksjon av pumpestasjon",
    customerName: "Kristiansand Kommune",
    category: "Inspection",
    urgency: "Normal",
    receivedAt: "2026-05-24T08:00:00.000Z",
    createdAt: "2026-05-24T08:00:00.000Z",
  },
  {
    id: "preview-form",
    source: "MockForm",
    status: "New",
    subject: "Skjema: Hasteforespørsel om FDV-dokumentasjon",
    customerName: "Agder Energi Drift AS",
    category: "Documentation",
    urgency: "High",
    receivedAt: "2026-05-24T09:00:00.000Z",
    createdAt: "2026-05-24T09:00:00.000Z",
  },
  {
    id: "preview-api",
    source: "Api",
    status: "New",
    subject: "API: Vedlikeholdsordre fra feltsystem",
    customerName: "Setesdal Miljøservice AS",
    category: "Maintenance",
    urgency: "Normal",
    receivedAt: "2026-05-24T10:00:00.000Z",
    createdAt: "2026-05-24T10:00:00.000Z",
  },
];

export function getDemoReadyIntakes(intakes: IntakeListItem[]) {
  return intakes
    .filter((intake) => !isTechnicalTestIntake(intake))
    .sort((first, second) => {
      const firstPriority = sourcePriority[normalizeSource(first.source)] ?? 99;
      const secondPriority = sourcePriority[normalizeSource(second.source)] ?? 99;

      if (firstPriority !== secondPriority) {
        return firstPriority - secondPriority;
      }

      return (
        new Date(second.receivedAt).getTime() -
        new Date(first.receivedAt).getTime()
      );
    });
}

export function getDemoPreviewIntakes(intakes: IntakeListItem[], count = 3) {
  const curated = getDemoReadyIntakes(intakes).filter((intake) =>
    demoSubjectPrefixes.test(intake.subject),
  );

  return (curated.length > 0 ? curated : demoPreviewFallback).slice(0, count);
}

export function cleanDemoSubject(subject: string) {
  const cleaned = subject.replace(demoSubjectPrefixes, "").trim();
  const normalized = cleaned.toLowerCase();
  const generatedLabel = generatedSubjectLabels.find(({ pattern }) =>
    pattern.test(cleaned),
  );

  return (
    subjectLabels[normalized] ??
    generatedLabel?.label ??
    capitalizeFirst(removeTechnicalSuffix(cleaned))
  );
}

function isTechnicalTestIntake(intake: IntakeListItem) {
  return technicalSubjectPatterns.some((pattern) => pattern.test(intake.subject));
}

function normalizeSource(source: string) {
  return source.replace(/\s/g, "").toLowerCase();
}

function capitalizeFirst(value: string) {
  return value.length === 0
    ? value
    : `${value.charAt(0).toUpperCase()}${value.slice(1)}`;
}

function removeTechnicalSuffix(value: string) {
  return value.replace(/\s+[0-9a-f]{12,}$/i, "").trim();
}
