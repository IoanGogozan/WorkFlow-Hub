import Link from "next/link";
import type { DemoStoryTechnicalLinks } from "@/lib/demo-story";

type TechnicalEvidencePanelProps = {
  links: DemoStoryTechnicalLinks;
};

const technicalCapabilities = [
  "ASP.NET Core / C#",
  "Next.js / TypeScript",
  "PostgreSQL",
  "Tenant-avgrensede data",
  "Sporbar hendelseslogg",
  "Automatiserte tester",
  "Azure-klargjort lagring og planlagt produksjonsoppsett",
];

export function TechnicalEvidencePanel({ links }: TechnicalEvidencePanelProps) {
  const evidenceLinks = [
    { label: "Åpne kildehenvendelsen", href: links.intakeHref },
    { label: "Åpne opprettet sak", href: links.caseHref },
    links.primaryDocumentHref
      ? { label: "Åpne hoveddokumentet", href: links.primaryDocumentHref }
      : null,
    links.deliveryPackageHref
      ? { label: "Åpne leveringsgrunnlaget", href: links.deliveryPackageHref }
      : null,
    { label: "Åpne integrasjonsstatus", href: links.integrationsHref },
  ].filter((link): link is { label: string; href: string } => link !== null);

  return (
    <section className="py-10 sm:py-14">
      <details className="group rounded-xl border border-[#d8dee8] bg-white">
        <summary className="flex cursor-pointer list-none items-center justify-between gap-5 px-5 py-5 text-lg font-semibold text-[#243147] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8] sm:px-6">
          <span>Se hva som faktisk er implementert</span>
          <span
            aria-hidden="true"
            className="text-2xl font-normal text-[#64748b] transition-transform motion-reduce:transition-none group-open:rotate-45"
          >
            +
          </span>
        </summary>

        <div className="border-t border-[#e2e6ec] px-5 py-6 sm:px-6">
          <p className="max-w-3xl text-sm leading-6 text-[#526075]">
            Linkene åpner de faktiske fiktive postene som inngår i scenariet.
            Denne tekniske visningen er dokumentasjon, ikke hovedopplevelsen.
          </p>

          <nav
            aria-label="Teknisk dokumentasjon for demosaken"
            className="mt-5 grid gap-2 sm:grid-cols-2"
          >
            {evidenceLinks.map((link) => (
              <Link
                className="rounded-md border border-[#d8dee8] bg-[#f8fafc] px-4 py-3 text-sm font-semibold text-[#315ea8] hover:bg-[#eef4fb] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[#315ea8]"
                href={link.href}
                key={link.label}
              >
                {link.label}
              </Link>
            ))}
          </nav>

          <h3 className="mt-7 text-base font-semibold text-[#243147]">
            Teknisk fundament
          </h3>
          <ul className="mt-3 grid gap-2 text-sm text-[#526075] sm:grid-cols-2">
            {technicalCapabilities.map((capability) => (
              <li className="flex gap-2" key={capability}>
                <span aria-hidden="true" className="font-bold text-[#24613f]">
                  ✓
                </span>
                {capability}
              </li>
            ))}
          </ul>
        </div>
      </details>
    </section>
  );
}
