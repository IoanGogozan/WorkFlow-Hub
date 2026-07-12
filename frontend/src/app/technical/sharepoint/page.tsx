"use client";

import { useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import {
  getSharePointTechnicalEvidence,
  testRestrictedSharePointAccess,
  type SharePointAccessEvidence,
  type SharePointTechnicalDocument,
  type SharePointTechnicalOperation,
  type SharePointTechnicalStatus,
} from "@/lib/sharepoint-technical";

type Evidence = {
  status: SharePointTechnicalStatus;
  tree: string[];
  documents: SharePointTechnicalDocument[];
  operations: SharePointTechnicalOperation[];
};

export default function SharePointTechnicalPage() {
  const [evidence, setEvidence] = useState<Evidence | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [accessEvidence, setAccessEvidence] = useState<SharePointAccessEvidence | null>(null);
  const [testingAccess, setTestingAccess] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    getSharePointTechnicalEvidence()
      .then(setEvidence)
      .catch((loadError: unknown) => setError(loadError instanceof Error ? loadError.message : "Kunne ikke laste simulatorbevis."));
    return () => controller.abort();
  }, []);

  async function testAccess() {
    setTestingAccess(true);
    setError(null);
    try {
      setAccessEvidence(await testRestrictedSharePointAccess());
    } catch (accessError: unknown) {
      setError(accessError instanceof Error ? accessError.message : "Tilgangstesten feilet.");
    } finally {
      setTestingAccess(false);
    }
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-7xl px-6 py-10">
        <div className="rounded-lg border border-[#f5c96b] bg-[#fff8e8] p-5 text-[#713f12]">
          <p className="text-sm font-semibold">Lokal simulator</p>
          <h2 className="mt-2 text-3xl font-semibold text-[#162033]">SharePoint / Microsoft Graph – teknisk bevis</h2>
          <p className="mt-3 max-w-4xl text-sm leading-6">
            This environment uses a local SharePoint and Microsoft Graph simulator. No live Microsoft 365 tenant is connected.
          </p>
        </div>

        {error ? <p className="mt-6 rounded-md bg-[#fdecec] p-4 text-sm text-[#a33a3a]">{error}</p> : null}
        {!evidence && !error ? <p className="mt-6 text-sm text-[#64748b]">Laster teknisk bevis …</p> : null}
        {evidence ? (
          <>
            <section className="mt-6 grid gap-4 md:grid-cols-3" aria-label="Integrasjonsstatus">
              <InfoCard title="Provider" value={`SharePoint · ${evidence.status.mode}`} />
              <InfoCard title="Område" value={`${evidence.status.siteName} · ${evidence.status.libraryName}`} />
              <InfoCard title="Tillatelser" value={`${evidence.status.permissionModel} · ${evidence.status.permissionLevel}`} />
            </section>

            <section className="mt-8 rounded-lg border border-[#d8deea] bg-white p-5">
              <h3 className="text-xl font-semibold text-[#162033]">Mappestruktur</h3>
              {evidence.tree.length ? (
                <ul className="mt-4 space-y-2 font-mono text-sm text-[#334155]">
                  {evidence.tree.map((path) => <li key={path}>📁 {path}</li>)}
                </ul>
              ) : <p className="mt-3 text-sm text-[#64748b]">Ingen mapper er opprettet i denne demoøkten ennå.</p>}
            </section>

            <section className="mt-8 overflow-hidden rounded-lg border border-[#d8deea] bg-white">
              <h3 className="p-5 text-xl font-semibold text-[#162033]">Synkroniserte dokumenter</h3>
              <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead className="bg-[#f8fafc] text-[#475569]"><tr><th className="p-3">Fil</th><th className="p-3">Versjon</th><th className="p-3">eTag</th><th className="p-3">Mappe</th></tr></thead><tbody>{evidence.documents.map((document) => <tr className="border-t border-[#e2e8f0]" key={document.externalItemId}><td className="p-3 font-semibold">{document.name}</td><td className="p-3">{document.version}</td><td className="p-3 font-mono">{document.eTag}</td><td className="p-3 font-mono text-xs">{document.parentPath}</td></tr>)}</tbody></table></div>
            </section>

            <section className="mt-8 overflow-hidden rounded-lg border border-[#d8deea] bg-white">
              <h3 className="p-5 text-xl font-semibold text-[#162033]">Operasjonslogg</h3>
              <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead className="bg-[#f8fafc] text-[#475569]"><tr><th className="p-3">Metode</th><th className="p-3">Operasjon</th><th className="p-3">Mål</th><th className="p-3">Resultat</th></tr></thead><tbody>{evidence.operations.map((operation, index) => <tr className="border-t border-[#e2e8f0]" key={`${operation.createdAt}-${index}`}><td className="p-3 font-mono">{operation.httpMethod}</td><td className="p-3">{operation.operation}</td><td className="p-3 font-mono text-xs">{operation.target}</td><td className="p-3">{operation.statusCode} {operation.succeeded ? "OK" : operation.errorCode}</td></tr>)}</tbody></table></div>
            </section>

            <section className="mt-8 rounded-lg border border-[#d8deea] bg-white p-5">
              <h3 className="text-xl font-semibold text-[#162033]">Sites.Selected-demonstrasjon</h3>
              <p className="mt-2 text-sm text-[#475569]">Testmålet er «HR Internal Site» og endrer ingen forretningsdata.</p>
              <button className="mt-4 rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white disabled:opacity-60" disabled={testingAccess} onClick={testAccess} type="button">{testingAccess ? "Tester …" : "Test restricted-site access"}</button>
              {accessEvidence ? <p className="mt-4 rounded-md bg-[#fff4e5] p-3 text-sm text-[#854d0e]">Target: HR Internal Site · Result: {accessEvidence.statusCode} {accessEvidence.errorCode}</p> : null}
            </section>
          </>
        ) : null}
      </div>
    </AppShell>
  );
}

function InfoCard({ title, value }: { title: string; value: string }) {
  return <article className="rounded-lg border border-[#d8deea] bg-white p-4"><p className="text-xs font-semibold uppercase tracking-wide text-[#64748b]">{title}</p><p className="mt-2 text-sm font-semibold text-[#162033]">{value}</p></article>;
}
