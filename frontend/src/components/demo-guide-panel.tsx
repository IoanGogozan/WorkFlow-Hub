import Link from "next/link";

const demoSteps = [
  {
    label: "Start demo",
    href: "/demo",
  },
  {
    label: "Se input fra flere kilder",
    href: "/intakes",
  },
  {
    label: "Kjør AI-forslag",
    href: "/intakes",
  },
  {
    label: "Godkjenn struktur",
    href: "/intakes",
  },
  {
    label: "Opprett sak",
    href: "/cases",
  },
  {
    label: "Kjør integrasjoner",
    href: "/integrations",
  },
  {
    label: "Lag leveringspakke",
    href: "/documents",
  },
  {
    label: "Se oppsummering",
    href: "/summary",
  },
] as const;

type DemoGuidePanelProps = {
  activeStep?: number;
  title?: string;
  nextTitle?: string;
  nextDescription?: string;
  nextHref?: string;
  nextLabel?: string;
};

export function DemoGuidePanel({
  activeStep = 1,
  title = "Demo flow",
  nextTitle = "Neste steg",
  nextDescription = "Åpne en forespørsel og kjør AI-forslag for å se hvordan ustrukturert input blir til godkjente saksdata.",
  nextHref = "/intakes",
  nextLabel = "Åpne input",
}: DemoGuidePanelProps) {
  return (
    <section className="rounded-md border border-[#d8deea] bg-white p-5">
      <h3 className="text-lg font-semibold text-[#162033]">{title}</h3>
      <ol className="mt-4 space-y-3 text-sm">
        {demoSteps.map((step, index) => {
          const stepNumber = index + 1;
          const status =
            stepNumber < activeStep
              ? "done"
              : stepNumber === activeStep
                ? "current"
                : "next";

          return (
          <li className="flex items-start gap-3" key={step.label}>
            <span
              className={
                status === "done"
                  ? "mt-0.5 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-[#047857] text-xs font-semibold text-white"
                  : status === "current"
                    ? "mt-0.5 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-[#2563eb] text-xs font-semibold text-white"
                    : "mt-0.5 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full border border-[#cbd5e1] bg-white text-xs font-semibold text-[#64748b]"
              }
            >
              {status === "done" ? "OK" : String(stepNumber)}
            </span>
            <Link
              className="font-medium text-[#334155] hover:text-[#2563eb]"
              href={step.href}
            >
              {step.label}
            </Link>
          </li>
          );
        })}
      </ol>
      <div className="mt-5 rounded-md border border-[#bfdbfe] bg-[#eff6ff] p-4">
        <p className="text-sm font-semibold text-[#1e3a8a]">{nextTitle}</p>
        <p className="mt-2 text-sm leading-6 text-[#475569]">
          {nextDescription}
        </p>
        <Link
          className="mt-4 inline-flex rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]"
          href={nextHref}
        >
          {nextLabel}
        </Link>
      </div>
    </section>
  );
}
