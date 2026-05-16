type ErrorStateProps = {
  title?: string;
  message: string;
};

export function ErrorState({
  title = "Could not load data",
  message,
}: ErrorStateProps) {
  return (
    <div
      role="alert"
      className="rounded-md border border-[#fca5a5] bg-[#fef2f2] p-5 text-sm text-[#991b1b]"
    >
      <p className="font-semibold">{title}</p>
      <p className="mt-2 leading-6">{message}</p>
    </div>
  );
}
