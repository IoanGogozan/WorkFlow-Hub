"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import {
  getDemoSessionExpiresAt,
  saveDemoSession,
  type DemoSession,
} from "@/lib/demo-session";

export function DemoExpiryBanner() {
  const [expiresAt, setExpiresAt] = useState<string | null>(null);
  const [isResetting, setIsResetting] = useState(false);

  useEffect(() => {
    const frame = window.requestAnimationFrame(() => {
      setExpiresAt(getDemoSessionExpiresAt());
    });
    return () => window.cancelAnimationFrame(frame);
  }, []);

  const expiryText = expiresAt
    ? new Intl.DateTimeFormat("no", {
        dateStyle: "medium",
        timeStyle: "short",
      }).format(new Date(expiresAt))
    : "automatisk etter 24 timer";

  return (
    <div className="border-b border-[#bfdbfe] bg-[#eff6ff] px-6 py-2 text-xs font-semibold text-[#1d4ed8]">
      <div className="mx-auto flex max-w-7xl flex-col items-center justify-center gap-2 sm:flex-row">
        <span>
          Offentlig demo - fiktive data - demo-trygge integrasjoner - slettes{" "}
          {expiryText}
        </span>
        <button
          className="rounded-md border border-[#bfdbfe] bg-white px-2.5 py-1 text-xs font-semibold text-[#1d4ed8] hover:bg-[#dbeafe] disabled:cursor-not-allowed disabled:opacity-60"
          disabled={isResetting}
          onClick={resetDemo}
          type="button"
        >
          {isResetting ? "Starter på nytt..." : "Start ny demo"}
        </button>
      </div>
    </div>
  );

  async function resetDemo() {
    try {
      setIsResetting(true);
      const session = await api<DemoSession>("/api/demo-sessions", {
        method: "POST",
      });
      saveDemoSession(session);
      window.location.assign("/intakes");
    } catch {
      setIsResetting(false);
    }
  }
}
