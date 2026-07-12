export type LiveDemoStage = {
  title: string;
  status: "Fullført" | "Pågår" | "Venter" | "Feilet";
  duration: string;
  provider: string;
  summary: string;
  evidence: string;
};

export function LiveDemoStageCard({ stage }: { stage: LiveDemoStage }) {
  const statusClass = {
    Fullført: "bg-[#e6f6ee] text-[#1f6b46]",
    Pågår: "bg-[#e8f0ff] text-[#315ea8]",
    Venter: "bg-[#eef2f7] text-[#526075]",
    Feilet: "bg-[#fdecec] text-[#a33a3a]",
  }[stage.status];

  return (
    <li className="rounded-lg border border-[#dce1e8] bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <h3 className="font-semibold text-[#172033]">{stage.title}</h3>
          <p className="mt-1 text-sm text-[#64748b]">{stage.provider}</p>
        </div>
        <span
          className={`rounded-full px-2.5 py-1 text-xs font-semibold ${statusClass}`}
        >
          {stage.status}
        </span>
      </div>
      <p className="mt-4 text-sm leading-6 text-[#334155]">{stage.summary}</p>
      <div className="mt-4 flex flex-wrap items-center justify-between gap-2 border-t border-[#e7ebf0] pt-3 text-xs">
        <span className="font-semibold text-[#526075]">{stage.duration}</span>
        <span className="font-mono text-[#315ea8]">{stage.evidence}</span>
      </div>
    </li>
  );
}
