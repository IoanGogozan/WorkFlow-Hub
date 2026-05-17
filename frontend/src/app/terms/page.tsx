import {
  LegalSection,
  PublicLegalLayout,
} from "@/components/public-legal-layout";

export default function TermsPage() {
  return (
    <PublicLegalLayout
      intro="These terms apply only to the public interactive demo of Norvix WorkFlow Hub."
      title="Terms of Use"
    >
      <LegalSection title="Permitted use">
        <p>
          You may use the demo to explore fictional workflow scenarios, including
          intake, AI-assisted review, case handling, document classification,
          delivery packages, public delivery links, audit trail, and integration
          status.
        </p>
      </LegalSection>

      <LegalSection title="No real customer data">
        <p>
          Do not submit personal data, confidential information, production
          customer data, credentials, secrets, contracts, or regulated content.
        </p>
        <p>
          Public upload is intentionally disabled in the demo. Use the provided
          sample document flow only.
        </p>
      </LegalSection>

      <LegalSection title="Demo limitations">
        <p>
          The demo is provided for evaluation and presentation. It is not a
          production SaaS service, and it does not include customer onboarding,
          billing, service-level commitments, real Microsoft Graph, real
          accounting integration, or real Fabric/Power BI integration.
        </p>
        <p>
          AI, Microsoft, accounting, and Fabric behavior is mock unless the
          interface clearly marks a capability as real-capable.
        </p>
      </LegalSection>

      <LegalSection title="Availability and cleanup">
        <p>
          Demo sessions are temporary and may expire, reset, or be removed at any
          time. Norvix AS may change, suspend, or remove the demo without notice.
        </p>
      </LegalSection>

      <LegalSection title="Contact">
        <p>
          Contact: Norvix AS. Use the contact channel published on the Norvix
          website for questions about the demo.
        </p>
      </LegalSection>
    </PublicLegalLayout>
  );
}
