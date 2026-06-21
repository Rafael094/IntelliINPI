"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertTriangle, CalendarClock, CheckCircle2, Clock, Loader2, Save, Trash2 } from "lucide-react";
import { formatDate } from "@/components/status-badge";
import { getApiErrorMessage } from "@/lib/api-error";
import { createDeadline, deleteDeadline, listClients, listDeadlines, listInpiDeadlines, listNitInventions, listOperationalDeadlines, updateDeadline } from "@/lib/queries";
import type { Client, Deadline, DeadlinePayload, InpiDeadline, NitInvention, OperationalDeadline } from "@/lib/types";

const deadlineTypes = [
  { value: "INPIRequirement", label: "Exigência INPI" },
  { value: "Opposition", label: "Oposição" },
  { value: "Appeal", label: "Recurso" },
  { value: "Annuity", label: "Anuidade" },
  { value: "Renewal", label: "Renovação" },
  { value: "ContractExpiration", label: "Vencimento de contrato" },
  { value: "Other", label: "Outro" }
];

export default function DeadlinesPage() {
  const queryClient = useQueryClient();
  const operationalDeadlinesQuery = useQuery({
    queryKey: ["deadlines", "operational"],
    queryFn: () => listOperationalDeadlines({ daysAhead: 365, includeManualReview: true })
  });
  const deadlinesQuery = useQuery({ queryKey: ["deadlines"], queryFn: listDeadlines });
  const inpiDeadlinesQuery = useQuery({ queryKey: ["inpi-deadlines"], queryFn: () => listInpiDeadlines({ daysAhead: 120 }) });
  const clientsQuery = useQuery({ queryKey: ["clients"], queryFn: listClients });
  const inventionsQuery = useQuery({ queryKey: ["nit-inventions"], queryFn: listNitInventions });
  const [editing, setEditing] = useState<Deadline | null>(null);

  const createMutation = useMutation({
    mutationFn: createDeadline,
    onSuccess: async () => {
      setEditing(null);
      await queryClient.invalidateQueries({ queryKey: ["deadlines"] });
    }
  });
  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: DeadlinePayload }) => updateDeadline(id, payload),
    onSuccess: async () => {
      setEditing(null);
      await queryClient.invalidateQueries({ queryKey: ["deadlines"] });
    }
  });
  const deleteMutation = useMutation({
    mutationFn: deleteDeadline,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["deadlines"] })
  });

  function handleSubmit(payload: DeadlinePayload) {
    if (editing) {
      updateMutation.mutate({ id: editing.id, payload });
      return;
    }

    createMutation.mutate(payload);
  }

  const error = createMutation.error ?? updateMutation.error;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Prazos</h1>
        <p className="mt-1 text-sm text-slate-500">Controle operacional de exigências, recursos, renovações e vencimentos.</p>
      </div>

      {error ? (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {getApiErrorMessage(error, "Não foi possível salvar o prazo.")}
        </div>
      ) : null}

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        <div className="border-b border-line px-4 py-3">
          <h2 className="text-base font-semibold text-ink">Agenda operacional</h2>
          <p className="mt-1 text-sm text-slate-500">Prazos manuais, prazos de PI e prazos calculados das marcas monitoradas.</p>
        </div>
        {operationalDeadlinesQuery.isLoading ? <State text="Carregando agenda operacional..." loading /> : null}
        {operationalDeadlinesQuery.isError ? <State text="Não foi possível carregar a agenda operacional." /> : null}
        {operationalDeadlinesQuery.data ? <OperationalDeadlineSummary items={operationalDeadlinesQuery.data} /> : null}
        {operationalDeadlinesQuery.data && operationalDeadlinesQuery.data.length > 0 ? <OperationalDeadlineTable items={operationalDeadlinesQuery.data} /> : null}
        {operationalDeadlinesQuery.data?.length === 0 ? <State text="Nenhum prazo operacional encontrado." /> : null}
      </section>

      <DeadlineForm
        key={editing?.id ?? "new-deadline"}
        clients={clientsQuery.data ?? []}
        inventions={inventionsQuery.data ?? []}
        initialValue={editing}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
        onCancel={() => setEditing(null)}
        onSubmit={handleSubmit}
      />

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        <div className="border-b border-line px-4 py-3">
          <h2 className="text-base font-semibold text-ink">Prazos de PI</h2>
          <p className="mt-1 text-sm text-slate-500">Separação entre prazo oficial do INPI e prazo interno administrativo.</p>
        </div>
        {inpiDeadlinesQuery.isLoading ? <State text="Carregando prazos de PI..." loading /> : null}
        {inpiDeadlinesQuery.isError ? <State text="Não foi possível carregar prazos de PI." /> : null}
        {inpiDeadlinesQuery.data?.length === 0 ? <State text="Nenhum prazo de PI cadastrado." /> : null}
        {inpiDeadlinesQuery.data && inpiDeadlinesQuery.data.length > 0 ? <InpiDeadlineTable items={inpiDeadlinesQuery.data} /> : null}
      </section>

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {deadlinesQuery.isLoading ? <State text="Carregando prazos..." loading /> : null}
        {deadlinesQuery.isError ? <State text="Não foi possível carregar prazos." /> : null}
        {deadlinesQuery.data?.length === 0 ? <State text="Nenhum prazo cadastrado." /> : null}
        {deadlinesQuery.data && deadlinesQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Prazo</th>
                  <th className="px-4 py-3">Tipo</th>
                  <th className="px-4 py-3">Vencimento</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Vínculo</th>
                  <th className="px-4 py-3 text-right">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {deadlinesQuery.data.map((deadline) => (
                  <tr key={deadline.id} className="hover:bg-slate-50">
                    <td className="min-w-72 px-4 py-3">
                      <p className="font-medium text-ink">{deadline.title}</p>
                      <p className="mt-1 text-xs text-slate-500">{deadline.description ?? "Sem descrição"}</p>
                    </td>
                    <td className="px-4 py-3 text-slate-600">{formatDeadlineType(deadline.type)}</td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">{formatDate(deadline.dueDate)}</td>
                    <td className="px-4 py-3 text-slate-600">{deadline.status}</td>
                    <td className="min-w-56 px-4 py-3 text-slate-600">{deadline.clientName ?? deadline.trademarkProcessNumber ?? deadline.inventionTitle ?? "Sem vínculo"}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="inline-flex gap-2">
                        <button onClick={() => setEditing(deadline)} className="h-9 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100">Editar</button>
                        <button onClick={() => deleteMutation.mutate(deadline.id)} className="inline-flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-red-700 hover:bg-red-50">
                          <Trash2 size={15} />
                          Excluir
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : null}
      </section>
    </div>
  );
}

function OperationalDeadlineSummary({ items }: { items: OperationalDeadline[] }) {
  const overdue = items.filter((item) => item.status === "Overdue").length;
  const today = items.filter((item) => item.status === "DueToday").length;
  const next30 = items.filter((item) => item.status === "DueSoon").length;
  const manual = items.filter((item) => item.requiresManualReview).length;

  return (
    <div className="grid gap-3 border-b border-line p-4 md:grid-cols-4">
      <SummaryCard title="Vencidos" value={overdue} tone="danger" icon={<AlertTriangle size={18} />} />
      <SummaryCard title="Vence hoje" value={today} tone="warning" icon={<Clock size={18} />} />
      <SummaryCard title="Próximos 30 dias" value={next30} tone="default" icon={<CalendarClock size={18} />} />
      <SummaryCard title="Revisão manual" value={manual} tone="neutral" icon={<CheckCircle2 size={18} />} />
    </div>
  );
}

function SummaryCard({ title, value, icon, tone }: { title: string; value: number; icon: React.ReactNode; tone: "danger" | "warning" | "default" | "neutral" }) {
  const toneClass = tone === "danger"
    ? "border-red-200 bg-red-50 text-red-700"
    : tone === "warning"
      ? "border-amber-200 bg-amber-50 text-amber-700"
      : tone === "default"
        ? "border-sky-200 bg-sky-50 text-sky-700"
        : "border-slate-200 bg-slate-50 text-slate-700";

  return (
    <div className={`rounded-md border p-3 ${toneClass}`}>
      <div className="flex items-center justify-between">
        <span className="text-xs font-medium uppercase">{title}</span>
        {icon}
      </div>
      <div className="mt-2 text-2xl font-semibold">{value}</div>
    </div>
  );
}

function OperationalDeadlineTable({ items }: { items: OperationalDeadline[] }) {
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full text-left text-sm">
        <thead className="bg-slate-50 text-xs uppercase text-slate-500">
          <tr>
            <th className="px-4 py-3">Prazo</th>
            <th className="px-4 py-3">Origem</th>
            <th className="px-4 py-3">Processo / vínculo</th>
            <th className="px-4 py-3">Vencimento</th>
            <th className="px-4 py-3">Situação</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-line">
          {items.map((item) => (
            <tr key={item.id} className="hover:bg-slate-50">
              <td className="min-w-80 px-4 py-3">
                <p className="font-medium text-ink">{item.title}</p>
                <p className="mt-1 text-xs text-slate-500">{item.description ?? "Sem descrição"}</p>
              </td>
              <td className="px-4 py-3">
                <span className={`inline-flex rounded-full border px-2 py-1 text-xs font-medium ${scopeClass(item.scope, item.requiresManualReview)}`}>
                  {item.scope}
                </span>
                <p className="mt-1 text-xs text-slate-500">{item.source}</p>
              </td>
              <td className="min-w-56 px-4 py-3 text-slate-600">
                {item.trademarkProcessNumber ? (
                  <Link href={`/inpi/trademarks/${encodeURIComponent(item.trademarkProcessNumber)}`} className="font-medium text-brand hover:text-teal-800">
                    {item.trademarkProcessNumber}
                  </Link>
                ) : (
                  item.clientName ?? item.inventionTitle ?? item.ipAssetTitle ?? "Sem vínculo"
                )}
                {item.trademarkName ? <p className="mt-1 text-xs text-slate-500">{item.trademarkName}</p> : null}
              </td>
              <td className="whitespace-nowrap px-4 py-3 text-slate-600">{item.dueDate ? formatDate(item.dueDate) : "Sem data"}</td>
              <td className="px-4 py-3">
                <span className={`inline-flex rounded-full border px-2 py-1 text-xs font-medium ${statusClass(item.status)}`}>
                  {item.statusLabel}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function DeadlineForm({
  clients,
  inventions,
  initialValue,
  isSubmitting,
  onSubmit,
  onCancel
}: {
  clients: Client[];
  inventions: NitInvention[];
  initialValue: Deadline | null;
  isSubmitting: boolean;
  onSubmit: (payload: DeadlinePayload) => void;
  onCancel: () => void;
}) {
  const [title, setTitle] = useState(initialValue?.title ?? "");
  const [description, setDescription] = useState(initialValue?.description ?? "");
  const [dueDate, setDueDate] = useState(initialValue?.dueDate ?? new Date().toISOString().slice(0, 10));
  const [status, setStatus] = useState(initialValue?.status ?? "Pendente");
  const [type, setType] = useState(initialValue?.type ?? "Other");
  const [clientId, setClientId] = useState(initialValue?.clientId ?? "");
  const [trademarkId, setTrademarkId] = useState(initialValue?.trademarkId ?? "");
  const [inventionId, setInventionId] = useState(initialValue?.inventionId ?? "");

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({
      title: title.trim(),
      description: description.trim() || null,
      dueDate,
      status: status.trim(),
      type,
      clientId: clientId || null,
      trademarkId: trademarkId.trim() || null,
      inventionId: inventionId || null
    });
  }

  return (
    <form className="rounded-lg border border-line bg-white p-4 shadow-panel" onSubmit={submit}>
      <div className="grid gap-4 lg:grid-cols-3">
        <Field label="Título">
          <input value={title} onChange={(event) => setTitle(event.target.value)} required className="input" />
        </Field>
        <Field label="Tipo">
          <select value={type} onChange={(event) => setType(event.target.value)} className="input">
            {deadlineTypes.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
          </select>
        </Field>
        <Field label="Vencimento">
          <input type="date" value={dueDate} onChange={(event) => setDueDate(event.target.value)} required className="input" />
        </Field>
        <Field label="Status">
          <input value={status} onChange={(event) => setStatus(event.target.value)} required className="input" />
        </Field>
        <Field label="Cliente">
          <select value={clientId} onChange={(event) => setClientId(event.target.value)} className="input">
            <option value="">Sem cliente</option>
            {clients.map((client) => <option key={client.id} value={client.id}>{client.name}</option>)}
          </select>
        </Field>
        <Field label="Invenção">
          <select value={inventionId} onChange={(event) => setInventionId(event.target.value)} className="input">
            <option value="">Sem invenção</option>
            {inventions.map((invention) => <option key={invention.id} value={invention.id}>{invention.title}</option>)}
          </select>
        </Field>
      </div>
      <div className="mt-4 grid gap-4 lg:grid-cols-2">
        <Field label="ID da marca">
          <input value={trademarkId} onChange={(event) => setTrademarkId(event.target.value)} placeholder="UUID da marca, opcional" className="input" />
        </Field>
        <Field label="Descrição">
          <input value={description} onChange={(event) => setDescription(event.target.value)} className="input" />
        </Field>
      </div>
      <div className="mt-4 flex gap-2">
        <button disabled={isSubmitting} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
          <Save size={16} />
          {initialValue ? "Salvar alterações" : "Criar prazo"}
        </button>
        {initialValue ? <button type="button" onClick={onCancel} className="h-10 rounded-md border border-line px-4 text-sm text-slate-700 hover:bg-slate-100">Cancelar</button> : null}
      </div>
    </form>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-medium text-slate-700">{label}</span>
      {children}
    </label>
  );
}

function InpiDeadlineTable({ items }: { items: InpiDeadline[] }) {
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full text-left text-sm">
        <thead className="bg-slate-50 text-xs uppercase text-slate-500">
          <tr>
            <th className="px-4 py-3">Origem</th>
            <th className="px-4 py-3">Ativo</th>
            <th className="px-4 py-3">Tipo</th>
            <th className="px-4 py-3">Vencimento</th>
            <th className="px-4 py-3">Status</th>
            <th className="px-4 py-3">Observação</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-line">
          {items.map((deadline) => (
            <tr key={deadline.id}>
              <td className="px-4 py-3">
                <span className={`inline-flex rounded-full border px-2 py-1 text-xs font-medium ${deadline.isInternal ? "border-sky-200 bg-sky-50 text-sky-700" : "border-amber-200 bg-amber-50 text-amber-700"}`}>
                  {deadline.isInternal ? "Prazo interno" : "Prazo INPI"}
                </span>
              </td>
              <td className="min-w-64 px-4 py-3">
                <p className="font-medium text-ink">{deadline.ipAssetTitle}</p>
                <p className="text-xs text-slate-500">{deadline.inpiProcessNumber ?? "Sem processo"}</p>
              </td>
              <td className="px-4 py-3">{formatInpiDeadlineType(deadline.type)}</td>
              <td className="px-4 py-3">{formatDate(deadline.dueDate)}</td>
              <td className="px-4 py-3">
                <span className={deadline.status === "RevisaoManualNecessaria" ? "text-amber-700" : "text-slate-700"}>
                  {deadline.status === "RevisaoManualNecessaria" ? "Revisão manual necessária" : deadline.status}
                </span>
              </td>
              <td className="min-w-72 px-4 py-3 text-slate-600">{deadline.notes ?? deadline.legalBasis ?? "Não informado"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function State({ text, loading = false }: { text: string; loading?: boolean }) {
  return <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">{loading ? <Loader2 className="animate-spin" size={18} /> : null}{text}</div>;
}

function formatDeadlineType(value: string) {
  return deadlineTypes.find((item) => item.value === value)?.label ?? value;
}

function formatInpiDeadlineType(value: string) {
  const labels: Record<string, string> = {
    TrademarkOpposition: "Oposição de marca",
    TrademarkManifestation: "Manifestação de marca",
    TrademarkAppeal: "Recurso de marca",
    TrademarkRenewal: "Renovação de marca",
    PatentAnnuity: "Anuidade de patente",
    PatentOfficeActionResponse: "Resposta a exigência",
    PatentAppeal: "Recurso de patente",
    InternalDeadline: "Prazo interno",
    Other: "Outro"
  };
  return labels[value] ?? value;
}

function statusClass(status: string) {
  const classes: Record<string, string> = {
    Overdue: "border-red-200 bg-red-50 text-red-700",
    DueToday: "border-amber-200 bg-amber-50 text-amber-700",
    DueSoon: "border-sky-200 bg-sky-50 text-sky-700",
    Upcoming: "border-slate-200 bg-slate-50 text-slate-700",
    Completed: "border-emerald-200 bg-emerald-50 text-emerald-700",
    ManualReviewRequired: "border-purple-200 bg-purple-50 text-purple-700"
  };

  return classes[status] ?? "border-slate-200 bg-slate-50 text-slate-700";
}

function scopeClass(scope: string, requiresManualReview: boolean) {
  if (requiresManualReview) {
    return "border-purple-200 bg-purple-50 text-purple-700";
  }

  if (scope.includes("INPI")) {
    return "border-amber-200 bg-amber-50 text-amber-700";
  }

  if (scope.includes("interno")) {
    return "border-sky-200 bg-sky-50 text-sky-700";
  }

  return "border-slate-200 bg-slate-50 text-slate-700";
}
