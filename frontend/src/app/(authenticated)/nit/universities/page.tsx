"use client";

import { FormEvent, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, Save, Trash2 } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { createNitUniversity, deleteNitUniversity, listNitUniversities, updateNitUniversity } from "@/lib/queries";
import type { NitUniversity, NitUniversityPayload } from "@/lib/types";

export default function NitUniversitiesPage() {
  const queryClient = useQueryClient();
  const universitiesQuery = useQuery({ queryKey: ["nit-universities"], queryFn: listNitUniversities });
  const [editing, setEditing] = useState<NitUniversity | null>(null);

  const createMutation = useMutation({
    mutationFn: createNitUniversity,
    onSuccess: async () => {
      setEditing(null);
      await queryClient.invalidateQueries({ queryKey: ["nit-universities"] });
    }
  });
  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: NitUniversityPayload }) => updateNitUniversity(id, payload),
    onSuccess: async () => {
      setEditing(null);
      await queryClient.invalidateQueries({ queryKey: ["nit-universities"] });
    }
  });
  const deleteMutation = useMutation({
    mutationFn: deleteNitUniversity,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["nit-universities"] })
  });

  function handleSubmit(payload: NitUniversityPayload) {
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
        <h1 className="text-2xl font-semibold text-ink">Universidades</h1>
        <p className="mt-1 text-sm text-slate-500">Cadastro de instituições vinculadas ao módulo NIT / INOVA+.</p>
      </div>

      {error ? (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {getApiErrorMessage(error, "Não foi possível salvar a universidade.")}
        </div>
      ) : null}

      <UniversityForm key={editing?.id ?? "new-university"} initialValue={editing} isSubmitting={createMutation.isPending || updateMutation.isPending} onCancel={() => setEditing(null)} onSubmit={handleSubmit} />

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {universitiesQuery.isLoading ? <State text="Carregando universidades..." loading /> : null}
        {universitiesQuery.isError ? <State text="Não foi possível carregar universidades." /> : null}
        {universitiesQuery.data?.length === 0 ? <State text="Nenhuma universidade cadastrada." /> : null}
        {universitiesQuery.data && universitiesQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Nome</th>
                  <th className="px-4 py-3">CNPJ</th>
                  <th className="px-4 py-3">Nível</th>
                  <th className="px-4 py-3">ID</th>
                  <th className="px-4 py-3 text-right">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {universitiesQuery.data.map((university) => (
                  <tr key={university.id} className="hover:bg-slate-50">
                    <td className="min-w-72 px-4 py-3 font-medium text-ink">{university.name}</td>
                    <td className="px-4 py-3 text-slate-600">{university.cnpj ?? "Não informado"}</td>
                    <td className="px-4 py-3 text-slate-600">{university.tier}</td>
                    <td className="max-w-60 truncate px-4 py-3 font-mono text-xs text-slate-500" title={university.id}>{university.id}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="inline-flex gap-2">
                        <button onClick={() => setEditing(university)} className="h-9 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100">Editar</button>
                        <button onClick={() => deleteMutation.mutate(university.id)} className="inline-flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-red-700 hover:bg-red-50">
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

function UniversityForm({
  initialValue,
  isSubmitting,
  onSubmit,
  onCancel
}: {
  initialValue: NitUniversity | null;
  isSubmitting: boolean;
  onSubmit: (payload: NitUniversityPayload) => void;
  onCancel: () => void;
}) {
  const [name, setName] = useState(initialValue?.name ?? "");
  const [cnpj, setCnpj] = useState(initialValue?.cnpj ?? "");
  const [tier, setTier] = useState(initialValue?.tier ?? "Nascente");

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({
      name: name.trim(),
      cnpj: cnpj.trim() || null,
      tier: tier.trim()
    });
  }

  return (
    <form className="rounded-lg border border-line bg-white p-4 shadow-panel" onSubmit={submit}>
      <div className="grid gap-4 lg:grid-cols-3">
        <Field label="Nome">
          <input value={name} onChange={(event) => setName(event.target.value)} required className="input" />
        </Field>
        <Field label="CNPJ">
          <input value={cnpj} onChange={(event) => setCnpj(event.target.value)} className="input" />
        </Field>
        <Field label="Nível">
          <input value={tier} onChange={(event) => setTier(event.target.value)} required className="input" />
        </Field>
      </div>
      <div className="mt-4 flex gap-2">
        <button disabled={isSubmitting} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
          <Save size={16} />
          {initialValue ? "Salvar alterações" : "Criar universidade"}
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

function State({ text, loading = false }: { text: string; loading?: boolean }) {
  return <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">{loading ? <Loader2 className="animate-spin" size={18} /> : null}{text}</div>;
}
