import { LiveDemoEvidencePage } from "@/components/live-demo-evidence/live-demo-evidence-page";

type LiveRunEvidenceRouteProps = {
  params: Promise<{ runId: string }>;
};

export default async function LiveRunEvidenceRoute({ params }: LiveRunEvidenceRouteProps) {
  const { runId } = await params;
  return <LiveDemoEvidencePage runId={runId} />;
}
