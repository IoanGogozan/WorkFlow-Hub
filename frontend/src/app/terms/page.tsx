import {
  LegalSection,
  PublicLegalLayout,
} from "@/components/public-legal-layout";

export default function TermsPage() {
  return (
    <PublicLegalLayout
      intro="Disse vilkårene gjelder bare den offentlige interaktive demoen av Norvix WorkFlow Hub."
      title="Vilkår for bruk"
    >
      <LegalSection title="Tillatt bruk">
        <p>
          Du kan bruke demoen til å utforske fiktive arbeidsflyter med input,
          kontroll med valgfri AI-støtte, saksbehandling, dokumentklassifisering,
          leveringspakker, kundelenker, hendelseslogg og integrasjonsstatus.
        </p>
      </LegalSection>

      <LegalSection title="Ingen ekte kundedata">
        <p>
          Ikke send inn persondata, konfidensiell informasjon, produksjonsdata
          om kunder, innloggingsdata, hemmeligheter, kontrakter eller regulert
          innhold.
        </p>
        <p>
          Offentlig opplasting er bevisst deaktivert i demoen. Bruk kun den
          innebygde demo-dokumentflyten.
        </p>
      </LegalSection>

      <LegalSection title="Begrensninger i demoen">
        <p>
          Demoen er laget for evaluering og presentasjon. Den er ikke en
          produksjonstjeneste og inkluderer ikke kundeonboarding, fakturering,
          SLA-forpliktelser eller ekte Microsoft Graph-, regnskaps- eller
          Fabric/Power BI-integrasjon.
        </p>
        <p>
          AI-, Microsoft-, regnskaps- og Fabric-flyt er simulert med mindre
          grensesnittet tydelig markerer at funksjonen kan kobles mot ekte
          system.
        </p>
      </LegalSection>

      <LegalSection title="Tilgjengelighet og opprydding">
        <p>
          Demoarbeidsområder er midlertidige og kan utløpe, nullstilles eller
          fjernes når som helst. Norvix AS kan endre, stanse eller fjerne demoen
          uten varsel.
        </p>
      </LegalSection>

      <LegalSection title="Kontakt">
        <p>
          Kontakt: Norvix AS. Bruk kontaktkanalen publisert på Norvix-nettstedet
          ved spørsmål om demoen.
        </p>
      </LegalSection>
    </PublicLegalLayout>
  );
}
