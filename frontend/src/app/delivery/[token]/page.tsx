import Link from "next/link";
import { notFound } from "next/navigation";
import { formatDateTime } from "@/lib/format";
import type { PublicDeliveryPackage } from "@/lib/types";

type PublicDeliveryPageProps = {
  params: Promise<{ token: string }>;
};

export default async function PublicDeliveryPage({
  params,
}: PublicDeliveryPageProps) {
  const { token } = await params;
  const delivery = await getPublicDelivery(token);

  return (
    <main className="min-h-screen bg-[#f5f7fb] text-[#162033]">
      <div className="border-b border-[#bfdbfe] bg-[#eff6ff] px-6 py-2 text-center text-xs font-semibold text-[#1d4ed8]">
        Offentlig demo - fiktive data - utløper automatisk
      </div>
      <div className="mx-auto max-w-4xl px-6 py-10">
        <header className="border-b border-[#d8deea] pb-6">
          <p className="text-sm font-medium text-[#4f46e5]">
            Norvix WorkFlow Hub
          </p>
          <h1 className="mt-2 text-3xl font-semibold">
            Leveranse fra Agder Drift & Service AS
          </h1>
          <p className="mt-3 text-sm text-[#64748b]">
            Sak: {delivery.caseTitle}
          </p>
          <p className="mt-1 text-sm text-[#64748b]">
            Tilgjengelig til: {formatDateTime(delivery.expiresAt)}
          </p>
        </header>

        <section className="mt-6 rounded-md border border-[#d8deea] bg-white">
          <div className="border-b border-[#d8deea] px-5 py-4">
            <h2 className="text-lg font-semibold">Dokumenter i leveransen</h2>
          </div>
          <div className="divide-y divide-[#e2e8f0]">
            {delivery.documents.map((document) => (
              <article
                className="flex flex-col gap-3 p-5 sm:flex-row sm:items-center sm:justify-between"
                key={document.documentId}
              >
                <div>
                  <p className="font-medium text-[#162033]">{document.title}</p>
                  <p className="mt-1 text-sm text-[#64748b]">
                    {document.documentType ?? "Dokumentmetadata"}
                  </p>
                </div>
                <Link
                  className="rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm font-semibold text-[#334155] hover:bg-[#eef2ff]"
                  href={`/delivery/${token}/documents/${document.documentId}`}
                >
                  Last ned
                </Link>
              </article>
            ))}
          </div>
        </section>
        <footer className="mt-8 flex flex-wrap gap-4 border-t border-[#d8deea] pt-5 text-sm font-semibold">
          <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/privacy">
            Personvern
          </Link>
          <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/terms">
            Vilkår
          </Link>
        </footer>
      </div>
    </main>
  );
}

async function getPublicDelivery(token: string) {
  const backendUrl =
    process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";
  const response = await fetch(`${backendUrl}/delivery/${token}`, {
    cache: "no-store",
  });

  if (response.status === 404) {
    notFound();
  }

  if (response.status === 410) {
    throw new Error("Denne leveranselenken er utløpt eller trukket tilbake.");
  }

  if (!response.ok) {
    throw new Error("Leveransen kunne ikke lastes.");
  }

  return response.json() as Promise<PublicDeliveryPackage>;
}
