"use client";

import Link from "next/link";
import { useState } from "react";
import { formatDateTime } from "@/lib/format";
import {
  openLiveDemoPdf,
  type LiveDemoEvidenceDocument,
} from "@/lib/live-demo-evidence";

type DocumentEvidenceCardProps = {
  documentEvidence: LiveDemoEvidenceDocument | null;
};

export function DocumentEvidenceCard({ documentEvidence }: DocumentEvidenceCardProps) {
  const [openingPdf, setOpeningPdf] = useState(false);
  const [openError, setOpenError] = useState(false);

  async function openPdf() {
    if (!documentEvidence || openingPdf) {
      return;
    }

    setOpeningPdf(true);
    setOpenError(false);
    try {
      await openLiveDemoPdf(documentEvidence.downloadHref);
    } catch {
      setOpenError(true);
    } finally {
      setOpeningPdf(false);
    }
  }

  return (
    <article className="rounded-xl border border-[#d8deea] bg-white p-6">
      <h3 className="text-xl font-semibold text-[#162033]">Opprettet dokument</h3>

      {documentEvidence ? (
        <>
          <dl className="mt-5 grid gap-4 sm:grid-cols-2">
            <EvidenceField label="Tittel" value={documentEvidence.title} />
            <EvidenceField label="Filnavn" value={documentEvidence.fileName} mono />
            <EvidenceField label="Størrelse" value={formatFileSize(documentEvidence.sizeBytes)} />
            <EvidenceField label="Type" value={documentEvidence.contentType} />
            <EvidenceField label="Versjon" value={documentEvidence.versionNumber.toString()} />
            <EvidenceField label="Opprettet" value={formatDateTime(documentEvidence.createdAt)} />
            <EvidenceField label="Hash" value={documentEvidence.contentHash ?? "Ikke tilgjengelig"} mono />
          </dl>
          <div className="mt-6 flex flex-wrap gap-3">
            <Link
              className="inline-flex rounded-md border border-[#bfdbfe] bg-[#eff6ff] px-4 py-2 text-sm font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
              href={documentEvidence.documentHref}
            >
              Åpne dokumentdetaljer
            </Link>
            <button
              className="inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
              disabled={openingPdf}
              onClick={openPdf}
              type="button"
            >
              {openingPdf ? "Åpner PDF …" : "Åpne demo-PDF"}
            </button>
          </div>
          {openError ? (
            <p className="mt-4 rounded-md bg-[#fef2f2] p-3 text-sm text-[#991b1b]" role="alert">
              Demo-PDF-en kunne ikke åpnes. Prøv igjen fra en aktiv demoøkt.
            </p>
          ) : null}
        </>
      ) : (
        <p className="mt-5 text-sm text-[#64748b]">Dokumentet er ikke opprettet ennå.</p>
      )}
    </article>
  );
}

function EvidenceField({
  label,
  value,
  mono = false,
}: {
  label: string;
  value: string;
  mono?: boolean;
}) {
  return (
    <div>
      <dt className="text-xs font-semibold uppercase tracking-wide text-[#64748b]">{label}</dt>
      <dd className={`mt-1 break-words text-sm text-[#162033] ${mono ? "font-mono" : "font-medium"}`}>
        {value}
      </dd>
    </div>
  );
}

function formatFileSize(sizeBytes: number) {
  if (sizeBytes < 1024) {
    return `${sizeBytes} B`;
  }

  return `${new Intl.NumberFormat("nb-NO", { maximumFractionDigits: 1 }).format(sizeBytes / 1024)} kB`;
}
