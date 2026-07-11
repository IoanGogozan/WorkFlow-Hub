import { ClientDemoShell } from "@/components/client-demo-shell";
import { LiveDemoHero } from "@/components/live-demo/live-demo-hero";
import { LiveDemoRunPanel } from "@/components/live-demo/live-demo-run-panel";

export function LiveDemoPreviewPage() {
  return (
    <ClientDemoShell>
      <div className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 sm:py-16">
        <LiveDemoHero />
        <LiveDemoRunPanel />
      </div>
    </ClientDemoShell>
  );
}
