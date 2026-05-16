const statusStyles: Record<string, string> = {
  connected: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  succeeded: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  active: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  disconnected: "bg-[#f1f5f9] text-[#475569] ring-[#cbd5e1]",
  mocked: "bg-[#eff6ff] text-[#1d4ed8] ring-[#bfdbfe]",
  new: "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]",
  needsreview: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  failed: "bg-[#fee2e2] text-[#b91c1c] ring-[#fca5a5]",
  error: "bg-[#fee2e2] text-[#b91c1c] ring-[#fca5a5]",
};

type StatusBadgeProps = {
  status: string;
};

export function StatusBadge({ status }: StatusBadgeProps) {
  const style =
    statusStyles[status.replace(/\s/g, "").toLowerCase()] ??
    "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]";

  return (
    <span className={`inline-flex rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ${style}`}>
      {status}
    </span>
  );
}
