import Link from "next/link";

type BrandLockupProps = {
  subtitle?: string;
};

export function BrandLockup({ subtitle }: BrandLockupProps) {
  return (
    <Link
      className="group flex w-fit items-center gap-3 rounded-md focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-[#173d32]"
      href="/"
    >
      <span className="grid size-9 shrink-0 place-items-center rounded-full bg-[#173d32] text-sm font-semibold text-white transition group-hover:bg-[#245747]">
        N
      </span>
      <span>
        <span className="block font-semibold tracking-tight text-[#172033]">
          Norvix WorkFlow Hub
        </span>
        {subtitle ? (
          <span className="mt-0.5 block text-[0.68rem] font-semibold uppercase tracking-[0.14em] text-[#64748b]">
            {subtitle}
          </span>
        ) : null}
      </span>
    </Link>
  );
}
