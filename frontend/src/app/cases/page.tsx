"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
import { api } from "@/lib/api";
import { formatDate, formatDateTime } from "@/lib/format";
import type { CaseListItem } from "@/lib/types";

export default function CasesPage() {
  const [cases, setCases] = useState<CaseListItem[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadCases() {
      try {
        setError(null);
        setCases(
          await api<CaseListItem[]>("/api/cases", {
            signal: controller.signal,
          }),
        );
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Cases could not be loaded.",
          );
        }
      }
    }

    void loadCases();
    return () => controller.abort();
  }, []);

  return (
    <AppShell>
      <div className="mx-auto max-w-7xl px-6 py-6">
        <div className="mb-6">
          <p className="text-sm font-medium text-[#64748b]">
            Steg 4: Sak opprettes
          </p>
          <h2 className="mt-2 text-3xl font-semibold">Saker</h2>
          <p className="mt-3 max-w-3xl text-sm leading-6 text-[#475569]">
            Godkjent input blir til sporbare saker med status, ansvar,
            dokumenter og historikk.
          </p>
          <div className="mt-5">
            <WorkflowProgress activeStep={3} />
          </div>
        </div>

        {error ? (
          <ErrorState message={error} />
        ) : !cases ? (
          <LoadingState label="Loading cases" />
        ) : cases.length === 0 ? (
          <EmptyState
            message="Opprett en sak fra godkjent input for å fortsette demoen."
            title="Ingen saker ennå"
          />
        ) : (
          <section className="overflow-hidden rounded-md border border-[#d8deea] bg-white">
            <div className="divide-y divide-[#e2e8f0]">
              {cases.map((caseItem) => (
                <article
                  className="grid gap-4 p-5 md:grid-cols-[1fr_160px_160px_100px]"
                  key={caseItem.id}
                >
                  <div>
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="text-sm font-medium text-[#64748b]">
                        {caseItem.caseNumber}
                      </p>
                      <StatusBadge status={caseItem.status} />
                    </div>
                    <Link
                      className="mt-2 block text-lg font-semibold text-[#162033] hover:text-[#2563eb]"
                      href={`/cases/${caseItem.id}`}
                    >
                      {caseItem.title}
                    </Link>
                  </div>
                  <div className="text-sm text-[#475569]">
                    <p className="font-medium text-[#162033]">Frist</p>
                    <p className="mt-1">{formatDate(caseItem.dueDate)}</p>
                  </div>
                  <div className="text-sm text-[#475569]">
                    <p className="font-medium text-[#162033]">Opprettet</p>
                    <p className="mt-1">{formatDateTime(caseItem.createdAt)}</p>
                  </div>
                  <div className="md:text-right">
                    <Link
                      className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                      href={`/cases/${caseItem.id}`}
                    >
                      Åpne sak
                    </Link>
                  </div>
                </article>
              ))}
            </div>
          </section>
        )}
      </div>
    </AppShell>
  );
}
