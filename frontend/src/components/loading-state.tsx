type LoadingStateProps = {
  label?: string;
};

export function LoadingState({ label = "Loading data" }: LoadingStateProps) {
  return (
    <div className="rounded-md border border-[#d8deea] bg-white p-6 text-sm font-medium text-[#475569]">
      {label}...
    </div>
  );
}
