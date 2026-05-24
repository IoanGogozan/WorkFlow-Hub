type WhyThisMattersProps = {
  title?: string;
  children: React.ReactNode;
};

export function WhyThisMatters({
  title = "Hvorfor dette er nyttig",
  children,
}: WhyThisMattersProps) {
  return (
    <aside className="rounded-md border border-[#bfdbfe] bg-[#eff6ff] p-5">
      <h3 className="text-lg font-semibold text-[#162033]">{title}</h3>
      <div className="mt-3 space-y-3 text-sm leading-6 text-[#475569]">
        {children}
      </div>
    </aside>
  );
}
