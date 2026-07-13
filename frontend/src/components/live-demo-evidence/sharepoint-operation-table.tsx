import { formatDateTime } from "@/lib/format";
import type { LiveDemoEvidenceSharePointOperation } from "@/lib/live-demo-evidence";

type SharePointOperationTableProps = {
  operations: LiveDemoEvidenceSharePointOperation[];
};

export function SharePointOperationTable({ operations }: SharePointOperationTableProps) {
  if (operations.length === 0) {
    return (
      <p className="mt-4 rounded-md bg-[#f8fafc] p-4 text-sm text-[#64748b]">
        Ingen simulatoroperasjoner er registrert for denne kjøringen ennå.
      </p>
    );
  }

  return (
    <>
      <p className="mt-4 text-xs text-[#64748b] sm:hidden" id="sharepoint-table-scroll-hint">
        Tabellen kan rulles vannrett.
      </p>
      <div
        aria-describedby="sharepoint-table-scroll-hint"
        aria-label="SharePoint simulatoroperasjoner"
        className="mt-2 overflow-x-auto rounded-lg border border-[#e2e8f0] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8] sm:mt-4"
        role="region"
        tabIndex={0}
      >
      <table className="w-full min-w-[760px] text-left text-sm">
        <caption className="sr-only">Operasjoner utført av den lokale SharePoint-simulatoren</caption>
        <thead className="bg-[#f8fafc] text-xs uppercase tracking-wide text-[#64748b]">
          <tr>
            <th className="p-3 font-semibold">Tid</th>
            <th className="p-3 font-semibold">Metode</th>
            <th className="p-3 font-semibold">Handling</th>
            <th className="p-3 font-semibold">Resultat</th>
            <th className="p-3 font-semibold">Varighet</th>
            <th className="p-3 font-semibold">Forsøk</th>
            <th className="p-3 font-semibold">Idempotens</th>
          </tr>
        </thead>
        <tbody>
          {operations.map((operation, index) => (
            <tr className="border-t border-[#e2e8f0] text-[#334155]" key={`${operation.timestamp}-${index}`}>
              <td className="whitespace-nowrap p-3">{formatDateTime(operation.timestamp)}</td>
              <td className="p-3 font-mono font-semibold">{operation.method}</td>
              <td className="max-w-sm break-all p-3 font-mono text-xs">{operation.action}</td>
              <td className="whitespace-nowrap p-3">
                <span className={operation.statusCode < 400 ? "text-[#166534]" : "text-[#b91c1c]"}>
                  {operation.statusCode} · {operation.result}
                </span>
              </td>
              <td className="whitespace-nowrap p-3">{operation.durationMs} ms</td>
              <td className="p-3">{operation.attempt}</td>
              <td className="p-3">{operation.idempotencyResult}</td>
            </tr>
          ))}
        </tbody>
      </table>
      </div>
    </>
  );
}
