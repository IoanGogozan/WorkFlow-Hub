import Link from "next/link";

type PublicLegalLayoutProps = {
  title: string;
  intro: string;
  children: React.ReactNode;
};

export function PublicLegalLayout({
  title,
  intro,
  children,
}: PublicLegalLayoutProps) {
  return (
    <main className="min-h-screen bg-[#f5f7fb] text-[#162033]">
      <div className="border-b border-[#bfdbfe] bg-[#eff6ff] px-6 py-2 text-center text-xs font-semibold text-[#1d4ed8]">
        Offentlig demo - fiktive data - utløper automatisk
      </div>
      <div className="mx-auto max-w-4xl px-6 py-10">
        <nav className="mb-8 flex flex-wrap gap-4 text-sm font-semibold">
          <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/demo">
            Start demo
          </Link>
          <Link className="text-[#2563eb] hover:text-[#1d4ed8]" href="/">
            Norvix WorkFlow Hub
          </Link>
        </nav>

        <article className="rounded-md border border-[#d8deea] bg-white p-6 sm:p-8">
          <p className="text-sm font-semibold text-[#4f46e5]">
            Norvix WorkFlow Hub offentlig demo
          </p>
          <h1 className="mt-3 text-3xl font-semibold">{title}</h1>
          <p className="mt-4 text-sm leading-6 text-[#475569]">{intro}</p>
          <div className="mt-8 space-y-7 text-sm leading-6 text-[#475569]">
            {children}
          </div>
        </article>
      </div>
    </main>
  );
}

export function LegalSection({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <section>
      <h2 className="text-lg font-semibold text-[#162033]">{title}</h2>
      <div className="mt-2 space-y-3">{children}</div>
    </section>
  );
}
