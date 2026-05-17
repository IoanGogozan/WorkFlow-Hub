import {
  LegalSection,
  PublicLegalLayout,
} from "@/components/public-legal-layout";

export default function PrivacyPage() {
  return (
    <PublicLegalLayout
      intro="This notice describes the limited data handling for the public interactive demo. It is not a customer production privacy notice."
      title="Privacy Notice"
    >
      <LegalSection title="Demo scope">
        <p>
          The demo uses fictional customers, cases, documents, integrations, and
          AI suggestions. It is intended for evaluating the workflow only.
        </p>
        <p>
          Do not upload or enter personal, sensitive, confidential, or customer
          production information in the demo.
        </p>
      </LegalSection>

      <LegalSection title="Data processed">
        <p>
          The demo may process a temporary demo session identifier, generated
          fictional workspace data, browser requests, technical logs, IP-derived
          technical metadata, user agent information, timestamps, and error
          events needed to operate and secure the demo.
        </p>
        <p>
          Demo AI and integration behavior is mock or demo-safe unless clearly
          marked otherwise in the interface.
        </p>
      </LegalSection>

      <LegalSection title="Retention and deletion">
        <p>
          Demo workspaces expire automatically. Expired demo sessions are cleaned
          up together with their database records and stored local demo files.
        </p>
        <p>
          Technical logs may be retained separately for security, troubleshooting,
          and service operation for a limited period.
        </p>
      </LegalSection>

      <LegalSection title="Controller and contact">
        <p>
          Norvix AS is responsible for the public demo operation and the limited
          technical data processed for that purpose.
        </p>
        <p>
          Contact: Norvix AS. Use the contact channel published on the Norvix
          website for privacy or deletion requests related to the demo.
        </p>
      </LegalSection>
    </PublicLegalLayout>
  );
}
