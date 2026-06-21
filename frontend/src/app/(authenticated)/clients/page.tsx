"use client";

import { FormEvent, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, Save, Trash2 } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { createClient, deleteClient, listClients, updateClient } from "@/lib/queries";
import type { Client, ClientPayload } from "@/lib/types";

export default function ClientsPage() {
  const queryClient = useQueryClient();
  const clientsQuery = useQuery({ queryKey: ["clients"], queryFn: listClients });
  const [editing, setEditing] = useState<Client | null>(null);

  const createMutation = useMutation({
    mutationFn: createClient,
    onSuccess: async () => {
      setEditing(null);
      await queryClient.invalidateQueries({ queryKey: ["clients"] });
    }
  });
  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: ClientPayload }) => updateClient(id, payload),
    onSuccess: async () => {
      setEditing(null);
      await queryClient.invalidateQueries({ queryKey: ["clients"] });
    }
  });
  const deleteMutation = useMutation({
    mutationFn: deleteClient,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["clients"] })
  });

  function handleSubmit(payload: ClientPayload) {
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
        <h1 className="text-2xl font-semibold text-ink">Clientes</h1>
        <p className="mt-1 text-sm text-slate-500">Cadastro operacional de clientes e titulares acompanhados.</p>
      </div>

      {error ? (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {getApiErrorMessage(error, "Não foi possível salvar o cliente.")}
        </div>
      ) : null}

      <ClientForm key={editing?.id ?? "new-client"} initialValue={editing} isSubmitting={createMutation.isPending || updateMutation.isPending} onCancel={() => setEditing(null)} onSubmit={handleSubmit} />

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {clientsQuery.isLoading ? <State text="Carregando clientes..." loading /> : null}
        {clientsQuery.isError ? <State text="Não foi possível carregar clientes." /> : null}
        {clientsQuery.data?.length === 0 ? <State text="Nenhum cliente cadastrado." /> : null}
        {clientsQuery.data && clientsQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Nome</th>
                  <th className="px-4 py-3">Documento</th>
                  <th className="px-4 py-3">E-mail</th>
                  <th className="px-4 py-3">Telefone</th>
                  <th className="px-4 py-3 text-right">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {clientsQuery.data.map((client) => (
                  <tr key={client.id} className="hover:bg-slate-50">
                    <td className="min-w-60 px-4 py-3 font-medium text-ink">{client.name}</td>
                    <td className="px-4 py-3 text-slate-600">{client.documentNumber ?? "Não informado"}</td>
                    <td className="px-4 py-3 text-slate-600">{client.email ?? "Não informado"}</td>
                    <td className="px-4 py-3 text-slate-600">{client.phone ?? "Não informado"}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="inline-flex gap-2">
                        <button onClick={() => setEditing(client)} className="h-9 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100">Editar</button>
                        <button onClick={() => deleteMutation.mutate(client.id)} className="inline-flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-red-700 hover:bg-red-50">
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

function ClientForm({
  initialValue,
  isSubmitting,
  onSubmit,
  onCancel
}: {
  initialValue: Client | null;
  isSubmitting: boolean;
  onSubmit: (payload: ClientPayload) => void;
  onCancel: () => void;
}) {
  const [name, setName] = useState(initialValue?.name ?? "");
  const [documentNumber, setDocumentNumber] = useState(initialValue?.documentNumber ?? "");
  const [email, setEmail] = useState(initialValue?.email ?? "");
  const [phone, setPhone] = useState(initialValue?.phone ?? "");
  const [notes, setNotes] = useState(initialValue?.notes ?? "");

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({
      name: name.trim(),
      documentNumber: documentNumber.trim() || null,
      email: email.trim() || null,
      phone: phone.trim() || null,
      notes: notes.trim() || null
    });
  }

  return (
    <form className="rounded-lg border border-line bg-white p-4 shadow-panel" onSubmit={submit}>
      <div className="grid gap-4 lg:grid-cols-2">
        <Field label="Nome">
          <input value={name} onChange={(event) => setName(event.target.value)} required className="input" />
        </Field>
        <Field label="Documento">
          <input value={documentNumber} onChange={(event) => setDocumentNumber(event.target.value)} className="input" />
        </Field>
        <Field label="E-mail">
          <input type="email" value={email} onChange={(event) => setEmail(event.target.value)} className="input" />
        </Field>
        <Field label="Telefone">
          <input value={phone} onChange={(event) => setPhone(event.target.value)} className="input" />
        </Field>
      </div>
      <div className="mt-4">
        <Field label="Observações">
          <textarea value={notes} onChange={(event) => setNotes(event.target.value)} rows={3} className="input min-h-24 py-2" />
        </Field>
      </div>
      <div className="mt-4 flex gap-2">
        <button disabled={isSubmitting} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
          <Save size={16} />
          {initialValue ? "Salvar alterações" : "Criar cliente"}
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
