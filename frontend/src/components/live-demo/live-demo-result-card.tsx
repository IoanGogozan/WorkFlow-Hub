import { CLIENT_DEMO_CONTACT_URL } from "@/lib/site-config";
import type { LiveDemoRunResult } from "@/lib/live-demo";

export function LiveDemoResultCard({ result, totalDurationMs }: { result: LiveDemoRunResult; totalDurationMs: number | null }) {
  const duration = totalDurationMs === null ? "under ett minutt" : `${(totalDurationMs / 1000).toLocaleString("nb-NO", { maximumFractionDigits: 1 })} sekunder`;
  return (
    <section
      aria-labelledby="live-demo-result-heading"
      className="mt-6 rounded-xl bg-[#172033] px-5 py-7 text-white sm:px-8 sm:py-9"
    >
      <p className="text-sm font-semibold text-[#9fc2ff]">Live-kjøring fullført</p>
      <h2
        className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl"
        id="live-demo-result-heading"
      >
        Fullført på {duration}
      </h2>
      <ul className="mt-5 grid gap-3 text-sm leading-6 text-[#e5edf9] sm:grid-cols-2">
        {result.brregMode === "live" ? <li>Brreg: Live kontroll</li> : null}
        {result.brregMode === "fallback" ? <li>Brreg: Fallback-snapshot</li> : null}
        {result.caseNumber ? <li>✓ Sak {result.caseNumber} opprettet</li> : null}
        <li>✓ Fiktiv PDF opprettet og lagret</li>
        {result.sharePointFileReference ? (
          <li>
            ✓ Simulated SharePoint adapter — no Microsoft 365 tenant connected
            ({result.sharePointFileReference})
          </li>
        ) : null}
        {result.erpReceiptId ? <li>✓ ERP demo receiver: {result.erpReceiptId}</li> : null}
        {result.auditEventCount !== null ? <li>✓ {result.auditEventCount} hendelser lagret i loggen</li> : null}
      </ul>
      <a
        className="mt-7 inline-flex rounded-md bg-white px-5 py-3 text-sm font-semibold text-[#172033] hover:bg-[#eef2f7] focus-visible:outline focus-visible:outline-2 focus-visible:outline-white"
        href={CLIENT_DEMO_CONTACT_URL}
      >
        Beskriv prosessen deres
      </a>
    </section>
  );
}
