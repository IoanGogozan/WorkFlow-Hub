"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { BrandLockup } from "@/components/brand-lockup";
import { api } from "@/lib/api";
import { saveDemoSession, type DemoSession } from "@/lib/demo-session";

const reasonText: Record<string, string> = {
  expired: "Demoen er utløpt. Start en ny for å fortsette.",
  missing: "Start demoen før du åpner integrasjonseksempelet.",
  invalid: "Demo-tokenet ble ikke godtatt. Start en ny demo.",
};

const demoBoundaries = [
  "Fiktive data",
  "Ingen innlogging",
  "Ingen ekte kundesystemer kontaktes",
  "Demoen slettes automatisk",
];

export default function DemoStartPage() {
  return (
    <Suspense fallback={<DemoPageShell />}>
      <DemoStartContent />
    </Suspense>
  );
}

function DemoStartContent() {
  const searchParams = useSearchParams();
  const reason = searchParams.get("reason");
  const [isReady, setIsReady] = useState(false);
  const [isStarting, setIsStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const frame = window.requestAnimationFrame(() => setIsReady(true));
    return () => window.cancelAnimationFrame(frame);
  }, []);

  async function startDemo() {
    try {
      setIsStarting(true);
      setError(null);
      const session = await api<DemoSession>("/api/demo-sessions", {
        method: "POST",
      });
      saveDemoSession(session);
      window.location.assign("/demo/run");
    } catch (startError) {
      setError(
        startError instanceof Error
          ? startError.message
          : "Demoen kunne ikke startes.",
      );
      setIsStarting(false);
    }
  }

  return (
    <DemoPageShell
      error={error}
      isReady={isReady}
      isStarting={isStarting}
      onStartDemo={startDemo}
      reason={reason}
    />
  );
}

type DemoPageShellProps = {
  reason?: string | null;
  error?: string | null;
  isReady?: boolean;
  isStarting?: boolean;
  onStartDemo?: () => void;
};

function DemoPageShell({
  reason,
  error,
  isReady = false,
  isStarting = false,
  onStartDemo,
}: DemoPageShellProps) {
  return (
    <main className="flex min-h-screen flex-col bg-[#f7f8fa]">
      <header className="border-b border-t-4 border-b-[#dce1e8] border-t-[#d8613c] bg-white">
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-6 py-5">
          <BrandLockup subtitle="Integrasjonseksempel" />
          <Link
            className="text-sm font-semibold text-[#315ea8] hover:text-[#244a86] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
            href="/technical"
          >
            Tekniske detaljer
          </Link>
        </div>
      </header>

      <section className="mx-auto flex w-full max-w-6xl flex-1 flex-col justify-center px-6 py-12">
        <div className="grid gap-8 rounded-xl border border-[#d8dee8] bg-white p-6 shadow-sm sm:p-8 lg:grid-cols-[minmax(0,1.15fr)_minmax(18rem,0.85fr)] lg:gap-12 lg:p-12">
          <div>
            <p className="text-sm font-semibold text-[#315ea8]">
              Praktisk integrasjonseksempel
            </p>
            <h1 className="mt-3 text-4xl font-semibold leading-tight tracking-tight text-[#172033] sm:text-5xl">
              Se hvordan én manuell serviceflyt kan automatiseres
            </h1>
            <p className="mt-5 max-w-2xl text-base leading-7 text-[#526075]">
              En henvendelse kommer på e-post. Kundeinformasjon må kontrolleres,
              sak opprettes, dokumenter lagres og status oppdateres. Demoen viser
              hvordan disse stegene kan bindes sammen uten å erstatte systemene
              bedriften allerede bruker.
            </p>

            {reason && reasonText[reason] ? (
              <div
                className="mt-6 rounded-md border border-[#e5cb7d] bg-[#fff9e8] p-4 text-sm text-[#6f571a]"
                role="status"
              >
                {reasonText[reason]}
              </div>
            ) : null}

            {error ? (
              <div
                className="mt-6 rounded-md border border-[#f3b7b7] bg-[#fff2f2] p-4 text-sm text-[#8f2525]"
                role="alert"
              >
                {error}
              </div>
            ) : null}

            <div className="mt-8">
              <button
                className="inline-flex rounded-md bg-[#315ea8] px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-[#274d8b] disabled:cursor-not-allowed disabled:opacity-60 focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
                disabled={!isReady || isStarting || !onStartDemo}
                onClick={onStartDemo}
                type="button"
              >
                {!isReady
                  ? "Laster demo..."
                  : isStarting
                    ? "Starter demo..."
                    : "Se automatiseringen"}
              </button>
              <p className="mt-3 text-sm text-[#64748b]">
                Ingen innlogging eller egne data er nødvendig.
              </p>
            </div>
          </div>

          <aside className="rounded-lg border border-[#dce3ec] bg-[#f3f6fa] p-5 sm:p-6">
            <h2 className="text-base font-semibold text-[#243147]">
              Trygg å utforske
            </h2>
            <ul className="mt-4 grid gap-3 text-sm text-[#445268]">
              {demoBoundaries.map((boundary) => (
                <li className="flex gap-3" key={boundary}>
                  <span aria-hidden="true" className="font-bold text-[#24613f]">
                    ✓
                  </span>
                  {boundary}
                </li>
              ))}
            </ul>
            <p className="mt-5 border-t border-[#d8dee8] pt-4 text-xs leading-5 text-[#64748b]">
              Demoen oppretter en ny fiktiv kjøring i et isolert arbeidsområde.
              Brreg kan kontrolleres offentlig; ingen kunde-, Microsoft- eller
              økonomisystemer kobles til.
            </p>
          </aside>
        </div>

      </section>
      <footer className="border-t border-[#dce1e8] bg-white">
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-3 px-6 py-5 text-xs text-[#64748b] sm:flex-row sm:items-center sm:justify-between">
          <p>Fiktive data · Ingen innlogging · Midlertidig arbeidsområde</p>
          <nav aria-label="Juridiske lenker" className="flex gap-4 font-semibold">
            <Link className="hover:text-[#315ea8]" href="/privacy">Personvern</Link>
            <Link className="hover:text-[#315ea8]" href="/terms">Vilkår</Link>
          </nav>
        </div>
      </footer>
    </main>
  );
}
