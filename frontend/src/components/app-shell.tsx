import Link from "next/link";
import { DemoExpiryBanner } from "@/components/demo-expiry-banner";

const navItems = [
  { key: "overview", label: "1. Oversikt", href: "/" },
  { key: "input", label: "2. Input", href: "/intakes" },
  { key: "ai-review", label: "3. AI review", href: "/intakes" },
  { key: "case", label: "4. Sak", href: "/cases" },
  { key: "documents", label: "5. Dokumenter", href: "/documents" },
  { key: "integrations", label: "6. Integrasjoner", href: "/integrations" },
  { key: "delivery", label: "7. Leveranse", href: "/summary" },
];

type AppShellProps = {
  children: React.ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  return (
    <main className="min-h-screen">
      <DemoExpiryBanner />
      <header className="border-b border-[#d8deea] bg-white">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 px-6 py-5 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-sm font-medium text-[#4f46e5]">
              Agder Drift & Service AS
            </p>
            <h1 className="mt-1 text-2xl font-semibold text-[#162033]">
              Norvix WorkFlow Hub
            </h1>
          </div>
          <nav aria-label="Main navigation" className="flex flex-wrap gap-2">
            {navItems.map((item) => (
              <Link
                key={item.key}
                className="rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm font-medium text-[#334155] hover:bg-[#eef2ff] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                href={item.href}
              >
                {item.label}
              </Link>
            ))}
          </nav>
        </div>
        <div className="border-t border-[#e2e8f0] bg-[#f8fafc]">
          <div className="mx-auto flex max-w-7xl flex-wrap items-center gap-3 px-6 py-3 text-xs text-[#475569]">
            <span className="font-semibold text-[#162033]">Demo story:</span>
            <span>Input</span>
            <span aria-hidden="true">&gt;</span>
            <span>AI-forslag</span>
            <span aria-hidden="true">&gt;</span>
            <span>Menneskelig godkjenning</span>
            <span aria-hidden="true">&gt;</span>
            <span>Sak</span>
            <span aria-hidden="true">&gt;</span>
            <span>Integrasjoner</span>
            <span aria-hidden="true">&gt;</span>
            <span>Kundeleveranse</span>
          </div>
        </div>
      </header>
      {children}
      <footer className="border-t border-[#d8deea] bg-white">
        <div className="mx-auto flex max-w-7xl flex-col gap-2 px-6 py-4 text-xs text-[#64748b] sm:flex-row sm:items-center sm:justify-between">
          <p>Public demo uses fictional data. Do not submit confidential information.</p>
          <nav aria-label="Legal links" className="flex gap-4 font-semibold">
            <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/privacy">
              Privacy
            </Link>
            <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/terms">
              Terms
            </Link>
          </nav>
        </div>
      </footer>
    </main>
  );
}
