"use client";

import { FormEvent, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, Pencil, Plus, Trash2 } from "lucide-react";
import { deleteNitInstitution, listNitInstitutions, saveNitInstitution } from "@/lib/queries";
import type { NitInstitution, NitInstitutionPayload } from "@/lib/types";

const empty: NitInstitutionPayload = { name: "", tradeName: null, cnpj: null, tier: "Intermediário", type: "Universidade", website: null, email: null, phone: null, contactName: null, status: "Ativa" };

export default function InstitutionsPage() {
  const qc = useQueryClient(); const query = useQuery({ queryKey: ["nit-institutions"], queryFn: listNitInstitutions });
  const [editing, setEditing] = useState<NitInstitution | null>(null); const [form, setForm] = useState(empty);
  const save = useMutation({ mutationFn: () => saveNitInstitution(form, editing?.id), onSuccess: () => { setEditing(null); setForm(empty); qc.invalidateQueries({ queryKey: ["nit-institutions"] }); } });
  const remove = useMutation({ mutationFn: deleteNitInstitution, onSuccess: () => qc.invalidateQueries({ queryKey: ["nit-institutions"] }) });
  function edit(x: NitInstitution) { setEditing(x); setForm({ name: x.name, tradeName: x.tradeName, cnpj: x.cnpj, tier: x.tier, type: x.type, website: x.website, email: x.email, phone: x.phone, contactName: x.contactName, status: x.status }); }
  return <div className="space-y-6"><Header icon={Building2} title="Instituições" subtitle="Organizações participantes do ecossistema de inovação." />
    <form onSubmit={(e) => { e.preventDefault(); save.mutate(); }} className="grid gap-3 rounded-lg border border-line bg-white p-4 shadow-panel md:grid-cols-4">
      <Field label="Nome" value={form.name} onChange={(v) => setForm({ ...form, name: v })} required /><Field label="Nome fantasia" value={form.tradeName ?? ""} onChange={(v) => setForm({ ...form, tradeName: v })} /><Field label="CNPJ" value={form.cnpj ?? ""} onChange={(v) => setForm({ ...form, cnpj: v })} />
      <label className="text-sm">Tipo<select className="input mt-1" value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })}>{["Universidade", "ICT", "Instituto", "Fundação", "Empresa", "Centro de Pesquisa", "Governo"].map(x => <option key={x}>{x}</option>)}</select></label>
      <Field label="Website" value={form.website ?? ""} onChange={(v) => setForm({ ...form, website: v })} /><Field label="E-mail" value={form.email ?? ""} onChange={(v) => setForm({ ...form, email: v })} /><Field label="Telefone" value={form.phone ?? ""} onChange={(v) => setForm({ ...form, phone: v })} /><Field label="Responsável" value={form.contactName ?? ""} onChange={(v) => setForm({ ...form, contactName: v })} />
      <div className="flex items-end gap-2 md:col-span-4"><button className="btn-primary" disabled={save.isPending}><Plus size={16} />{editing ? "Salvar alterações" : "Cadastrar instituição"}</button>{editing && <button type="button" className="btn-secondary" onClick={() => { setEditing(null); setForm(empty); }}>Cancelar</button>}</div>
    </form>
    <div className="overflow-hidden rounded-lg border border-line bg-white shadow-panel"><table className="w-full text-sm"><thead><tr className="bg-slate-50 text-left"><th className="p-3">Instituição</th><th>Tipo</th><th>CNPJ</th><th>Responsável</th><th className="text-right pr-3">Ações</th></tr></thead><tbody>{query.data?.map(x => <tr key={x.id} className="border-t border-line"><td className="p-3 font-medium">{x.tradeName || x.name}<span className="block text-xs font-normal text-slate-500">{x.name}</span></td><td>{x.type}</td><td>{x.cnpj || "-"}</td><td>{x.contactName || "-"}</td><td className="pr-3 text-right"><button title="Editar" className="icon-button" onClick={() => edit(x)}><Pencil size={16} /></button><button title="Desativar" className="icon-button ml-1 text-red-600" onClick={() => remove.mutate(x.id)}><Trash2 size={16} /></button></td></tr>)}</tbody></table>{!query.isLoading && !query.data?.length && <p className="p-8 text-center text-sm text-slate-500">Nenhuma instituição cadastrada.</p>}</div>
  </div>;
}
function Field({ label, value, onChange, required = false }: { label: string; value: string; onChange: (v: string) => void; required?: boolean }) { return <label className="text-sm">{label}<input className="input mt-1" value={value} onChange={(e) => onChange(e.target.value)} required={required} /></label>; }
function Header({ icon: Icon, title, subtitle }: { icon: typeof Building2; title: string; subtitle: string }) { return <div className="flex items-center gap-3"><div className="flex h-10 w-10 items-center justify-center rounded-md bg-teal-50 text-brand"><Icon size={20} /></div><div><h1 className="text-2xl font-semibold">{title}</h1><p className="text-sm text-slate-500">{subtitle}</p></div></div>; }
