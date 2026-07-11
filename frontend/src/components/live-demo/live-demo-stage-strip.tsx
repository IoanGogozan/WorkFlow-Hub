const stages = ["Mottatt", "Kontrollert", "Opprettet", "Synkronisert"];

export function LiveDemoStageStrip() {
  return (
    <section aria-label="Fire steg i live-demoen" className="pb-10 sm:pb-14">
      <ol className="grid gap-3 sm:grid-cols-4">
        {stages.map((stage, index) => (
          <li
            className="flex items-center gap-3 rounded-lg border border-[#dce1e8] bg-white px-4 py-4 shadow-sm"
            key={stage}
          >
            <span className="flex size-7 shrink-0 items-center justify-center rounded-full bg-[#e8f0ff] text-sm font-bold text-[#315ea8]">
              {index + 1}
            </span>
            <span className="text-sm font-semibold text-[#172033]">{stage}</span>
          </li>
        ))}
      </ol>
    </section>
  );
}
