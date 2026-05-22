"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { api } from "@/lib/api";
import { saveDemoSession, type DemoSession } from "@/lib/demo-session";

const reasonText: Record<string, string> = {
  expired: "Your demo session expired. Start a new workspace to continue.",
  missing: "Start a demo workspace before opening the app.",
  invalid: "The demo session token was not accepted. Start a new workspace.",
};

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
      <section className="mx-auto flex min-h-screen max-w-4xl flex-col justify-center px-6 py-12">
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
            Start a temporary demo workspace
          </h1>
          <p className="mt-4 max-w-2xl text-base leading-7 text-[#475569]">
            This public demo uses fictional data and creates an isolated
            workspace for your browser session. Demo data expires automatically.
          </p>

          <ul className="mt-6 divide-y divide-[#e2e8f0] border-y border-[#e2e8f0] text-sm text-[#334155]">
            <li className="py-3">
              Fictional customers, cases, documents, and integrations.
            </li>
            <li className="py-3">
              Mock AI and mock accounting/Microsoft integration behavior.
            </li>
            <li className="py-3">
              Do not upload personal, sensitive, or confidential information.
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
                ? "Loading demo..."
                : isStarting
                  ? "Starting workspace..."
                  : "Start demo workspace"}
            </button>
            <p className="text-sm text-[#64748b]">
              No login is required for the public demo.
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
