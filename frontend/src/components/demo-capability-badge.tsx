import {
  capabilityToneClasses,
  type DemoCapability,
} from "@/lib/demo-capabilities";

export function DemoCapabilityBadge({
  capability,
}: {
  capability: DemoCapability;
}) {
  return (
    <span
      className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-semibold ${capabilityToneClasses(capability.tone)}`}
      title={capability.detail}
    >
      {capability.label}
    </span>
  );
}

export function DemoCapabilityNote({
  capability,
}: {
  capability: DemoCapability;
}) {
  return (
    <div
      className={`rounded-md border px-3 py-2 text-sm ${capabilityToneClasses(capability.tone)}`}
    >
      <span className="font-semibold">{capability.label}</span>
      <span className="ml-2">{capability.detail}</span>
    </div>
  );
}
