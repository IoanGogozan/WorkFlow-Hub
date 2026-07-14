"use client";

import { useEffect, useRef } from "react";
import { ClientDemoCta } from "@/components/client-demo-cta";
import { ClientDemoShell } from "@/components/client-demo-shell";
import { LiveDemoDetails } from "@/components/live-demo/live-demo-details";
import { LiveDemoHero } from "@/components/live-demo/live-demo-hero";
import { LiveDemoRunPanel } from "@/components/live-demo/live-demo-run-panel";
import { LiveDemoStageStrip } from "@/components/live-demo/live-demo-stage-strip";
import { useLiveDemoRun } from "@/hooks/use-live-demo-run";

export function LiveDemoPreviewPage() {
  const liveDemo = useLiveDemoRun();
  const runHeadingRef = useRef<HTMLHeadingElement>(null);
  const focusedRunState = useRef<string | null>(null);

  useEffect(() => {
    if (!liveDemo.run) return;
    const stateKey = `${liveDemo.run.runId}:${liveDemo.run.retryCount}`;
    if (focusedRunState.current === stateKey) return;
    focusedRunState.current = stateKey;
    runHeadingRef.current?.focus();
  }, [liveDemo.run]);

  return (
    <ClientDemoShell>
      <div className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 sm:py-16">
        <LiveDemoHero
          capabilities={liveDemo.capabilities}
          disabled={liveDemo.isActive || liveDemo.isStarting || liveDemo.capabilities?.enabled === false}
          isStarting={liveDemo.isStarting}
          onStart={liveDemo.start}
        />
        <LiveDemoStageStrip />
        <LiveDemoRunPanel {...liveDemo} headingRef={runHeadingRef} />
        <ClientDemoCta />
        <LiveDemoDetails capabilities={liveDemo.capabilities} />
      </div>
    </ClientDemoShell>
  );
}
