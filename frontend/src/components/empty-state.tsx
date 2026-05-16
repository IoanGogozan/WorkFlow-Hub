type EmptyStateProps = {
  title: string;
  message: string;
  action?: React.ReactNode;
};

export function EmptyState({ title, message, action }: EmptyStateProps) {
  return (
    <div className="rounded-md border border-dashed border-[#cbd5e1] bg-[#f8fafc] p-6 text-center">
      <p className="font-semibold text-[#162033]">{title}</p>
      <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-[#64748b]">
        {message}
      </p>
      {action ? <div className="mt-4">{action}</div> : null}
    </div>
  );
}
