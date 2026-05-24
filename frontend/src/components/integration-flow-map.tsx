const targets = [
  "Brreg",
  "SharePoint",
  "Regnskap",
  "Dashboard",
  "Kundeportal",
  "Audit trail",
];

export function IntegrationFlowMap() {
  return (
    <section className="rounded-md border border-[#d8deea] bg-white p-5">
      <h3 className="text-lg font-semibold text-[#162033]">
        Integrert dataflyt
      </h3>
      <div className="mt-5 grid gap-3 text-sm font-semibold text-[#334155] md:grid-cols-[1fr_auto_1.2fr_auto_2fr] md:items-center">
        <FlowNode label="Input" />
        <FlowArrow />
        <FlowNode label="Norvix workflow" tone="primary" />
        <FlowArrow />
        <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-3">
          {targets.map((target) => (
            <FlowNode key={target} label={target} />
          ))}
        </div>
      </div>
      <p className="mt-4 max-w-3xl text-sm leading-6 text-[#475569]">
        Demoen viser hvordan godkjente data kan flyte videre til systemene
        virksomheten allerede bruker, uten at ansatte ma kopiere status,
        metadata og leveransedata manuelt.
      </p>
    </section>
  );
}

function FlowNode({
  label,
  tone = "default",
}: {
  label: string;
  tone?: "default" | "primary";
}) {
  return (
    <div
      className={
        tone === "primary"
          ? "rounded-md border border-[#2563eb] bg-[#eff6ff] px-3 py-3 text-center text-[#1d4ed8]"
          : "rounded-md border border-[#cbd5e1] bg-[#f8fafc] px-3 py-3 text-center"
      }
    >
      {label}
    </div>
  );
}

function FlowArrow() {
  return (
    <div className="hidden text-center text-xl font-semibold text-[#64748b] md:block">
      &gt;
    </div>
  );
}
