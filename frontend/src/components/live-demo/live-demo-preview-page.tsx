"use client";

import { ClientDemoShell } from "@/components/client-demo-shell";
import { LiveDemoHero } from "@/components/live-demo/live-demo-hero";
import { LiveDemoRunPanel } from "@/components/live-demo/live-demo-run-panel";
import { useLiveDemoRun } from "@/hooks/use-live-demo-run";

export function LiveDemoPreviewPage() {
  const liveDemo = useLiveDemoRun();

  return (
    <ClientDemoShell>
      <div className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 sm:py-16">
        <LiveDemoHero
          disabled={liveDemo.isActive || liveDemo.isStarting || liveDemo.capabilities?.enabled === false}
          isStarting={liveDemo.isStarting}
          onStart={liveDemo.start}
        />
        <LiveDemoRunPanel {...liveDemo} />
      </div>
    </ClientDemoShell>
  );
}
