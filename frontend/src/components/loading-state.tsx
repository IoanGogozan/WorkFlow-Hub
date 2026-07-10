type LoadingStateProps = {
  label?: string;
};

export function LoadingState({ label = "Laster data" }: LoadingStateProps) {
  return (
    <div
      aria-live="polite"
      className="rounded-md border border-[#d8deea] bg-white p-6 text-sm font-medium text-[#475569]"
      role="status"
    >
      {label}...
    </div>
  );
}
