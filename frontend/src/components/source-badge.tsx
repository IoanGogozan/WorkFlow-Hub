const sourceLabels: Record<string, string> = {
  email: "E-post",
  epost: "E-post",
  "e-post": "E-post",
  mockemail: "E-post",
  webform: "Skjema",
  form: "Skjema",
  skjema: "Skjema",
  mockform: "Skjema",
  api: "API",
  upload: "Dokument",
  documentupload: "Dokument",
  document: "Dokument",
  dokument: "Dokument",
  mockdocumentupload: "Dokument",
  manual: "Manuell",
  manuell: "Manuell",
};

const sourceStyles: Record<string, string> = {
  "E-post": "bg-[#eff6ff] text-[#1d4ed8] ring-[#bfdbfe]",
  Skjema: "bg-[#ecfdf5] text-[#047857] ring-[#a7f3d0]",
  API: "bg-[#fef3c7] text-[#92400e] ring-[#fcd34d]",
  Dokument: "bg-[#f1f5f9] text-[#475569] ring-[#cbd5e1]",
  Manuell: "bg-[#fdf2f8] text-[#be185d] ring-[#fbcfe8]",
};

type SourceBadgeProps = {
  source: string;
};

export function SourceBadge({ source }: SourceBadgeProps) {
  const normalized = source.replace(/\s/g, "").toLowerCase();
  const label = sourceLabels[normalized] ?? source;
  const style =
    sourceStyles[label] ?? "bg-[#eef2ff] text-[#3730a3] ring-[#c7d2fe]";

  return (
    <span
      className={`inline-flex rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ${style}`}
    >
      {label}
    </span>
  );
}
