"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { AppShell } from "@/components/app-shell";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { StatusBadge } from "@/components/status-badge";
import { WorkflowProgress } from "@/components/workflow-progress";
import { WhyThisMatters } from "@/components/why-this-matters";
import { api } from "@/lib/api";
import { formatDateTime } from "@/lib/format";
import type { DeliveryPackage } from "@/lib/types";

function defaultExpiry() {
  const date = new Date();
  date.setDate(date.getDate() + 7);
  date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
  return date.toISOString().slice(0, 16);
}

export default function DeliveryPackagePage() {
  const params = useParams<{ id: string }>();
  const packageId = params.id;
  const [deliveryPackage, setDeliveryPackage] =
    useState<DeliveryPackage | null>(null);
  const [recipientEmail, setRecipientEmail] = useState("");
  const [expiresAt, setExpiresAt] = useState(defaultExpiry);
  const [newPublicUrl, setNewPublicUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [action, setAction] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadPackage() {
      try {
        setError(null);
        setDeliveryPackage(
          await api<DeliveryPackage>(`/api/delivery-packages/${packageId}`, {
            signal: controller.signal,
          }),
        );
      } catch (loadError) {
        if (!controller.signal.aborted) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Delivery package could not be loaded.",
          );
        }
      }
    }

    void loadPackage();
    return () => controller.abort();
  }, [packageId]);

  async function generatePdf() {
    try {
      setAction("pdf");
      setActionError(null);
      setDeliveryPackage(
        await api<DeliveryPackage>(
          `/api/delivery-packages/${packageId}/generate-pdf`,
          { method: "POST" },
        ),
      );
    } catch (pdfError) {
      setActionError(
        pdfError instanceof Error
          ? pdfError.message
          : "Summary PDF could not be generated.",
      );
    } finally {
      setAction(null);
    }
  }

  async function createLink(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    try {
      setAction("link");
      setActionError(null);
      setNewPublicUrl(null);
      const expires = new Date(expiresAt);
      const updated = await api<DeliveryPackage>(
        `/api/delivery-packages/${packageId}/create-link`,
        {
          method: "POST",
          body: {
            recipientEmail: recipientEmail || null,
            expiresAt: expires.toISOString(),
          },
        },
      );
      setDeliveryPackage(updated);
      const token = updated.links.find((link) => link.token)?.token;
      if (token) {
        setNewPublicUrl(`${window.location.origin}/delivery/${token}`);
      }
    } catch (linkError) {
      setActionError(
        linkError instanceof Error
          ? linkError.message
          : "Public link could not be created.",
      );
    } finally {
      setAction(null);
    }
  }

  async function revokeLink(linkId: string) {
    try {
      setAction(linkId);
      setActionError(null);
      await api(`/api/delivery-links/${linkId}/revoke`, { method: "POST" });
      setDeliveryPackage(
        await api<DeliveryPackage>(`/api/delivery-packages/${packageId}`),
      );
    } catch (revokeError) {
      setActionError(
        revokeError instanceof Error
          ? revokeError.message
          : "Delivery link could not be revoked.",
      );
    } finally {
      setAction(null);
    }
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-6xl px-6 py-6">
        {error ? (
          <ErrorState message={error} />
        ) : !deliveryPackage ? (
          <LoadingState label="Loading delivery package" />
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
            <section className="space-y-6">
              <div>
                <Link
                  className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                  href={`/cases/${deliveryPackage.caseId}`}
                >
                  Tilbake til sak
                </Link>
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <h2 className="text-3xl font-semibold">
                    {deliveryPackage.title}
                  </h2>
                  <StatusBadge status={deliveryPackage.status} />
                </div>
                <div className="mt-5">
                  <WorkflowProgress activeStep={4} />
                </div>
              </div>

              <WhyThisMatters title="Ryddig kundeleveranse">
                <p>
                  I stedet for løse vedlegg på e-post får kunden én
                  leveringslenke. Firmaet får status, historikk og kontroll.
                </p>
              </WhyThisMatters>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Dokumenter i leveransen</h3>
                <div className="mt-4 divide-y divide-[#e2e8f0]">
                  {deliveryPackage.items.map((item) => (
                    <div
                      className="flex flex-col gap-2 py-3 sm:flex-row sm:items-center sm:justify-between"
                      key={item.id}
                    >
                      <p className="font-medium text-[#162033]">
                        {item.displayName}
                      </p>
                      <Link
                        className="text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                        href={`/documents/${item.documentId}`}
                      >
                        Åpne dokument
                      </Link>
                    </div>
                  ))}
                </div>
              </section>

              <section className="rounded-md border border-[#d8deea] bg-white p-6">
                <h3 className="text-lg font-semibold">Kundelenker</h3>
                {deliveryPackage.links.length === 0 ? (
                  <p className="mt-3 text-sm text-[#64748b]">
                    Ingen kundelenker er opprettet ennå.
                  </p>
                ) : (
                  <div className="mt-4 divide-y divide-[#e2e8f0]">
                    {deliveryPackage.links.map((link) => (
                      <div
                        className="grid gap-3 py-3 md:grid-cols-[1fr_auto]"
                        key={link.id}
                      >
                        <div>
                          <p className="font-medium text-[#162033]">
                            {link.recipientEmail ?? "Ingen mottaker e-post"}
                          </p>
                          <p className="mt-1 text-sm text-[#64748b]">
                            Utløper {formatDateTime(link.expiresAt)}
                            {link.revokedAt
                              ? ` - Trukket tilbake ${formatDateTime(link.revokedAt)}`
                              : ""}
                          </p>
                          {link.token ? (
                            <Link
                              className="mt-2 inline-flex text-sm font-semibold text-[#2563eb] hover:text-[#1d4ed8]"
                              href={`/delivery/${link.token}`}
                              target="_blank"
                            >
                              Åpne kundeside
                            </Link>
                          ) : null}
                        </div>
                        <button
                          className="h-fit rounded-md border border-[#fca5a5] bg-[#fef2f2] px-3 py-2 text-sm font-semibold text-[#b91c1c] hover:bg-[#fee2e2] disabled:cursor-not-allowed disabled:opacity-60"
                          disabled={action !== null || link.revokedAt !== null}
                          onClick={() => revokeLink(link.id)}
                          type="button"
                        >
                          {action === link.id ? "Trekker tilbake..." : "Trekk tilbake"}
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </section>
            </section>

            <aside className="space-y-6">
              {actionError ? <ErrorState message={actionError} /> : null}

              <section className="rounded-md border border-[#d8deea] bg-white p-5">
                <h3 className="text-lg font-semibold">PDF-oppsummering</h3>
                <p className="mt-2 text-sm leading-6 text-[#64748b]">
                  Lag en PDF-oppsummering for leveransen. PDF-en lagres i
                  demoarbeidsområdet og slettes når demoen utløper.
                </p>
                <dl className="mt-4 text-sm">
                  <dt className="font-semibold text-[#334155]">Generert</dt>
                  <dd className="mt-1 text-[#64748b]">
                    {formatDateTime(deliveryPackage.summaryGeneratedAt)}
                  </dd>
                </dl>
                <button
                  className="mt-4 rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8] disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={action !== null}
                  onClick={generatePdf}
                  type="button"
                >
                  {action === "pdf" ? "Genererer..." : "Generer PDF"}
                </button>
              </section>

              <form
                className="space-y-4 rounded-md border border-[#d8deea] bg-white p-5"
                onSubmit={createLink}
              >
                <h3 className="text-lg font-semibold">Lag kundelenke</h3>
                <label className="block">
                  <span className="mb-1 block text-sm font-semibold text-[#334155]">
                    Mottaker e-post
                  </span>
                  <input
                    className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                    onChange={(event) => setRecipientEmail(event.target.value)}
                    type="email"
                    value={recipientEmail}
                  />
                </label>
                <label className="block">
                  <span className="mb-1 block text-sm font-semibold text-[#334155]">
                    Utløper
                  </span>
                  <input
                    className="w-full rounded-md border border-[#cbd5e1] px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#2563eb]"
                    onChange={(event) => setExpiresAt(event.target.value)}
                    required
                    type="datetime-local"
                    value={expiresAt}
                  />
                </label>
                <button
                  className="rounded-md bg-[#047857] px-4 py-2 text-sm font-semibold text-white hover:bg-[#065f46] disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={action !== null}
                  type="submit"
                >
                  {action === "link" ? "Oppretter..." : "Lag kundelenke"}
                </button>
                {newPublicUrl ? (
                  <div className="rounded-md border border-[#bbf7d0] bg-[#f0fdf4] p-3 text-sm">
                    <p className="font-semibold text-[#166534]">
                      Kundelenke opprettet
                    </p>
                    <Link
                      className="mt-2 block break-all text-[#2563eb] hover:text-[#1d4ed8]"
                      href={newPublicUrl}
                      target="_blank"
                    >
                      {newPublicUrl}
                    </Link>
                  </div>
                ) : null}
              </form>
            </aside>
          </div>
        )}
      </div>
    </AppShell>
  );
}
