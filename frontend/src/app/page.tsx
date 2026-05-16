const metrics = [
  { label: "New intakes", value: "12", tone: "border-l-[#2563eb]" },
  { label: "Waiting review", value: "7", tone: "border-l-[#b45309]" },
  { label: "Delivery links", value: "4", tone: "border-l-[#047857]" },
  { label: "Integration failures", value: "2", tone: "border-l-[#be123c]" },
];

const intakeItems = [
  {
    source: "Mock email",
    title: "Service request from coastal customer",
    status: "NeedsReview",
    detail: "AI found missing HMS attachment and suggested 3 tasks.",
  },
  {
    source: "Manual",
    title: "Documentation package for facility handover",
    status: "AIAnalyzed",
    detail: "Customer and organization number suggested.",
  },
  {
    source: "Mock form",
    title: "Urgent maintenance follow-up",
    status: "New",
    detail: "Awaiting first review and document upload.",
  },
];

const integrationStatuses = [
  {
    name: "Brønnøysundregistrene",
    status: "Connected",
    tone: "ok",
    detail: "Organization lookup adapter",
    lastSync: "12 min ago",
    failedSyncs: "0",
  },
  {
    name: "Microsoft Graph / SharePoint",
    status: "Mocked",
    tone: "mock",
    detail: "Document library sync path",
    lastSync: "Ready",
    failedSyncs: "0",
  },
  {
    name: "Tripletex Accounting",
    status: "Error",
    tone: "error",
    detail: "Mock failure available for retry testing",
    lastSync: "Failed 18 min ago",
    failedSyncs: "1",
  },
  {
    name: "Power BI / Fabric",
    status: "Mocked",
    tone: "mock",
    detail: "Metrics export available as CSV and JSON",
    lastSync: "Pending",
    failedSyncs: "0",
  },
];

const statusStyles = {
  ok: "bg-[#dcfce7] text-[#166534] ring-[#86efac]",
  mock: "bg-[#eff6ff] text-[#1d4ed8] ring-[#bfdbfe]",
  error: "bg-[#fee2e2] text-[#b91c1c] ring-[#fca5a5]",
};

export default function Home() {
  return (
    <main className="min-h-screen">
      <header className="border-b border-[#d8deea] bg-white">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 px-6 py-5 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-sm font-medium text-[#4f46e5]">
              Agder Drift & Service AS
            </p>
            <h1 className="mt-1 text-2xl font-semibold text-[#162033]">
              Norvix WorkFlow Hub
            </h1>
          </div>
          <nav aria-label="Main navigation" className="flex flex-wrap gap-2">
            {["Dashboard", "Intake", "Cases", "Documents", "Integrations"].map(
              (item) => (
                <a
                  key={item}
                  className="rounded-md border border-[#cbd5e1] bg-white px-3 py-2 text-sm font-medium text-[#334155] hover:bg-[#eef2ff]"
                  href="#"
                >
                  {item}
                </a>
              ),
            )}
          </nav>
        </div>
      </header>

      <div className="mx-auto grid max-w-7xl gap-6 px-6 py-6 lg:grid-cols-[1fr_340px]">
        <section aria-labelledby="dashboard-heading" className="space-y-6">
          <div>
            <p className="text-sm font-medium text-[#64748b]">Fra e-post og skjema til sak, dokumentasjon og rapportering</p>
            <h2
              id="dashboard-heading"
              className="mt-2 text-3xl font-semibold text-[#162033]"
            >
              Operational dashboard
            </h2>
          </div>

          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            {metrics.map((metric) => (
              <article
                key={metric.label}
                className={`border-l-4 ${metric.tone} rounded-md border-y border-r border-[#d8deea] bg-white p-4`}
              >
                <p className="text-sm font-medium text-[#64748b]">{metric.label}</p>
                <p className="mt-3 text-3xl font-semibold text-[#162033]">{metric.value}</p>
              </article>
            ))}
          </div>

          <section
            aria-labelledby="intake-heading"
            className="rounded-md border border-[#d8deea] bg-white"
          >
            <div className="border-b border-[#d8deea] px-5 py-4">
              <h3 id="intake-heading" className="text-lg font-semibold">
                Intake inbox
              </h3>
            </div>
            <div className="divide-y divide-[#e2e8f0]">
              {intakeItems.map((item) => (
                <article
                  key={item.title}
                  className="grid gap-3 p-5 md:grid-cols-4"
                >
                  <div>
                    <p className="text-sm font-medium text-[#64748b]">
                      {item.source}
                    </p>
                    <p className="mt-1 font-semibold text-[#162033]">
                      {item.title}
                    </p>
                  </div>
                  <p className="md:col-span-2 text-sm leading-6 text-[#475569]">
                    {item.detail}
                  </p>
                  <div className="md:text-right">
                    <span className="inline-flex rounded-md bg-[#eef2ff] px-2.5 py-1 text-sm font-medium text-[#3730a3]">
                      {item.status}
                    </span>
                  </div>
                </article>
              ))}
            </div>
          </section>
        </section>

        <aside className="space-y-6" aria-label="Operational side panel">
          <section className="rounded-md border border-[#d8deea] bg-white p-5">
            <h3 className="text-lg font-semibold">AI review queue</h3>
            <p className="mt-3 text-sm leading-6 text-[#475569]">
              Suggestions are waiting for human approval before case data,
              document metadata, or delivery status can change.
            </p>
            <button className="mt-4 rounded-md bg-[#2563eb] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d4ed8]">
              Open review queue
            </button>
          </section>

          <section className="rounded-md border border-[#d8deea] bg-white">
            <div className="border-b border-[#d8deea] px-5 py-4">
              <h3 className="text-lg font-semibold">Integrations</h3>
            </div>
            <div className="divide-y divide-[#e2e8f0]">
              {integrationStatuses.map((integration) => (
                <div key={integration.name} className="p-5">
                  <div className="flex items-center justify-between gap-3">
                    <p className="font-medium">{integration.name}</p>
                    <span
                      className={`rounded-md px-2 py-1 text-xs font-semibold ring-1 ${statusStyles[integration.tone as keyof typeof statusStyles]}`}
                    >
                      {integration.status}
                    </span>
                  </div>
                  <p className="mt-2 text-sm text-[#64748b]">
                    {integration.detail}
                  </p>
                  <div className="mt-3 grid grid-cols-2 gap-3 text-xs text-[#475569]">
                    <span>Last sync: {integration.lastSync}</span>
                    <span
                      className={
                        integration.failedSyncs === "0"
                          ? "text-[#166534]"
                          : "font-semibold text-[#b91c1c]"
                      }
                    >
                      Failed: {integration.failedSyncs}
                    </span>
                  </div>
                  {integration.status === "Error" ? (
                    <button className="mt-3 rounded-md border border-[#fca5a5] bg-[#fef2f2] px-3 py-1.5 text-xs font-semibold text-[#b91c1c] hover:bg-[#fee2e2]">
                      Retry sync
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          </section>
        </aside>
      </div>
      </main>
  );
}
