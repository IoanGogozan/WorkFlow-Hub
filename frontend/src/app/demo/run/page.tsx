import type { Metadata } from "next";
import { LiveDemoPreviewPage } from "@/components/live-demo/live-demo-preview-page";

export const metadata: Metadata = {
  title: "Interactive Demo | Norvix WorkFlow Hub",
  description: "Run the fictional WorkFlow Hub integration scenario and inspect its evidence.",
};

export default function DemoRunPage() {
  return <LiveDemoPreviewPage />;
}
