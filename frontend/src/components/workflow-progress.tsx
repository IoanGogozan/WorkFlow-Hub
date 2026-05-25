import Link from "next/link";

const workflowSteps = [
  {
    label: "Input",
    href: "/intakes",
  },
  {
    label: "AI sortering",
    href: "/intakes",
  },
  {
    label: "Godkjenn",
    href: "/intakes",
  },
  {
    label: "Sak",
    href: "/cases",
  },
  {
    label: "Lagre/send",
    href: "/integrations",
  },
  {
    label: "Leveranse",
    href: "/documents",
  },
] as const;

type WorkflowProgressProps = {
  activeStep?: number;
  completed?: boolean;
};

export function WorkflowProgress({
  activeStep = 1,
  completed = false,
}: WorkflowProgressProps) {
  return (
    <nav aria-label="Arbeidsflyt">
      <ol className="grid grid-cols-2 gap-2 text-sm sm:grid-cols-3">
        {workflowSteps.map((step, index) => {
          const stepNumber = index + 1;
          const isDone = completed || stepNumber < activeStep;
          const isActive = !completed && stepNumber === activeStep;

          return (
            <li key={step.label}>
              <Link
                className={
                  isActive
                    ? "flex min-h-11 min-w-0 items-center justify-center gap-2 rounded-md border border-[#bfdbfe] bg-[#eff6ff] px-2 py-2 font-semibold text-[#1d4ed8]"
                    : isDone
                      ? "flex min-h-11 min-w-0 items-center justify-center gap-2 rounded-md border border-[#bbf7d0] bg-[#f0fdf4] px-2 py-2 font-semibold text-[#166534]"
                      : "flex min-h-11 min-w-0 items-center justify-center gap-2 rounded-md border border-[#e2e8f0] bg-[#f8fafc] px-2 py-2 font-semibold text-[#475569] hover:bg-[#eef2ff]"
                }
                href={step.href}
              >
                <span
                  className={
                    isActive
                      ? "inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-[#2563eb] text-xs text-white"
                      : isDone
                        ? "inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-[#047857] text-[10px] text-white"
                        : "inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full border border-[#cbd5e1] bg-white text-xs text-[#64748b]"
                  }
                >
                  {isDone ? "OK" : stepNumber}
                </span>
                <span className="min-w-0 truncate whitespace-nowrap text-center">
                  {step.label}
                </span>
              </Link>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
