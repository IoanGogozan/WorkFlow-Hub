"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { DemoExpiryBanner } from "@/components/demo-expiry-banner";

const navItems = [
  { key: "overview", label: "Oversikt", href: "/technical" },
  { key: "sharepoint", label: "SharePoint simulator", href: "/technical/sharepoint" },
  { key: "entry", label: "Inngang", href: "/intakes" },
  { key: "review", label: "Vurdering", href: "/intakes" },
  { key: "case-documents", label: "Sak og dokumenter", href: "/cases" },
  { key: "delivery-summary", label: "Leveranse og oppsummering", href: "/summary" },
];

type AppShellProps = {
  children: React.ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();

  return (
    <main className="min-h-screen overflow-x-clip">
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
          <nav aria-label="Hovednavigasjon" className="flex flex-wrap gap-1.5">
            <Link
              className="rounded-md px-3 py-2 text-sm font-semibold text-[#1d4ed8] hover:bg-[#eff6ff] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
              href="/"
            >
              Til integrasjonseksempelet
            </Link>
            {navItems.map((item) => (
              <Link
                key={item.key}
                aria-current={isActiveNavItem(item.key, item.href, pathname) ? "page" : undefined}
                className={
                  isActiveNavItem(item.key, item.href, pathname)
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
          <p>Offentlig demo bruker fiktive data. Ikke send inn konfidensiell informasjon.</p>
          <nav aria-label="Juridiske lenker" className="flex gap-4 font-semibold">
            <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/privacy">
              Personvern
            </Link>
            <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/terms">
              Vilkår
            </Link>
          </nav>
        </div>
      </footer>
    </main>
  );
}

function isActiveNavItem(key: string, href: string, pathname: string) {
  if (key === "entry") {
    return pathname === "/intakes" || pathname === "/intakes/new";
  }

  if (key === "review") {
    return pathname.startsWith("/intakes/");
  }

  if (key === "case-documents") {
    return pathname.startsWith("/cases") || pathname.startsWith("/documents");
  }

  if (key === "delivery-summary") {
    return (
      pathname.startsWith("/delivery") ||
      pathname.startsWith("/integrations") ||
      pathname === "/summary"
    );
  }

  return pathname === href || pathname.startsWith(`${href}/`);
}
