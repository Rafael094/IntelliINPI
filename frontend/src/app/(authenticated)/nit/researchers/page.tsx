"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, Plus, Search, Trash2, Users } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { maskCpf, maskOrcid, maskPhone } from "@/lib/input-masks";
import { deleteNitResearcher, listNitInstitutions, listNitResearchers, saveNitResearcher } from "@/lib/queries";
import type { NitResearcherPayload } from "@/lib/types";

const blank: NitResearcherPayload = { institutionId: "", name: "", cpf: null, email: null, phone: null, department: null, position: null, lattesUrl: null, orcid: null, specialties: null, technologyAreas: null };

export default function ResearchersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [form, setForm] = useState(blank);
  const institutions = useQuery({ queryKey: ["nit-institutions"], queryFn: listNitInstitutions });
  const query = useQuery({ queryKey: ["nit-researchers", search, page], queryFn: () => listNitResearchers({ search, page, pageSize: 20 }) });
  const save = useMutation({
    mutationFn: () => saveNitResearcher(form),
    onSuccess: async () => {
      setForm(blank);
      setPage(1);
      await queryClient.invalidateQueries({ queryKey: ["nit-researchers"] });
      await queryClient.refetchQueries({ queryKey: ["nit-researchers"], type: "active" });
    }
  });
  const remove = useMutation({ mutationFn: deleteNitResearcher, onSuccess: () => queryClient.invalidateQueries({ queryKey: ["nit-researchers"] }) });

  const fields = [
    { label: "Nome", key: "name", required: true },
    { label: "CPF", key: "cpf", mask: maskCpf, inputMode: "numeric", maxLength: 14, placeholder: "000.000.000-00" },
    { label: "E-mail", key: "email", type: "email" },
    { label: "Telefone", key: "phone", mask: maskPhone, inputMode: "tel", maxLength: 15, placeholder: "(00) 00000-0000" },
    { label: "Departamento", key: "department" },
    { label: "Cargo", key: "position" },
    { label: "Currículo Lattes (opcional)", key: "lattesUrl", type: "url", placeholder: "https://lattes.cnpq.br/..." },
    { label: "ORCID", key: "orcid", mask: maskOrcid, maxLength: 19, placeholder: "0000-0000-0000-0000" },
    { label: "Especialidades", key: "specialties" },
    { label: "Áreas tecnológicas", key: "technologyAreas" }
  ] as const;

  return <div className="space-y-6">
    <div className="flex items-center gap-3"><div className="flex h-10 w-10 items-center justify-center rounded-md bg-blue-50 text-blue-700"><Users size={20} /></div><div><h1 className="text-2xl font-semibold">Pesquisadores</h1><p className="text-sm text-slate-500">Inventores e pesquisadores vinculados às instituições.</p></div></div>
    <form onSubmit={(event) => { event.preventDefault(); save.mutate(); }} className="form-panel grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <label className="text-sm">Instituição<select className="input mt-1" required value={form.institutionId} onChange={(event) => setForm({ ...form, institutionId: event.target.value })}><option value="">Selecione</option>{institutions.data?.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      {fields.map((field) => <label className="text-sm" key={field.key}>{field.label}<input className="input mt-1" required={"required" in field && field.required} type={"type" in field ? field.type : "text"} inputMode={"inputMode" in field ? field.inputMode : undefined} maxLength={"maxLength" in field ? field.maxLength : undefined} placeholder={"placeholder" in field ? field.placeholder : undefined} value={form[field.key] ?? ""} onChange={(event) => { const value = "mask" in field ? field.mask(event.target.value) : event.target.value; setForm({ ...form, [field.key]: value || null }); }} /></label>)}
      {save.isError && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 md:col-span-2 xl:col-span-4">{getApiErrorMessage(save.error, "Não foi possível cadastrar o pesquisador.")}</p>}
      <div className="md:col-span-2 xl:col-span-4"><button className="btn-primary" disabled={save.isPending}>{save.isPending ? <Loader2 className="animate-spin" size={16} /> : <Plus size={16} />}Cadastrar pesquisador</button></div>
    </form>
    <div className="relative max-w-md"><Search className="absolute left-3 top-2.5 text-slate-400" size={17} /><input className="input pl-9" placeholder="Buscar por nome, CPF ou e-mail" value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} /></div>
    <div className="data-panel overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="bg-slate-50 text-left"><th className="p-3">Pesquisador</th><th>Instituição</th><th>Departamento</th><th>Áreas</th><th>Invenções</th><th /></tr></thead><tbody>{query.data?.items.map((item) => <tr key={item.id} className="border-t border-line"><td className="p-3 font-medium">{item.name}<span className="block text-xs font-normal text-slate-500">{item.email || maskCpf(item.cpf ?? "") || "Sem contato"}</span></td><td>{item.institutionName}</td><td>{item.department || "-"}</td><td>{item.technologyAreas || "-"}</td><td>{item.inventionsCount}</td><td><button title="Desativar" className="icon-button text-red-600" onClick={() => remove.mutate(item.id)}><Trash2 size={16} /></button></td></tr>)}</tbody></table>
      {query.isLoading && <p className="p-8 text-center text-sm text-slate-500">Carregando pesquisadores...</p>}
      {!query.isLoading && !query.data?.items.length && <p className="p-8 text-center text-sm text-slate-500">Nenhum pesquisador encontrado.</p>}
      <div className="flex items-center justify-between border-t border-line p-3 text-sm"><span>{query.data?.totalItems ?? 0} pesquisadores</span><div className="flex gap-2"><button className="btn-secondary" disabled={page === 1} onClick={() => setPage((current) => current - 1)}>Anterior</button><button className="btn-secondary" disabled={(query.data?.items.length ?? 0) < 20} onClick={() => setPage((current) => current + 1)}>Próxima</button></div></div>
    </div>
  </div>;
}
