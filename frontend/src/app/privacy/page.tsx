import {
  LegalSection,
  PublicLegalLayout,
} from "@/components/public-legal-layout";

export default function PrivacyPage() {
  return (
    <PublicLegalLayout
      intro="Denne teksten beskriver den begrensede databehandlingen i den offentlige interaktive demoen. Den er ikke en personvernerklæring for produksjonskunder."
      title="Personvern"
    >
      <LegalSection title="Demoens omfang">
        <p>
          Demoen bruker fiktive kunder, saker, dokumenter, integrasjoner og
          forslag. Den er kun ment for å evaluere arbeidsflyten.
        </p>
        <p>
          Ikke last opp eller skriv inn personlige, sensitive, konfidensielle
          eller produksjonsrelaterte kundedata i demoen.
        </p>
      </LegalSection>

      <LegalSection title="Data som behandles">
        <p>
          Demoen kan behandle en midlertidig demoidentifikator, genererte
          fiktive arbeidsområdedata, nettleserforespørsler, tekniske logger,
          IP-avledet teknisk metadata, brukeragent, tidsstempler og feilhendelser
          som trengs for å drifte og sikre demoen.
        </p>
        <p>
          AI- og integrasjonsflyt er simulert eller demo-trygg med mindre noe
          annet er tydelig markert i grensesnittet.
        </p>
      </LegalSection>

      <LegalSection title="Lagring og sletting">
        <p>
          Demoarbeidsområder utløper automatisk. Utløpte demoer ryddes opp
          sammen med databaseposter og lagrede lokale demofiler.
        </p>
        <p>
          Tekniske logger kan lagres separat for sikkerhet, feilsøking og drift
          i en begrenset periode.
        </p>
      </LegalSection>

      <LegalSection title="Ansvarlig og kontakt">
        <p>
          Norvix AS er ansvarlig for drift av den offentlige demoen og den
          begrensede tekniske databehandlingen som trengs for dette formålet.
        </p>
        <p>
          Kontakt: Norvix AS. Bruk kontaktkanalen publisert på Norvix-nettstedet
          for personvern- eller sletteforespørsler knyttet til demoen.
        </p>
      </LegalSection>
    </PublicLegalLayout>
  );
}
