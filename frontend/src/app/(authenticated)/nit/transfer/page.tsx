"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { GripVertical, Handshake, Loader2, Plus } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { listNitCompanies, listNitPortfolioInventions, listNitTransferPipeline, moveNitTransferOpportunity, saveNitTransferOpportunity } from "@/lib/queries";

const stages = [
  { name: "Nova Tecnologia", header: "bg-sky-50 border-sky-200 text-sky-900", card: "border-l-sky-500" },
  { name: "Em Prospecção", header: "bg-blue-50 border-blue-200 text-blue-900", card: "border-l-blue-500" },
  { name: "Empresa Interessada", header: "bg-amber-50 border-amber-200 text-amber-900", card: "border-l-amber-500" },
  { name: "NDA Assinado", header: "bg-violet-50 border-violet-200 text-violet-900", card: "border-l-violet-500" },
  { name: "Negociação", header: "bg-orange-50 border-orange-200 text-orange-900", card: "border-l-orange-500" },
  { name: "Licenciamento", header: "bg-emerald-50 border-emerald-200 text-emerald-900", card: "border-l-emerald-500" },
  { name: "Em Operação", header: "bg-teal-50 border-teal-200 text-teal-900", card: "border-l-teal-500" },
  { name: "Gerando Royalties", header: "bg-green-50 border-green-200 text-green-900", card: "border-l-green-600" }
] as const;

export default function TransferPage() {
  const queryClient = useQueryClient();
  const pipeline = useQuery({ queryKey: ["nit-transfer"], queryFn: listNitTransferPipeline });
  const inventions = useQuery({ queryKey: ["nit-inventions-portfolio"], queryFn: listNitPortfolioInventions });
  const companies = useQuery({ queryKey: ["nit-companies"], queryFn: () => listNitCompanies() });
  const [inventionId, setInventionId] = useState("");
  const [companyId, setCompanyId] = useState("");
  const create = useMutation({
    mutationFn: () => saveNitTransferOpportunity({ inventionId, companyId: companyId || null, stage: stages[0].name, notes: null }),
    onSuccess: async () => { setInventionId(""); setCompanyId(""); await queryClient.invalidateQueries({ queryKey: ["nit-transfer"] }); }
  });
  const move = useMutation({
    mutationFn: ({ id, stage }: { id: string; stage: string }) => moveNitTransferOpportunity(id, stage),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["nit-transfer"] })
  });

  return <div className="space-y-5">
    <div className="flex items-center gap-3"><div className="flex h-10 w-10 items-center justify-center rounded-md bg-violet-50 text-violet-700"><Handshake size={20} /></div><div><h1 className="text-2xl font-semibold">Transferência Tecnológica</h1><p className="text-sm text-slate-500">Pipeline de prospecção, negociação e licenciamento.</p></div></div>
    <div className="form-panel flex flex-wrap items-end gap-3"><label className="min-w-64 flex-1 text-sm">Invenção<select className="input mt-1" value={inventionId} onChange={(event) => setInventionId(event.target.value)}><option value="">Selecione uma invenção</option>{inventions.data?.map((item) => <option key={item.id} value={item.id}>{item.title}</option>)}</select></label><label className="min-w-64 flex-1 text-sm">Empresa interessada<select className="input mt-1" value={companyId} onChange={(event) => setCompanyId(event.target.value)}><option value="">Empresa ainda não definida</option>{companies.data?.map((item) => <option key={item.id} value={item.id}>{item.tradeName || item.legalName}</option>)}</select></label><button className="btn-primary" disabled={!inventionId || create.isPending} onClick={() => create.mutate()}>{create.isPending ? <Loader2 className="animate-spin" size={16} /> : <Plus size={16} />}Adicionar ao pipeline</button></div>
    {(create.isError || move.isError) && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{getApiErrorMessage(create.error || move.error, "Não foi possível atualizar o pipeline.")}</p>}
    <div className="overflow-x-auto pb-4"><div className="grid min-w-[1500px] grid-cols-8 gap-3">{stages.map((stage) => {
      const items = pipeline.data?.filter((item) => item.stage === stage.name) ?? [];
      return <section key={stage.name} className="min-h-[440px] overflow-hidden rounded-md border border-line bg-slate-50" onDragOver={(event) => event.preventDefault()} onDrop={(event) => { const id = event.dataTransfer.getData("text/plain"); if (id) move.mutate({ id, stage: stage.name }); }}>
        <header className={`border-b p-3 ${stage.header}`}><p className="text-sm font-semibold">{stage.name}</p><p className="mt-0.5 text-xs opacity-70">{items.length} {items.length === 1 ? "item" : "itens"}</p></header>
        <div className="space-y-2 p-2">{items.map((item) => <article draggable onDragStart={(event) => { event.dataTransfer.effectAllowed = "move"; event.dataTransfer.setData("text/plain", item.id); }} key={item.id} className={`cursor-grab rounded-md border border-line border-l-4 bg-white p-3 shadow-sm transition hover:-translate-y-0.5 hover:shadow ${stage.card}`}><div className="flex gap-2"><GripVertical size={16} className="mt-0.5 shrink-0 text-slate-400" /><div className="min-w-0"><p className="text-sm font-semibold text-ink">{item.inventionTitle}</p><p className="mt-1 truncate text-xs text-slate-600">{item.companyName || "Sem empresa definida"}</p><p className="mt-1 truncate text-xs text-slate-400">{item.institutionName}</p></div></div></article>)}{!items.length && <p className="px-2 py-6 text-center text-xs text-slate-400">Arraste uma oportunidade para esta etapa</p>}</div>
      </section>;
    })}</div></div>
  </div>;
}
