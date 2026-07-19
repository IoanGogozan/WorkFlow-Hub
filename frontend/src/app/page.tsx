import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: { absolute: "Norvix WorkFlow Hub | Verifiable workflow automation demo" },
  description:
    "A portfolio demonstration that turns a fictional service request into a case, document, downstream system updates, and inspectable audit evidence.",
};

const capabilities = [
  {
    number: "01",
    title: "Tenant-scoped execution",
    body: "Every visitor receives a temporary workspace. Requests, artifacts, and evidence remain scoped to that demo tenant.",
  },
  {
    number: "02",
    title: "Worker-backed orchestration",
    body: "Persisted steps expose duration, attempt count, failure state, retry behavior, and idempotent downstream operations.",
  },
  {
    number: "03",
    title: "Verifiable integration evidence",
    body: "Each run links its request, Brreg result, case, PDF, SharePoint simulator history, ERP receipt, and audit events.",
  },
  {
    number: "04",
    title: "Human-controlled automation",
    body: "The broader application keeps review, approval, and exception handling visible instead of presenting automation as autonomous.",
  },
];

const walkthrough = [
  ["01", "Start safely", "Create an isolated workspace with fictional data and no registration."],
  ["02", "Run one request", "Watch the backend worker validate, create, synchronize, and record the flow."],
  ["03", "Inspect the evidence", "Open the exact run and verify every artifact and integration boundary."],
];

export default function HomePage() {
  return (
    <main className="min-h-screen overflow-x-clip bg-[#f3f1eb] text-[#14251f]">
      <header className="border-b border-[#173d32]/15 bg-[#f8f7f2]/95">
        <nav
          aria-label="Primary navigation"
          className="mx-auto flex max-w-7xl items-center justify-between px-5 py-5 sm:px-8 lg:px-12"
        >
          <Link className="flex items-center gap-3 font-semibold tracking-tight" href="/">
            <span className="grid size-9 place-items-center rounded-full bg-[#173d32] text-sm text-white">N</span>
            <span>Norvix WorkFlow Hub</span>
          </Link>
          <div className="flex items-center gap-3 sm:gap-6">
            <a
              className="hidden text-sm font-semibold text-[#40554d] transition hover:text-[#14251f] sm:block"
              href="https://github.com/IoanGogozan/WorkFlow-Hub"
              rel="noreferrer"
              target="_blank"
            >
              Source
            </a>
            <Link
              className="rounded-full bg-[#d8613c] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[#bd4e2e]"
              href="/demo"
            >
              Launch demo <span aria-hidden="true">→</span>
            </Link>
          </div>
        </nav>
      </header>

      <section className="relative border-b border-[#173d32]/15">
        <div className="mx-auto grid max-w-7xl gap-12 px-5 py-16 sm:px-8 sm:py-24 lg:grid-cols-[1.08fr_0.92fr] lg:items-center lg:px-12 lg:py-28">
          <div>
            <p className="mb-6 flex items-center gap-3 text-xs font-bold uppercase tracking-[0.2em] text-[#527267]">
              <span className="h-px w-8 bg-[#d8613c]" /> Verifiable integration demo
            </p>
            <h1 className="max-w-4xl text-5xl font-semibold leading-[0.98] tracking-[-0.045em] text-[#14251f] sm:text-6xl lg:text-7xl">
              One request.
              <br />
              Every handoff
              <br />
              made visible.
            </h1>
            <p className="mt-7 max-w-2xl text-lg leading-8 text-[#52655e]">
              WorkFlow Hub turns a fictional service email into a structured case, document, downstream system updates, and an audit trail you can inspect—without pretending demo adapters are customer integrations.
            </p>
            <div className="mt-9 flex flex-wrap gap-3">
              <Link
                className="rounded-full bg-[#173d32] px-6 py-3.5 text-sm font-semibold text-white transition hover:bg-[#245747]"
                href="/demo"
              >
                Run the workflow <span aria-hidden="true">→</span>
              </Link>
              <a
                className="rounded-full border border-[#173d32]/25 bg-white/40 px-6 py-3.5 text-sm font-semibold text-[#173d32] transition hover:bg-white"
                href="#walkthrough"
              >
                See the walkthrough
              </a>
            </div>
            <div className="mt-9 flex flex-wrap gap-x-6 gap-y-2 text-sm text-[#5c6f68]">
              <span>✓ No registration</span>
              <span>✓ Fictional data</span>
              <span>✓ Temporary workspace</span>
            </div>
          </div>

          <div className="relative">
            <div className="absolute -inset-5 -rotate-2 rounded-[2.25rem] bg-[#d8613c]/12" />
            <div className="relative overflow-hidden rounded-[1.75rem] border border-[#173d32]/20 bg-[#173d32] p-5 text-white shadow-2xl shadow-[#173d32]/20 sm:p-7">
              <div className="flex items-center justify-between border-b border-white/15 pb-5">
                <div>
                  <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#a8c8ba]">Live run topology</p>
                  <p className="mt-1 font-mono text-sm text-white/65">RUN-WFH-2026</p>
                </div>
                <span className="rounded-full bg-[#83c7a8]/15 px-3 py-1.5 text-xs font-semibold text-[#a9e0c6]">Inspectable</span>
              </div>
              <div className="mt-7 space-y-3">
                <FlowRow label="Request structured" meta="internal · persisted" status="01" />
                <FlowRow label="Organization checked" meta="Brreg · live or fallback" status="02" />
                <FlowRow label="Case + PDF created" meta="internal · tenant scoped" status="03" />
                <FlowRow label="Systems synchronized" meta="simulator + signed receiver" status="04" />
              </div>
              <div className="mt-7 grid grid-cols-3 gap-2 border-t border-white/15 pt-5 text-center">
                <Metric value="7" label="persisted steps" />
                <Metric value="2" label="downstream proofs" />
                <Metric value="1" label="audit timeline" />
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="bg-[#fffdf8] py-20 sm:py-28">
        <div className="mx-auto max-w-7xl px-5 sm:px-8 lg:px-12">
          <div className="grid gap-10 lg:grid-cols-[0.78fr_1.22fr] lg:gap-20">
            <div>
              <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#d8613c]">The operational problem</p>
              <h2 className="mt-4 text-4xl font-semibold leading-tight tracking-[-0.035em] sm:text-5xl">
                The tools work.
                <br />
                The gaps between them do not.
              </h2>
            </div>
            <div className="grid gap-6 text-lg leading-8 text-[#52655e] sm:grid-cols-2">
              <p>Email, customer records, documents, project systems, and reporting often contain parts of the same job.</p>
              <p>Employees bridge those tools by copying identifiers, moving files, updating status, and reconstructing what happened.</p>
              <p className="sm:col-span-2 sm:max-w-3xl">
                WorkFlow Hub demonstrates a bounded alternative: coordinate one request from receipt to downstream acknowledgement, while keeping every boundary and human review point visible.
              </p>
            </div>
          </div>
        </div>
      </section>

      <section className="border-y border-[#173d32]/15 bg-[#e5ece5] py-20 sm:py-28">
        <div className="mx-auto max-w-7xl px-5 sm:px-8 lg:px-12">
          <div className="max-w-3xl">
            <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#527267]">Engineering evidence</p>
            <h2 className="mt-4 text-4xl font-semibold tracking-[-0.035em] sm:text-5xl">Built to be exercised, not just described.</h2>
            <p className="mt-5 text-lg leading-8 text-[#52655e]">The public story stays short. The technical application keeps the underlying state and evidence available for inspection.</p>
          </div>
          <div className="mt-12 grid border-l border-t border-[#173d32]/20 sm:grid-cols-2">
            {capabilities.map((capability) => (
              <article className="border-b border-r border-[#173d32]/20 p-6 sm:p-8" key={capability.number}>
                <span className="font-mono text-xs font-bold text-[#d8613c]">{capability.number}</span>
                <h3 className="mt-8 text-xl font-semibold">{capability.title}</h3>
                <p className="mt-3 leading-7 text-[#52655e]">{capability.body}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="bg-[#fffdf8] py-20 sm:py-28" id="walkthrough">
        <div className="mx-auto max-w-7xl px-5 sm:px-8 lg:px-12">
          <div className="flex flex-col justify-between gap-5 sm:flex-row sm:items-end">
            <div>
              <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#d8613c]">Three-minute walkthrough</p>
              <h2 className="mt-4 text-4xl font-semibold tracking-[-0.035em] sm:text-5xl">Follow one complete run.</h2>
            </div>
            <Link className="font-semibold text-[#173d32] underline decoration-[#d8613c] decoration-2 underline-offset-4" href="/technical">
              Explore technical views
            </Link>
          </div>
          <div className="mt-12 grid gap-px overflow-hidden rounded-2xl border border-[#173d32]/15 bg-[#173d32]/15 lg:grid-cols-3">
            {walkthrough.map(([number, title, body]) => (
              <article className="bg-[#f3f1eb] p-7 sm:p-9" key={number}>
                <span className="font-mono text-sm font-bold text-[#d8613c]">{number}</span>
                <h3 className="mt-12 text-2xl font-semibold">{title}</h3>
                <p className="mt-4 leading-7 text-[#52655e]">{body}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="bg-[#173d32] py-20 text-white sm:py-24">
        <div className="mx-auto grid max-w-7xl gap-12 px-5 sm:px-8 lg:grid-cols-[0.8fr_1.2fr] lg:px-12">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#a8c8ba]">Deliberate scope</p>
            <h2 className="mt-4 text-4xl font-semibold tracking-[-0.035em]">A portfolio sandbox, not a production platform.</h2>
            <p className="mt-5 leading-7 text-white/65">The limits are part of the demonstration. They are documented so a reviewer can distinguish engineering evidence from future production work.</p>
          </div>
          <div className="grid gap-8 sm:grid-cols-2">
            <ScopeList
              title="Implemented now"
              items={["Tenant-scoped application state", "Worker and retry flow", "Brreg live/fallback evidence", "SharePoint simulator", "Signed ERP demo receiver", "Run-specific audit evidence"]}
            />
            <ScopeList
              muted
              title="Production extensions"
              items={["Customer identity and access", "Microsoft 365 connection", "Customer ERP credentials", "Governed AI provider", "Operational SLOs and alerts", "Customer legal assessment"]}
            />
          </div>
        </div>
      </section>

      <section className="bg-[#d8613c] py-16 text-white sm:py-20">
        <div className="mx-auto flex max-w-7xl flex-col justify-between gap-8 px-5 sm:px-8 lg:flex-row lg:items-center lg:px-12">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.2em] text-white/70">Ready to inspect it?</p>
            <h2 className="mt-3 text-4xl font-semibold tracking-[-0.035em] sm:text-5xl">Run the workflow. Then verify the evidence.</h2>
          </div>
          <div className="shrink-0">
            <Link className="inline-flex rounded-full bg-white px-6 py-3.5 text-sm font-bold text-[#173d32] transition hover:bg-[#f3f1eb]" href="/demo">
              Open the demo <span className="ml-2" aria-hidden="true">→</span>
            </Link>
            <p className="mt-3 text-xs text-white/70">Fictional data · No account · Ephemeral state</p>
          </div>
        </div>
      </section>

      <footer className="bg-[#102b23] text-white/65">
        <div className="mx-auto flex max-w-7xl flex-col gap-5 px-5 py-8 text-sm sm:px-8 md:flex-row md:items-center md:justify-between lg:px-12">
          <p><span className="font-semibold text-white">Norvix WorkFlow Hub</span> · Integration engineering portfolio demo</p>
          <nav aria-label="Footer navigation" className="flex flex-wrap gap-5">
            <Link href="/privacy">Privacy</Link>
            <Link href="/terms">Terms</Link>
            <a href="https://github.com/IoanGogozan/WorkFlow-Hub" rel="noreferrer" target="_blank">Source</a>
          </nav>
        </div>
      </footer>
    </main>
  );
}

function FlowRow({ label, meta, status }: { label: string; meta: string; status: string }) {
  return (
    <div className="flex items-center gap-4 rounded-xl border border-white/10 bg-white/[0.06] p-4">
      <span className="grid size-9 shrink-0 place-items-center rounded-full bg-[#d8613c] font-mono text-xs font-bold">{status}</span>
      <div className="min-w-0">
        <p className="font-semibold">{label}</p>
        <p className="mt-0.5 truncate font-mono text-xs text-white/50">{meta}</p>
      </div>
      <span className="ml-auto size-2 shrink-0 rounded-full bg-[#8ed0b1]" aria-label="Completed" />
    </div>
  );
}

function Metric({ value, label }: { value: string; label: string }) {
  return <div><strong className="block text-xl">{value}</strong><span className="text-[0.68rem] text-white/45">{label}</span></div>;
}

function ScopeList({ title, items, muted = false }: { title: string; items: string[]; muted?: boolean }) {
  return (
    <div className={muted ? "rounded-2xl border border-white/10 p-6" : "rounded-2xl bg-white/[0.07] p-6"}>
      <h3 className="font-semibold text-white">{title}</h3>
      <ul className="mt-5 space-y-3 text-sm text-white/65">
        {items.map((item) => <li className="flex gap-3" key={item}><span className={muted ? "text-white/25" : "text-[#8ed0b1]"}>●</span>{item}</li>)}
      </ul>
    </div>
  );
}
