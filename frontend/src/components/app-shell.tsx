"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { DemoExpiryBanner } from "@/components/demo-expiry-banner";

const navItems = [
  { key: "overview", label: "Oversikt", href: "/" },
  { key: "input", label: "Input", href: "/intakes" },
  { key: "case", label: "Sak", href: "/cases" },
  { key: "documents", label: "Dokumenter", href: "/documents" },
  { key: "integrations", label: "Integrasjoner", href: "/integrations" },
  { key: "summary", label: "Oppsummering", href: "/summary" },
];

type AppShellProps = {
  children: React.ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();

  return (
    <main className="min-h-screen">
      <DemoExpiryBanner />
      <header className="border-b border-[#d8deea] bg-white">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 px-6 py-5 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-sm font-medium text-[#4f46e5]">
              Norvix WorkFlow Hub
            </p>
            <h1 className="mt-1 text-2xl font-semibold text-[#162033]">
              Agder Drift & Service AS
            </h1>
          </div>
          <nav aria-label="Main navigation" className="flex flex-wrap gap-1.5">
            {navItems.map((item) => (
              <Link
                key={item.key}
                aria-current={isActiveNavItem(item.href, pathname) ? "page" : undefined}
                className={
                  isActiveNavItem(item.href, pathname)
                    ? "rounded-md bg-[#eff6ff] px-3 py-2 text-sm font-semibold text-[#1d4ed8] ring-1 ring-[#bfdbfe] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                    : "rounded-md px-3 py-2 text-sm font-medium text-[#334155] hover:bg-[#eef2ff] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                }
                href={item.href}
              >
                {item.label}
              </Link>
            ))}
          </nav>
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

function isActiveNavItem(href: string, pathname: string) {
  if (href === "/") {
    return pathname === "/";
  }

  if (href === "/summary") {
    return pathname === "/summary";
  }

  return pathname === href || pathname.startsWith(`${href}/`);
}
