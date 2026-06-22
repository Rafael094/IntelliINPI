"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building, Loader2, Plus, Trash2 } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { maskCnpj, maskPhone } from "@/lib/input-masks";
import { deleteNitCompany, listNitCompanies, listNitInstitutions, saveNitCompany } from "@/lib/queries";
import type { NitCompany, NitCompanyPayload } from "@/lib/types";

const blank: NitCompanyPayload = { institutionId: null, legalName: "", tradeName: null, cnpj: null, segment: null, size: null, contactName: null, email: null, phone: null, website: null, notes: null };

export default function CompaniesPage() {
  const queryClient = useQueryClient();
  const [form, setForm] = useState(blank);
  const query = useQuery({ queryKey: ["nit-companies"], queryFn: () => listNitCompanies() });
  const institutions = useQuery({ queryKey: ["nit-institutions"], queryFn: listNitInstitutions });
  const save = useMutation({
    mutationFn: () => saveNitCompany(form),
    onSuccess: async (saved) => {
      queryClient.setQueryData<NitCompany[]>(["nit-companies"], (current = []) => [...current, saved]);
      setForm(blank);
      await queryClient.invalidateQueries({ queryKey: ["nit-companies"] });
    }
  });
  const remove = useMutation({ mutationFn: deleteNitCompany, onSuccess: () => queryClient.invalidateQueries({ queryKey: ["nit-companies"] }) });

  return <div className="space-y-6">
    <div className="flex items-center gap-3"><div className="flex h-10 w-10 items-center justify-center rounded-md bg-indigo-50 text-indigo-700"><Building size={20} /></div><div><h1 className="text-2xl font-semibold">Empresas</h1><p className="text-sm text-slate-500">Relacionamento com parceiros e potenciais licenciados.</p></div></div>
    <form onSubmit={(event) => { event.preventDefault(); save.mutate(); }} className="form-panel grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <label className="text-sm">Instituição relacionada<select className="input mt-1" value={form.institutionId ?? ""} onChange={(event) => setForm({ ...form, institutionId: event.target.value || null })}><option value="">Sem vínculo</option>{institutions.data?.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <Field label="Razão social" required value={form.legalName} onChange={(value) => setForm({ ...form, legalName: value })} />
      <Field label="Nome fantasia" value={form.tradeName ?? ""} onChange={(value) => setForm({ ...form, tradeName: value || null })} />
      <Field label="CNPJ" value={form.cnpj ?? ""} onChange={(value) => setForm({ ...form, cnpj: maskCnpj(value) || null })} inputMode="numeric" maxLength={18} placeholder="00.000.000/0000-00" />
      <Field label="Segmento" value={form.segment ?? ""} onChange={(value) => setForm({ ...form, segment: value || null })} />
      <label className="text-sm">Porte<select className="input mt-1" value={form.size ?? ""} onChange={(event) => setForm({ ...form, size: event.target.value || null })}><option value="">Selecione</option>{["MEI", "Microempresa", "Pequena", "Média", "Grande"].map((item) => <option key={item}>{item}</option>)}</select></label>
      <Field label="Responsável" value={form.contactName ?? ""} onChange={(value) => setForm({ ...form, contactName: value || null })} />
      <Field label="E-mail" type="email" value={form.email ?? ""} onChange={(value) => setForm({ ...form, email: value || null })} />
      <Field label="Telefone" value={form.phone ?? ""} onChange={(value) => setForm({ ...form, phone: maskPhone(value) || null })} inputMode="tel" maxLength={15} placeholder="(00) 00000-0000" />
      <Field label="Website (opcional)" type="url" value={form.website ?? ""} onChange={(value) => setForm({ ...form, website: value || null })} placeholder="https://exemplo.com.br" />
      <label className="text-sm md:col-span-2">Observações<textarea className="input mt-1 min-h-20 py-2" value={form.notes ?? ""} onChange={(event) => setForm({ ...form, notes: event.target.value || null })} /></label>
      {save.isError && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 md:col-span-2 xl:col-span-4">{getApiErrorMessage(save.error, "Não foi possível cadastrar a empresa.")}</p>}
      <div className="md:col-span-2 xl:col-span-4"><button className="btn-primary" disabled={save.isPending}>{save.isPending ? <Loader2 className="animate-spin" size={16} /> : <Plus size={16} />}Cadastrar empresa</button></div>
    </form>
    <div className="data-panel overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="bg-slate-50 text-left"><th className="p-3">Empresa</th><th>CNPJ</th><th>Segmento</th><th>Porte</th><th>Contato</th><th>Contratos</th><th /></tr></thead><tbody>{query.data?.map((item) => <tr className="border-t border-line" key={item.id}><td className="p-3 font-medium">{item.tradeName || item.legalName}<span className="block text-xs font-normal text-slate-500">{item.legalName}</span></td><td>{maskCnpj(item.cnpj ?? "") || "-"}</td><td>{item.segment || "-"}</td><td>{item.size || "-"}</td><td>{item.contactName || item.email || "-"}</td><td>{item.contractsCount}</td><td><button title="Desativar" className="icon-button text-red-600" onClick={() => remove.mutate(item.id)}><Trash2 size={16} /></button></td></tr>)}</tbody></table>
      {query.isLoading && <p className="p-8 text-center text-sm text-slate-500">Carregando empresas...</p>}
      {!query.isLoading && !query.data?.length && <p className="p-8 text-center text-sm text-slate-500">Nenhuma empresa cadastrada.</p>}
    </div>
  </div>;
}

type FieldProps = { label: string; value: string; onChange: (value: string) => void; required?: boolean; type?: string; inputMode?: "numeric" | "tel"; maxLength?: number; placeholder?: string };
function Field({ label, value, onChange, required, type = "text", inputMode, maxLength, placeholder }: FieldProps) {
  return <label className="text-sm">{label}<input className="input mt-1" value={value} onChange={(event) => onChange(event.target.value)} required={required} type={type} inputMode={inputMode} maxLength={maxLength} placeholder={placeholder} /></label>;
}
