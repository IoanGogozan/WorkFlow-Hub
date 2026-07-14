"use client";

import { useState } from "react";
import { formatDateTime } from "@/lib/format";
import type { LiveDemoEvidenceAuditEvent } from "@/lib/live-demo-evidence";

const collapsedEventCount = 10;

type AuditEvidenceTimelineProps = {
  events: LiveDemoEvidenceAuditEvent[];
};

export function AuditEvidenceTimeline({ events }: AuditEvidenceTimelineProps) {
  const [showAll, setShowAll] = useState(false);
  const orderedEvents = [...events].sort(
    (left, right) => Date.parse(left.timestamp) - Date.parse(right.timestamp),
  );
  const visibleEvents = showAll ? orderedEvents : orderedEvents.slice(0, collapsedEventCount);

  return (
    <section className="rounded-xl border border-[#d8deea] bg-white p-6" id="audit">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h3 className="text-xl font-semibold text-[#162033]">Hendelseslogg</h3>
          <p className="mt-2 text-sm text-[#64748b]">
            Kronologisk spor for denne kjøringen · {events.length} hendelser
          </p>
        </div>
      </div>

      {visibleEvents.length > 0 ? (
        <ol className="mt-6 border-l-2 border-[#c7d2fe] pl-6">
          {visibleEvents.map((event, index) => (
            <li className="relative pb-7 last:pb-0" key={`${event.timestamp}-${event.eventType}-${index}`}>
              <span
                aria-hidden="true"
                className="absolute -left-[31px] top-1.5 h-3 w-3 rounded-full bg-[#4f46e5] ring-4 ring-white"
              />
              <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <p className="font-semibold text-[#162033]">{eventLabel(event)}</p>
                  <p className="mt-1 text-sm text-[#475569]">
                    {event.provider ?? event.entityType} · {event.result}
                  </p>
                </div>
                <time className="whitespace-nowrap text-xs font-medium text-[#64748b]" dateTime={event.timestamp}>
                  {formatDateTime(event.timestamp)}
                </time>
              </div>
              <dl className="mt-3 flex flex-wrap gap-x-6 gap-y-2 text-xs text-[#64748b]">
                {event.durationMs !== null ? <InlineDetail label="Varighet" value={`${event.durationMs} ms`} /> : null}
                {event.attempt !== null ? <InlineDetail label="Forsøk" value={event.attempt.toString()} /> : null}
                <InlineDetail label="Korrelasjon" value={event.correlationId} mono />
              </dl>
            </li>
          ))}
        </ol>
      ) : (
        <p className="mt-5 text-sm text-[#64748b]">Ingen hendelser er registrert ennå.</p>
      )}

      {events.length > collapsedEventCount ? (
        <button
          className="mt-6 rounded-md border border-[#bfdbfe] bg-[#eff6ff] px-4 py-2 text-sm font-semibold text-[#1d4ed8] hover:bg-[#dbeafe]"
          onClick={() => setShowAll((current) => !current)}
          type="button"
        >
          {showAll ? "Vis færre hendelser" : "Vis alle hendelser"}
        </button>
      ) : null}
    </section>
  );
}

function InlineDetail({
  label,
  value,
  mono = false,
}: {
  label: string;
  value: string;
  mono?: boolean;
}) {
  return (
    <div className="flex gap-1">
      <dt className="font-semibold">{label}:</dt>
      <dd className={mono ? "font-mono" : undefined}>{value}</dd>
    </div>
  );
}

function eventLabel(event: LiveDemoEvidenceAuditEvent) {
  const labels: Record<string, string> = {
    LiveDemoStepCompleted: `${event.operationLabel} fullført`,
    LiveDemoStepFailed: `${event.operationLabel} feilet`,
    LiveDemoRunRetried: "Kjøringen satt i kø for nytt forsøk",
  };
  return labels[event.eventType] ?? event.operationLabel;
}
