"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { api } from "@/lib/api";
import { saveDemoSession, type DemoSession } from "@/lib/demo-session";

const reasonText: Record<string, string> = {
  expired: "Demoarbeidsområdet er utløpt. Start et nytt for å fortsette.",
  missing: "Start et demoarbeidsområde før du åpner appen.",
  invalid: "Demo-tokenet ble ikke godtatt. Start et nytt arbeidsområde.",
};

const demoCards = [
  {
    title: "Input fra flere kilder",
    text: "E-post, skjema, API og manuell registrering samles i én innboks.",
  },
  {
    title: "AI foreslår struktur",
    text: "Ustrukturert tekst gjøres om til felter, oppgaver og mangelliste.",
  },
  {
    title: "Mennesker godkjenner",
    text: "AI endrer ikke data direkte. Forslag må godkjennes før saken går videre.",
  },
  {
    title: "Integrasjoner distribuerer data",
    text: "Data kan sendes til dokumentarkiv, økonomisystem, rapportering, kundeportal og audit log.",
  },
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
      window.location.assign("/");
    } catch (startError) {
      setError(
        startError instanceof Error
          ? startError.message
          : "Demo workspace could not be started.",
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
    <main className="min-h-screen bg-[#f5f7fb]">
      <section className="mx-auto flex min-h-screen max-w-6xl flex-col justify-center px-6 py-12">
        <div className="mb-6">
          <Link
            className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
            href="/"
          >
            Norvix WorkFlow Hub
          </Link>
        </div>

        <div className="rounded-md border border-[#d8deea] bg-white p-6 shadow-sm sm:p-8">
          <p className="text-sm font-semibold text-[#4f46e5]">
            Public interactive demo
          </p>
          <h1 className="mt-3 text-3xl font-semibold text-[#162033] sm:text-4xl">
            Test en integrert arbeidsflyt
          </h1>
          <p className="mt-4 max-w-2xl text-base leading-7 text-[#475569]">
            Denne demoen viser hvordan Norvix kan koble sammen e-post,
            skjema, API, dokumenter, AI-forslag, menneskelig godkjenning,
            integrasjoner, rapportering og kundeleveranse i én samlet flyt.
          </p>

          <div className="mt-6 grid gap-4 md:grid-cols-2">
            {demoCards.map((card) => (
              <article
                className="rounded-md border border-[#e2e8f0] bg-[#f8fafc] p-4"
                key={card.title}
              >
                <h2 className="text-base font-semibold text-[#162033]">
                  {card.title}
                </h2>
                <p className="mt-2 text-sm leading-6 text-[#475569]">
                  {card.text}
                </p>
              </article>
            ))}
          </div>

          <ul className="mt-6 grid gap-3 text-sm text-[#334155] sm:grid-cols-2 lg:grid-cols-4">
            <li className="rounded-md border border-[#e2e8f0] bg-white p-3">
              Fiktive data
            </li>
            <li className="rounded-md border border-[#e2e8f0] bg-white p-3">
              Demo-safe integrasjoner
            </li>
            <li className="rounded-md border border-[#e2e8f0] bg-white p-3">
              Ingen innlogging
            </li>
            <li className="rounded-md border border-[#e2e8f0] bg-white p-3">
              Slettes automatisk etter 24 timer
            </li>
          </ul>

          {reason && reasonText[reason] ? (
            <div className="mt-6 rounded-md border border-[#fde68a] bg-[#fffbeb] p-4 text-sm text-[#92400e]">
              {reasonText[reason]}
            </div>
          ) : null}

          {error ? (
            <div className="mt-6 rounded-md border border-[#fca5a5] bg-[#fef2f2] p-4 text-sm text-[#991b1b]">
              {error}
            </div>
          ) : null}

          <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:items-center">
            <button
              className="inline-flex w-fit rounded-md bg-[#2563eb] px-5 py-3 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
              disabled={!isReady || isStarting || !onStartDemo}
              onClick={onStartDemo}
              type="button"
            >
              {!isReady
                ? "Laster demo..."
                : isStarting
                  ? "Starter arbeidsområde..."
                  : "Start demoarbeidsområde"}
            </button>
            <p className="text-sm text-[#64748b]">
              Ingen innlogging er nødvendig for den offentlige demoen.
            </p>
          </div>

          <nav
            aria-label="Demo legal links"
            className="mt-6 flex flex-wrap gap-4 text-sm font-semibold"
          >
            <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/privacy">
              Privacy notice
            </Link>
            <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/terms">
              Terms of use
            </Link>
          </nav>
        </div>
      </section>
    </main>
  );
}
