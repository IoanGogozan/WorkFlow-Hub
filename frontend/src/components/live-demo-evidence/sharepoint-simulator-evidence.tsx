import Link from "next/link";
import { SharePointOperationTable } from "@/components/live-demo-evidence/sharepoint-operation-table";
import type { LiveDemoEvidenceSharePoint } from "@/lib/live-demo-evidence";

type SharePointSimulatorEvidenceProps = {
  evidence: LiveDemoEvidenceSharePoint | null;
};

export function SharePointSimulatorEvidence({ evidence }: SharePointSimulatorEvidenceProps) {
  return (
    <section className="rounded-xl border border-[#f5c96b] bg-white p-6" id="sharepoint">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h3 className="text-xl font-semibold text-[#162033]">Lokal SharePoint-simulator</h3>
            <span className="inline-flex rounded-md bg-[#fff7ed] px-2.5 py-1 text-xs font-semibold text-[#9a3412] ring-1 ring-[#fdba74]">
              Simulert
            </span>
          </div>
          <p className="mt-2 text-sm font-medium text-[#713f12]">
            Ingen Microsoft 365-konto er tilkoblet.
          </p>
        </div>
        {evidence ? (
          <Link
            className="inline-flex w-fit rounded-md border border-[#bfdbfe] bg-[#eff6ff] px-4 py-2 text-sm font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
            href={evidence.technicalSharePointHref}
          >
            Åpne full simulatorvisning
          </Link>
        ) : null}
      </div>

      {evidence ? (
        <>
          <dl className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <EvidenceField label="Simulert område" value={evidence.siteName} />
            <EvidenceField label="Bibliotek" value={evidence.libraryName} />
            <EvidenceField label="Mappe-ID" value={evidence.folderId} mono />
            <EvidenceField label="Fil-ID" value={evidence.fileId} mono />
            <EvidenceField label="Filnavn" value={evidence.fileName} mono />
            <EvidenceField label="Versjon" value={evidence.version.toString()} />
            <EvidenceField label="eTag" value={evidence.eTag} mono />
          </dl>

          <div className="mt-6 rounded-lg bg-[#f8fafc] p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-[#64748b]">Mappesti</p>
            <p className="mt-2 break-all font-mono text-sm text-[#334155]">📁 {evidence.folderPath}</p>
          </div>

          {Object.keys(evidence.metadata).length > 0 ? (
            <div className="mt-6">
              <h4 className="text-sm font-semibold text-[#162033]">Metadata</h4>
              <dl className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                {Object.entries(evidence.metadata).map(([key, value]) => (
                  <EvidenceField key={key} label={key} value={value} />
                ))}
              </dl>
            </div>
          ) : null}

          <div className="mt-7 border-t border-[#e2e8f0] pt-6">
            <h4 className="text-lg font-semibold text-[#162033]">Operasjoner for denne kjøringen</h4>
            <SharePointOperationTable operations={evidence.operations} />
          </div>
        </>
      ) : (
        <p className="mt-5 text-sm text-[#64748b]">Dokumentet er ikke synkronisert til simulatoren ennå.</p>
      )}
    </section>
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
