"use client";

import { FormEvent, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Save } from "lucide-react";
import { inventionStatuses } from "@/lib/nit-format";
import { listNitUniversities } from "@/lib/queries";
import type { NitInvention, NitInventionPayload } from "@/lib/types";

export function InventionForm({
  initialValue,
  onSubmit,
  isSubmitting
}: {
  initialValue?: NitInvention;
  onSubmit: (payload: NitInventionPayload) => void;
  isSubmitting: boolean;
}) {
  const universitiesQuery = useQuery({ queryKey: ["nit-universities"], queryFn: listNitUniversities });
  const [universityId, setUniversityId] = useState(initialValue?.universityId ?? "");
  const [title, setTitle] = useState(initialValue?.title ?? "");
  const [summary, setSummary] = useState(initialValue?.summary ?? "");
  const [inventors, setInventors] = useState(initialValue?.inventors ?? "");
  const [depositDate, setDepositDate] = useState(initialValue?.depositDate ?? "");
  const [status, setStatus] = useState(initialValue?.status ?? "Draft");
  const [inpiProcessNumber, setInpiProcessNumber] = useState(initialValue?.inpiProcessNumber ?? initialValue?.patentNumber ?? "");
  const [localError, setLocalError] = useState<string | null>(null);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLocalError(null);

    const normalizedProcessNumber = inpiProcessNumber.trim() || null;

    onSubmit({
      universityId: universityId || null,
      title: title.trim(),
      summary: summary.trim(),
      inventors: inventors.trim(),
      depositDate: depositDate || null,
      status,
      patentNumber: normalizedProcessNumber,
      inpiProcessNumber: normalizedProcessNumber
    });
  }

  return (
    <form className="space-y-4 rounded-lg border border-line bg-white p-4 shadow-panel" onSubmit={handleSubmit}>
      {localError ? (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{localError}</div>
      ) : null}
      <div className="grid gap-4 lg:grid-cols-2">
        <Field label="Universidade">
          <select value={universityId} onChange={(event) => setUniversityId(event.target.value)} className="input">
            <option value="">Usar universidade do perfil</option>
            {universitiesQuery.data?.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name}
              </option>
            ))}
          </select>
          <p className="mt-1 text-xs text-slate-500">
            Cadastre universidades em NIT / INOVA+ antes de criar invenções como admin global.
          </p>
        </Field>
        <Field label="Status">
          <select value={status} onChange={(event) => setStatus(event.target.value)} className="input">
            {inventionStatuses.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </select>
        </Field>
      </div>
      <Field label="Título">
        <input value={title} onChange={(event) => setTitle(event.target.value)} required className="input" />
      </Field>
      <Field label="Resumo">
        <textarea value={summary} onChange={(event) => setSummary(event.target.value)} required rows={5} className="input min-h-32 py-2" />
      </Field>
      <Field label="Inventores">
        <textarea value={inventors} onChange={(event) => setInventors(event.target.value)} required rows={3} className="input min-h-24 py-2" />
      </Field>
      <div className="grid gap-4 lg:grid-cols-2">
        <Field label="Data de depósito">
          <input type="date" value={depositDate} onChange={(event) => setDepositDate(event.target.value)} className="input" />
        </Field>
        <Field label="Processo INPI">
          <input value={inpiProcessNumber} onChange={(event) => setInpiProcessNumber(event.target.value)} className="input" />
        </Field>
      </div>
      <button disabled={isSubmitting} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
        <Save size={16} />
        Salvar
      </button>
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
