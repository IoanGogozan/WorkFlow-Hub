"use client";

import Link from "next/link";
import { useState } from "react";
import { openLiveDemoPdf } from "@/lib/live-demo-evidence";
import type { LiveDemoRunResult } from "@/lib/live-demo";
import { CLIENT_DEMO_CONTACT_URL } from "@/lib/site-config";

type LiveDemoResultCardProps = {
  result: LiveDemoRunResult;
  totalDurationMs: number | null;
  brregDurationMs: number | null;
  erpAttemptCount: number;
  erpDurationMs: number | null;
};

export function LiveDemoResultCard({
  result,
  totalDurationMs,
  brregDurationMs,
  erpAttemptCount,
  erpDurationMs,
}: LiveDemoResultCardProps) {
  const [openingPdf, setOpeningPdf] = useState(false);
  const [pdfError, setPdfError] = useState(false);
  const duration = formatDuration(totalDurationMs, "under ett minutt");

  async function openPdf() {
    if (!result.documentDownloadHref || openingPdf) {
      return;
    }

    setOpeningPdf(true);
    setPdfError(false);
    try {
      await openLiveDemoPdf(result.documentDownloadHref);
    } catch {
      setPdfError(true);
    } finally {
      setOpeningPdf(false);
    }
  }

  return (
    <section
      aria-labelledby="live-demo-result-heading"
      className="mt-6 rounded-xl bg-[#172033] px-5 py-7 text-white sm:px-8 sm:py-9"
      id="resultat"
    >
      <p className="text-sm font-semibold text-[#9fc2ff]">Live-kjøring fullført</p>
      <h2 className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl" id="live-demo-result-heading">
        Fullført på {duration}
      </h2>

      <div className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        <ResultItem title="Brreg">
          <ResultValue value={result.brregMode === "live" ? "Live" : "Fallback"} />
          <p className="mt-1 text-xs text-[#b8c7dc]">{formatDuration(brregDurationMs, "Varighet ikke tilgjengelig")}</p>
        </ResultItem>

        <ResultItem title="Sak">
          <ResultValue value={result.caseNumber ?? "Ikke opprettet"} mono />
          {result.caseHref ? <ResultLink href={result.caseHref} label="Åpne saken" /> : null}
        </ResultItem>

        <ResultItem title="PDF">
          <ResultValue value={result.documentFileName ?? "Demo-PDF opprettet"} mono />
          {result.documentDownloadHref ? (
            <button
              className="mt-3 text-sm font-semibold text-[#9fc2ff] underline-offset-4 hover:underline disabled:opacity-60"
              disabled={openingPdf}
              onClick={openPdf}
              type="button"
            >
              {openingPdf ? "Åpner PDF …" : "Åpne PDF"}
            </button>
          ) : null}
        </ResultItem>

        <ResultItem title="Leveringspakke">
          <ResultValue value={result.deliveryPackageHref ? "Opprettet" : "Ikke opprettet"} />
          {result.deliveryPackageHref ? (
            <ResultLink href={result.deliveryPackageHref} label="Åpne leveringspakken" />
          ) : null}
        </ResultItem>

        <ResultItem title="SharePoint">
          <div className="flex flex-wrap items-center gap-2">
            <ResultValue value={result.sharePointFileReference ? "Synkronisert" : "Ikke synkronisert"} />
            <span className="rounded-md bg-[#fff7ed] px-2 py-0.5 text-xs font-semibold text-[#9a3412]">Simulator</span>
          </div>
          {result.sharePointEvidenceHref ? (
            <ResultLink href={result.sharePointEvidenceHref} label="Se simulatorbevis" />
          ) : null}
        </ResultItem>

        {result.erpReceiptId ? <ResultItem title="Norvix ERP demo receiver">
          <ResultValue value={result.erpReceiptId ? "Melding mottatt" : "Ikke tilgjengelig ennå"} />
          {result.erpReceiptId ? (
            <>
              <p className="mt-2 break-words font-mono text-xs text-[#dbeafe]">Kvittering: {result.erpReceiptId}</p>
              <p className="mt-1 text-xs text-[#b8c7dc]">
                Forsøk: {erpAttemptCount} · {formatDuration(erpDurationMs, "Varighet ikke tilgjengelig")}
              </p>
            </>
          ) : null}
        </ResultItem> : null}

        <ResultItem title="Hendelseslogg">
          <ResultValue value={`${result.auditEventCount ?? 0} hendelser`} />
          <ResultLink href={result.auditHref} label="Se hendelseslogg" />
        </ResultItem>
      </div>

      {pdfError ? (
        <p className="mt-4 rounded-md bg-[#3b2530] px-4 py-3 text-sm text-[#fecaca]" role="alert">
          PDF-en kunne ikke åpnes. Prøv igjen fra den aktive demoøkten.
        </p>
      ) : null}

      <div className="mt-7 flex flex-wrap gap-3">
        <Link
          className="inline-flex rounded-md bg-white px-5 py-3 text-sm font-semibold text-[#172033] hover:bg-[#eef2f7] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white"
          href={result.evidenceHref}
        >
          Se hva som faktisk ble opprettet
        </Link>
        <a
          className="inline-flex rounded-md border border-[#9fc2ff] px-5 py-3 text-sm font-semibold text-white hover:bg-[#26334a] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white"
          href={CLIENT_DEMO_CONTACT_URL}
        >
          Beskriv prosessen deres
        </a>
      </div>
    </section>
  );
}

function ResultItem({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <article className="rounded-lg border border-[#3b4961] bg-[#202c41] p-4">
      <h3 className="text-xs font-semibold uppercase tracking-wide text-[#9fb0c8]">{title}</h3>
      <div className="mt-2">{children}</div>
    </article>
  );
}

function ResultValue({ value, mono = false }: { value: string; mono?: boolean }) {
  return <p className={`break-words text-sm font-semibold text-white ${mono ? "font-mono" : ""}`}>{value}</p>;
}

function ResultLink({ href, label }: { href: string; label: string }) {
  return (
    <Link className="mt-3 inline-flex text-sm font-semibold text-[#9fc2ff] underline-offset-4 hover:underline" href={href}>
      {label}
    </Link>
  );
}

function formatDuration(durationMs: number | null, fallback: string) {
  if (durationMs === null) {
    return fallback;
  }

  if (durationMs < 1000) {
    return `${durationMs} ms`;
  }

  return `${(durationMs / 1000).toLocaleString("nb-NO", { maximumFractionDigits: 1 })} sekunder`;
}
