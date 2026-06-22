"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, Loader2, Pencil, Plus, Trash2 } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { maskCnpj, maskPhone } from "@/lib/input-masks";
import { deleteNitInstitution, listNitInstitutions, saveNitInstitution } from "@/lib/queries";
import type { NitInstitution, NitInstitutionPayload } from "@/lib/types";

const empty: NitInstitutionPayload = {
  name: "", tradeName: null, cnpj: null, tier: "Intermediário", type: "Universidade",
  website: null, email: null, phone: null, contactName: null, status: "Ativa"
};

export default function InstitutionsPage() {
  const queryClient = useQueryClient();
  const query = useQuery({ queryKey: ["nit-institutions"], queryFn: listNitInstitutions });
  const [editing, setEditing] = useState<NitInstitution | null>(null);
  const [form, setForm] = useState(empty);

  const save = useMutation({
    mutationFn: () => saveNitInstitution(form, editing?.id),
    onSuccess: async (saved) => {
      queryClient.setQueryData<NitInstitution[]>(["nit-institutions"], (current = []) =>
        editing ? current.map((item) => item.id === saved.id ? saved : item) : [...current, saved]);
      setEditing(null);
      setForm(empty);
      await queryClient.invalidateQueries({ queryKey: ["nit-institutions"] });
    }
  });
  const remove = useMutation({
    mutationFn: deleteNitInstitution,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["nit-institutions"] })
  });

  function edit(item: NitInstitution) {
    setEditing(item);
    setForm({
      name: item.name, tradeName: item.tradeName, cnpj: maskCnpj(item.cnpj ?? ""), tier: item.tier,
      type: item.type, website: item.website, email: item.email, phone: maskPhone(item.phone ?? ""),
      contactName: item.contactName, status: item.status
    });
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-md bg-teal-50 text-brand"><Building2 size={20} /></div>
        <div><h1 className="text-2xl font-semibold">Instituições</h1><p className="text-sm text-slate-500">Organizações participantes do ecossistema de inovação.</p></div>
      </div>

      <form onSubmit={(event) => { event.preventDefault(); save.mutate(); }} className="form-panel grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Field label="Nome" value={form.name} onChange={(value) => setForm({ ...form, name: value })} required />
        <Field label="Nome fantasia" value={form.tradeName ?? ""} onChange={(value) => setForm({ ...form, tradeName: value || null })} />
        <Field label="CNPJ" value={form.cnpj ?? ""} onChange={(value) => setForm({ ...form, cnpj: maskCnpj(value) || null })} inputMode="numeric" maxLength={18} placeholder="00.000.000/0000-00" />
        <label className="text-sm">Tipo<select className="input mt-1" value={form.type} onChange={(event) => setForm({ ...form, type: event.target.value })}>{["Universidade", "ICT", "Instituto", "Fundação", "Empresa", "Centro de Pesquisa", "Governo"].map((item) => <option key={item}>{item}</option>)}</select></label>
        <Field label="Website (opcional)" value={form.website ?? ""} onChange={(value) => setForm({ ...form, website: value || null })} type="url" placeholder="https://exemplo.com.br" />
        <Field label="E-mail" value={form.email ?? ""} onChange={(value) => setForm({ ...form, email: value || null })} type="email" />
        <Field label="Telefone" value={form.phone ?? ""} onChange={(value) => setForm({ ...form, phone: maskPhone(value) || null })} inputMode="tel" maxLength={15} placeholder="(00) 00000-0000" />
        <Field label="Responsável" value={form.contactName ?? ""} onChange={(value) => setForm({ ...form, contactName: value || null })} />
        {save.isError && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 md:col-span-2 xl:col-span-4">{getApiErrorMessage(save.error, "Não foi possível salvar a instituição.")}</p>}
        <div className="flex gap-2 md:col-span-2 xl:col-span-4">
          <button className="btn-primary" disabled={save.isPending}>{save.isPending ? <Loader2 className="animate-spin" size={16} /> : <Plus size={16} />}{editing ? "Salvar alterações" : "Cadastrar instituição"}</button>
          {editing && <button type="button" className="btn-secondary" onClick={() => { setEditing(null); setForm(empty); }}>Cancelar</button>}
        </div>
      </form>

      <div className="data-panel overflow-x-auto">
        <table className="min-w-full text-sm"><thead><tr className="bg-slate-50 text-left"><th className="p-3">Instituição</th><th>Tipo</th><th>CNPJ</th><th>Responsável</th><th className="pr-3 text-right">Ações</th></tr></thead>
          <tbody>{query.data?.map((item) => <tr key={item.id} className="border-t border-line"><td className="p-3 font-medium">{item.tradeName || item.name}<span className="block text-xs font-normal text-slate-500">{item.name}</span></td><td>{item.type}</td><td>{maskCnpj(item.cnpj ?? "") || "-"}</td><td>{item.contactName || "-"}</td><td className="pr-3 text-right"><button title="Editar" className="icon-button" onClick={() => edit(item)}><Pencil size={16} /></button><button title="Desativar" className="icon-button ml-1 text-red-600" onClick={() => remove.mutate(item.id)}><Trash2 size={16} /></button></td></tr>)}</tbody>
        </table>
        {query.isLoading && <p className="p-8 text-center text-sm text-slate-500">Carregando instituições...</p>}
        {!query.isLoading && !query.data?.length && <p className="p-8 text-center text-sm text-slate-500">Nenhuma instituição cadastrada.</p>}
      </div>
    </div>
  );
}

type FieldProps = { label: string; value: string; onChange: (value: string) => void; required?: boolean; type?: string; inputMode?: "numeric" | "tel"; maxLength?: number; placeholder?: string };
function Field({ label, value, onChange, required, type = "text", inputMode, maxLength, placeholder }: FieldProps) {
  return <label className="text-sm">{label}<input className="input mt-1" value={value} onChange={(event) => onChange(event.target.value)} required={required} type={type} inputMode={inputMode} maxLength={maxLength} placeholder={placeholder} /></label>;
}
