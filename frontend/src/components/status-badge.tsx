const statusStyles: Record<string, string> = {
  connected: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  succeeded: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  completed: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  running: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  queued: "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]",
  active: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  disconnected: "bg-[#f1f5f9] text-[#475569] ring-[#cbd5e1]",
  mocked: "bg-[#eff6ff] text-[#1d4ed8] ring-[#bfdbfe]",
  new: "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]",
  needsreview: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  failed: "bg-[#fee2e2] text-[#b91c1c] ring-[#fca5a5]",
  error: "bg-[#fee2e2] text-[#b91c1c] ring-[#fca5a5]",
  convertedtocase: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  approved: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  aianalyzed: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  uploaded: "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]",
  processing: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  aiclassified: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  rejected: "bg-[#fee2e2] text-[#b91c1c] ring-[#fca5a5]",
  archived: "bg-[#f1f5f9] text-[#475569] ring-[#cbd5e1]",
  draft: "bg-[#f1f5f9] text-[#475569] ring-[#cbd5e1]",
  open: "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]",
  waitingforcustomer: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  waitingforinternalreview: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  readyfordelivery: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  delivered: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  closed: "bg-[#f1f5f9] text-[#475569] ring-[#cbd5e1]",
  ready: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  revoked: "bg-[#fee2e2] text-[#b91c1c] ring-[#fca5a5]",
};

const statusLabels: Record<string, string> = {
  new: "Ny",
  convertedtocase: "Sak opprettet",
  needsreview: "Klar for kontroll",
  approved: "Godkjent",
  aianalyzed: "KI-analysert",
  connected: "Tilkoblet",
  disconnected: "Ikke tilkoblet",
  mocked: "Demo",
  succeeded: "Fullført",
  completed: "Fullført",
  running: "Pågår",
  queued: "I kø",
  failed: "Feilet",
  error: "Feil",
  uploaded: "Lastet opp",
  processing: "Behandles",
  aiclassified: "KI-klassifisert",
  rejected: "Avvist",
  archived: "Arkivert",
  draft: "Utkast",
  open: "Åpen",
  waitingforcustomer: "Venter på kunde",
  waitingforinternalreview: "Venter på intern kontroll",
  readyfordelivery: "Klar for levering",
  delivered: "Levert",
  closed: "Lukket",
  ready: "Klar",
  revoked: "Trukket tilbake",
};

type StatusBadgeProps = {
  status: string;
};

export function StatusBadge({ status }: StatusBadgeProps) {
  const normalized = status.replace(/\s/g, "").toLowerCase();
  const style =
    statusStyles[normalized] ??
    "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]";
  const label = statusLabels[normalized] ?? status;

  return (
    <span className={`inline-flex rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ${style}`}>
      {label}
    </span>
  );
}
