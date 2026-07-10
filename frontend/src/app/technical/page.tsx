"use client";

import Link from "next/link";
import { AppShell } from "@/components/app-shell";

const benefits = [
  "Mindre manuelt arbeid",
  "Bedre kontroll på dokumenter og status",
  "Klarere grunnlag for levering og rapportering",
];

const demoSteps = [
  { title: "Motta henvendelse", text: "E-post, skjema, API og dokumenter samles i én inngang." },
  { title: "Kontroller og berik informasjon", text: "Informasjon kontrolleres, berikes og kan få AI-forslag der det gir verdi." },
  { title: "Opprett sak og dokumentasjon", text: "Godkjent informasjon blir til sak, oppgaver, status og dokumentgrunnlag." },
  { title: "Lever og rapporter", text: "Data og dokumenter er klare for kunde, regnskap, hendelseslogg og rapportering." },
];

export default function TechnicalOverviewPage() {
  return (
    <AppShell>
      <div className="mx-auto max-w-6xl px-6 py-10">
        <section className="py-8 sm:py-12">
          <div className="inline-flex rounded-md border border-[#bfdbfe] bg-[#eff6ff] px-3 py-1 text-sm font-semibold text-[#1d4ed8]">
            Demo med fiktive data
          </div>
          <h2 className="mt-5 max-w-4xl text-4xl font-semibold leading-tight text-[#162033] sm:text-5xl">
            Fra henvendelse til sak og leveranse uten dobbeltregistrering
          </h2>
          <p className="mt-5 max-w-3xl text-base leading-7 text-[#475569]">
            Norvix WorkFlow Hub viser hvordan e-post, skjema og dokumenter kan
            samles i en strukturert arbeidsflyt med menneskelig kontroll, trygg
            viderelevering og valgfri AI-støtte.
          </p>
          <div className="mt-7 flex flex-col gap-3 sm:flex-row sm:items-center">
            <Link className="inline-flex w-fit rounded-md bg-[#2563eb] px-5 py-3 text-sm font-semibold text-white hover:bg-[#1d4ed8]" href="/intakes">
              Start teknisk gjennomgang
            </Link>
            <p className="text-sm text-[#64748b]">Demoen bruker kun fiktive data.</p>
          </div>
        </section>
        <section aria-label="Viktigste fordeler" className="grid gap-3 border-y border-[#d8deea] py-5 sm:grid-cols-3">
          {benefits.map((benefit) => (
            <div className="rounded-md border border-[#e2e8f0] bg-white px-4 py-3 text-sm font-semibold text-[#334155]" key={benefit}>
              {benefit}
            </div>
          ))}
        </section>
        <section className="py-8">
          <h3 className="text-lg font-semibold text-[#162033]">Slik demonstreres verdien</h3>
          <div className="mt-4 grid gap-4 md:grid-cols-4">
            {demoSteps.map((step) => (
              <article className="rounded-md border border-[#d8deea] bg-white p-4" key={step.title}>
                <h4 className="text-base font-semibold text-[#162033]">{step.title}</h4>
                <p className="mt-2 text-sm leading-6 text-[#475569]">{step.text}</p>
              </article>
            ))}
          </div>
        </section>
      </div>
    </AppShell>
  );
}
