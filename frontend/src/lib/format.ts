export function formatDateTime(value: string | null) {
  if (!value) {
    return "Aldri";
  }

  return new Intl.DateTimeFormat("nb-NO", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export function formatDate(value: string | null) {
  if (!value) {
    return "Ikke satt";
  }

  return new Intl.DateTimeFormat("nb-NO", {
    dateStyle: "medium",
  }).format(new Date(value));
}
