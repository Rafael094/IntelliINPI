type StatusBadgeProps = {
  status?: string | null;
};

const statusClasses: Record<string, string> = {
  Running: "border-sky-200 bg-sky-50 text-sky-700",
  Completed: "border-emerald-200 bg-emerald-50 text-emerald-700",
  CompletedWithWarnings: "border-amber-200 bg-amber-50 text-amber-700",
  Failed: "border-red-200 bg-red-50 text-red-700",
  StopRequested: "border-amber-200 bg-amber-50 text-amber-700",
  Stopped: "border-slate-200 bg-slate-50 text-slate-700"
};

export function StatusBadge({ status }: StatusBadgeProps) {
  const value = status || "Sem status";
  const className = statusClasses[value] ?? "border-slate-200 bg-slate-50 text-slate-700";
  const labels: Record<string, string> = {
    Running: "Em execução",
    Completed: "Concluído",
    CompletedWithWarnings: "Concluído com avisos",
    Failed: "Falhou",
    StopRequested: "Parada solicitada",
    Stopped: "Parado"
  };

  return <span className={`inline-flex rounded-full border px-2 py-1 text-xs font-medium ${className}`}>{labels[value] ?? value}</span>;
}

export function formatDateTime(value: string | null | undefined) {
  if (!value) {
    return "Não informado";
  }

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short"
  }).format(new Date(value));
}

export function formatDate(value: string | null | undefined) {
  if (!value) {
    return "Não informado";
  }

  return new Intl.DateTimeFormat("pt-BR", { timeZone: "UTC" }).format(new Date(`${value}T00:00:00Z`));
}
