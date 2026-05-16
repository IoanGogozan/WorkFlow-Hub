"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AppShell } from "@/components/app-shell";
import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
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
          <p className="text-sm font-medium text-[#64748b]">Case workspace</p>
          <h2 className="mt-2 text-3xl font-semibold">Cases</h2>
        </div>

        {error ? (
          <ErrorState message={error} />
        ) : !cases ? (
          <LoadingState label="Loading cases" />
        ) : cases.length === 0 ? (
          <EmptyState
            message="Convert an approved intake to create the first case."
            title="No cases yet"
          />
        ) : (
          <section className="overflow-hidden rounded-md border border-[#d8deea] bg-white">
            <div className="divide-y divide-[#e2e8f0]">
              {cases.map((caseItem) => (
                <article
                  className="grid gap-4 p-5 md:grid-cols-[1fr_160px_160px_80px]"
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
                    <p className="font-medium text-[#162033]">Due date</p>
                    <p className="mt-1">{formatDate(caseItem.dueDate)}</p>
                  </div>
                  <div className="text-sm text-[#475569]">
                    <p className="font-medium text-[#162033]">Created</p>
                    <p className="mt-1">{formatDateTime(caseItem.createdAt)}</p>
                  </div>
                  <div className="md:text-right">
                    <Link
                      className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                      href={`/cases/${caseItem.id}`}
                    >
                      Open
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
