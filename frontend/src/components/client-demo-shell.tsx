"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { getDemoSessionExpiresAt } from "@/lib/demo-session";

type ClientDemoShellProps = {
  children: React.ReactNode;
};

export function ClientDemoShell({ children }: ClientDemoShellProps) {
  const [expiresAt, setExpiresAt] = useState<string | null>(null);

  useEffect(() => {
    const frame = window.requestAnimationFrame(() => {
      setExpiresAt(getDemoSessionExpiresAt());
    });
    return () => window.cancelAnimationFrame(frame);
  }, []);

  const expiryText = expiresAt
    ? new Intl.DateTimeFormat("nb-NO", {
        dateStyle: "medium",
        timeStyle: "short",
      }).format(new Date(expiresAt))
    : "automatisk etter 24 timer";

  return (
    <main className="flex min-h-screen flex-col overflow-x-clip bg-[#f7f8fa]">
      <header className="border-b border-[#dce1e8] bg-white">
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:gap-6 sm:px-6 sm:py-5">
          <div>
            <Link
              className="text-lg font-semibold tracking-tight text-[#172033]"
              href="/"
            >
              Norvix
            </Link>
            <p className="mt-0.5 text-xs font-semibold uppercase tracking-[0.16em] text-[#64748b]">
              Integrasjonseksempel
            </p>
          </div>
          <Link
            className="text-sm font-semibold text-[#315ea8] hover:text-[#244a86] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
            href="/technical"
          >
            Tekniske detaljer
          </Link>
        </div>
      </header>

      <div className="flex-1">{children}</div>

      <footer className="border-t border-[#dce1e8] bg-white">
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-3 px-6 py-5 text-xs text-[#64748b] sm:flex-row sm:items-center sm:justify-between">
          <p>Fiktive data · Demoen utløper {expiryText}</p>
          <nav aria-label="Juridiske lenker" className="flex gap-4 font-semibold">
            <Link className="hover:text-[#315ea8]" href="/privacy">
              Personvern
            </Link>
            <Link className="hover:text-[#315ea8]" href="/terms">
              Vilkår
            </Link>
          </nav>
        </div>
      </footer>
    </main>
  );
}
