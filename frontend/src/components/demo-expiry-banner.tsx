"use client";

import { useEffect, useState } from "react";
import { getDemoSessionExpiresAt } from "@/lib/demo-session";

export function DemoExpiryBanner() {
  const [expiresAt, setExpiresAt] = useState<string | null>(null);

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
    <div className="border-b border-[#bfdbfe] bg-[#eff6ff] px-6 py-2 text-center text-xs font-semibold text-[#1d4ed8]">
      Public demo - fiktive data - demo-safe integrasjoner - slettes{" "}
      {expiryText}
    </div>
  );
}
