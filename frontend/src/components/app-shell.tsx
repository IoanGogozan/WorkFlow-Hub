import Link from "next/link";

const navItems = [
  { label: "Dashboard", href: "/" },
  { label: "Intake", href: "/intakes" },
  { label: "Cases", href: "/cases" },
  { label: "Documents", href: "/documents" },
  { label: "Integrations", href: "/integrations" },
];

type AppShellProps = {
  children: React.ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  return (
    <main className="min-h-screen">
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
                key={item.href}
                className="rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm font-medium text-[#334155] hover:bg-[#eef2ff] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                href={item.href}
              >
                {item.label}
              </Link>
            ))}
          </nav>
        </div>
      </header>
      {children}
    </main>
  );
}
