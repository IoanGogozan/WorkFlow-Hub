import { ManualProcessPanel } from "@/components/manual-process-panel";
import { TimeSavingsCalculator } from "@/components/time-savings-calculator";

export function LiveDemoDetails() {
  return (
    <section aria-label="Mer om live-demoen" className="mt-6 space-y-3">
      <Detail title="Hva ble automatisert?">
        <p className="leading-6">
          Demoen samler kontroll, saksopprettelse, dokumentproduksjon,
          synkronisering og logging i én sporbar kjøring. Sammenlign med
          handlingene som ellers ofte utføres manuelt.
        </p>
        <ManualProcessPanel />
      </Detail>

      <Detail title="Hvordan beregnes mulig tidsbesparelse?">
        <p className="leading-6">
          Beregningen bruker antall henvendelser, manuell tid per henvendelse og
          en estimert reduksjon. Den er et åpent eksempel, ikke et løfte om
          faktisk effekt.
        </p>
        <TimeSavingsCalculator />
      </Detail>

      <Detail title="Hva er ekte og hva er simulert?">
        <dl className="grid gap-4 leading-6 sm:grid-cols-2">
          <Explanation
            description="Saksdata, PDF, hendelseslogg og kjøringsspesifikke bevis opprettes i Norvix sitt selvhostede demomiljø. Brreg bruker live offentlig oppslag når tjenesten svarer, ellers merket fallback."
            term="Ekte kjøring"
          />
          <Explanation
            description="SharePoint vises med en lokal simulator uten Microsoft 365-tilkobling. ERP-mottakeren er ikke tilgjengelig før en kvittering kan vises og verifiseres."
            term="Simulert eller utilgjengelig"
          />
        </dl>
      </Detail>

      <Detail title="Tekniske detaljer">
        <ul className="grid list-disc gap-2 pl-5 leading-6">
          <li>Demoøkten og alle spørringer er tenant-avgrenset.</li>
          <li>Hvert steg lagres med status, varighet og offentlig-sikker dokumentasjon.</li>
          <li>Retry fortsetter kontrollert uten å opprette dupliserte artefakter.</li>
          <li>Alle viste data er fiktive og kan knyttes til den aktuelle kjøringen.</li>
        </ul>
      </Detail>
    </section>
  );
}

function Detail({
  children,
  title,
}: {
  children: React.ReactNode;
  title: string;
}) {
  return (
    <details className="group rounded-lg border border-[#dce1e8] bg-white px-4 py-3 text-sm text-[#526075]">
      <summary className="cursor-pointer font-semibold text-[#172033] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-[#315ea8]">
        {title}
      </summary>
      <div className="pt-4">{children}</div>
    </details>
  );
}

function Explanation({
  description,
  term,
}: {
  description: string;
  term: string;
}) {
  return (
    <div className="rounded-lg bg-[#f5f7fa] p-4">
      <dt className="font-semibold text-[#172033]">{term}</dt>
      <dd className="mt-1">{description}</dd>
    </div>
  );
}
