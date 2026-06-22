"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ClipboardList, Loader2, Plus, X } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { listNitInstitutions, listNitPortfolioInventions, listNitResearchers, saveNitPortfolioInvention } from "@/lib/queries";
import type { NitPortfolioInvention, NitPortfolioInventionPayload } from "@/lib/types";

const blank: NitPortfolioInventionPayload = { institutionId: "", title: "", summary: "", executiveSummary: null, technicalDescription: null, technologyArea: null, trl: null, commercialPotential: null, targetMarket: null, protectionStatus: null, creationDate: null, responsible: null, status: "Rascunho", researcherIds: [] };

export default function InventionsPage() {
  const queryClient = useQueryClient();
  const [form, setForm] = useState(blank);
  const [showForm, setShowForm] = useState(false);
  const query = useQuery({ queryKey: ["nit-inventions-portfolio"], queryFn: listNitPortfolioInventions });
  const institutions = useQuery({ queryKey: ["nit-institutions"], queryFn: listNitInstitutions });
  const researchers = useQuery({ queryKey: ["nit-researchers", form.institutionId], queryFn: () => listNitResearchers({ institutionId: form.institutionId, pageSize: 100 }), enabled: Boolean(form.institutionId) });
  const save = useMutation({
    mutationFn: () => saveNitPortfolioInvention(form),
    onSuccess: async (saved) => {
      queryClient.setQueryData<NitPortfolioInvention[]>(["nit-inventions-portfolio"], (current = []) => [saved, ...current.filter((item) => item.id !== saved.id)]);
      setForm(blank);
      setShowForm(false);
      await queryClient.invalidateQueries({ queryKey: ["nit-inventions-portfolio"] });
      await queryClient.refetchQueries({ queryKey: ["nit-inventions-portfolio"], type: "active" });
    }
  });

  return <div className="space-y-6">
    <div className="flex flex-wrap items-center justify-between gap-3"><div className="flex items-center gap-3"><div className="flex h-10 w-10 items-center justify-center rounded-md bg-amber-50 text-amber-700"><ClipboardList size={20} /></div><div><h1 className="text-2xl font-semibold">Invenções</h1><p className="text-sm text-slate-500">Portfólio tecnológico, maturidade e potencial comercial.</p></div></div><button className={showForm ? "btn-secondary" : "btn-primary"} onClick={() => setShowForm((current) => !current)}>{showForm ? <X size={16} /> : <Plus size={16} />}{showForm ? "Fechar formulário" : "Nova invenção"}</button></div>

    {showForm && <form onSubmit={(event) => { event.preventDefault(); save.mutate(); }} className="form-panel grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <label className="text-sm">Instituição<select required className="input mt-1" value={form.institutionId} onChange={(event) => setForm({ ...form, institutionId: event.target.value, researcherIds: [] })}><option value="">Selecione</option>{institutions.data?.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label className="text-sm md:col-span-2">Título<input required className="input mt-1" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} /></label>
      <label className="text-sm">Status<select className="input mt-1" value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value })}>{["Rascunho", "Em Avaliação", "Aprovada", "Rejeitada", "Protegida", "Licenciada"].map((item) => <option key={item}>{item}</option>)}</select></label>
      <label className="text-sm md:col-span-2">Resumo<textarea required className="input mt-1 min-h-24 py-2" value={form.summary} onChange={(event) => setForm({ ...form, summary: event.target.value })} /></label>
      <label className="text-sm md:col-span-2">Resumo executivo<textarea className="input mt-1 min-h-24 py-2" value={form.executiveSummary ?? ""} onChange={(event) => setForm({ ...form, executiveSummary: event.target.value || null })} /></label>
      <label className="text-sm md:col-span-2">Descrição técnica<textarea className="input mt-1 min-h-24 py-2" value={form.technicalDescription ?? ""} onChange={(event) => setForm({ ...form, technicalDescription: event.target.value || null })} /></label>
      <label className="text-sm">Área tecnológica<input className="input mt-1" value={form.technologyArea ?? ""} onChange={(event) => setForm({ ...form, technologyArea: event.target.value || null })} /></label>
      <label className="text-sm">TRL (1 a 9)<input type="number" min="1" max="9" className="input mt-1" value={form.trl ?? ""} onChange={(event) => setForm({ ...form, trl: event.target.value ? Number(event.target.value) : null })} /></label>
      <label className="text-sm">Data de criação<input type="date" className="input mt-1" value={form.creationDate ?? ""} onChange={(event) => setForm({ ...form, creationDate: event.target.value || null })} /></label>
      <label className="text-sm">Responsável<input className="input mt-1" value={form.responsible ?? ""} onChange={(event) => setForm({ ...form, responsible: event.target.value || null })} /></label>
      <label className="text-sm md:col-span-2">Potencial comercial<input className="input mt-1" value={form.commercialPotential ?? ""} onChange={(event) => setForm({ ...form, commercialPotential: event.target.value || null })} /></label>
      <label className="text-sm md:col-span-2">Mercado alvo<input className="input mt-1" value={form.targetMarket ?? ""} onChange={(event) => setForm({ ...form, targetMarket: event.target.value || null })} /></label>
      <label className="text-sm md:col-span-2">Pesquisadores<select multiple className="input mt-1 min-h-28 py-2" value={form.researcherIds} onChange={(event) => setForm({ ...form, researcherIds: Array.from(event.target.selectedOptions).map((option) => option.value) })}>{researchers.data?.items.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select><span className="mt-1 block text-xs text-slate-500">Use Ctrl para selecionar mais de um pesquisador.</span></label>
      <label className="text-sm md:col-span-2">Status de proteção<input className="input mt-1" value={form.protectionStatus ?? ""} onChange={(event) => setForm({ ...form, protectionStatus: event.target.value || null })} /></label>
      {save.isError && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700 md:col-span-2 xl:col-span-4">{getApiErrorMessage(save.error, "Não foi possível salvar a invenção.")}</p>}
      <div className="md:col-span-2 xl:col-span-4"><button className="btn-primary" disabled={save.isPending}>{save.isPending ? <Loader2 className="animate-spin" size={16} /> : <Plus size={16} />}Salvar invenção</button></div>
    </form>}

    <div className="data-panel overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="bg-slate-50 text-left"><th className="p-3">Tecnologia</th><th>Instituição</th><th>Área</th><th>TRL</th><th>Pesquisadores</th><th>Status</th></tr></thead><tbody>{query.data?.map((item) => <tr className="border-t border-line" key={item.id}><td className="p-3 font-medium">{item.title}<span className="block max-w-md truncate text-xs font-normal text-slate-500">{item.executiveSummary || item.summary}</span></td><td>{item.institutionName}</td><td>{item.technologyArea || "-"}</td><td>{item.trl || "-"}</td><td>{item.researchers.join(", ") || "-"}</td><td><StatusBadge status={item.status} /></td></tr>)}</tbody></table>
      {query.isLoading && <p className="p-8 text-center text-sm text-slate-500">Carregando invenções...</p>}
      {!query.isLoading && !query.data?.length && <p className="p-8 text-center text-sm text-slate-500">Nenhuma invenção cadastrada.</p>}
    </div>
  </div>;
}

function StatusBadge({ status }: { status: string }) {
  const tone = status === "Licenciada" ? "bg-emerald-50 text-emerald-700" : status === "Rejeitada" ? "bg-red-50 text-red-700" : status === "Rascunho" ? "bg-slate-100 text-slate-700" : "bg-blue-50 text-blue-700";
  return <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${tone}`}>{status}</span>;
}
