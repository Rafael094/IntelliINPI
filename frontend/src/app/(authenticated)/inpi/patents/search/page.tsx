"use client";

import { FormEvent, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { ChevronDown, Loader2, Search } from "lucide-react";
import { formatDate } from "@/components/status-badge";
import {
  monitorPatent,
  searchInpiPatentsAdvanced,
  searchInpiPatentsBasic,
  type InpiPatentAdvancedSearchParams,
  type InpiPatentBasicSearchParams,
} from "@/lib/queries";
import type { InpiPatentResult, InpiSearchResponse } from "@/lib/types";

type SearchMode = "basic" | "advanced";

const emptyBasic: InpiPatentBasicSearchParams = {
  query: "",
  processNumber: "",
  gruNumber: "",
  protocolNumber: "",
  searchMode: "todasPalavras",
  searchField: "Titulo",
  page: 1,
  pageSize: 20,
};

const emptyAdvanced: InpiPatentAdvancedSearchParams = {
  processNumber: "",
  priorityNumber: "",
  pctNumber: "",
  startDate: "",
  endDate: "",
  priorityStartDate: "",
  priorityEndDate: "",
  pctDepositStartDate: "",
  pctDepositEndDate: "",
  pctPublicationStartDate: "",
  pctPublicationEndDate: "",
  ipcClass: "",
  ipcKeyword: "",
  title: "",
  abstract: "",
  applicant: "",
  applicantDocument: "",
  inventor: "",
  grantedOnly: false,
  page: 1,
  pageSize: 20,
};

export default function InpiPatentSearchPage() {
  const [mode, setMode] = useState<SearchMode>("basic");
  const [basic, setBasic] = useState(emptyBasic);
  const [advanced, setAdvanced] = useState(emptyAdvanced);
  const [data, setData] = useState<InpiSearchResponse<InpiPatentResult> | null>(null);
  const basicMutation = useMutation({ mutationFn: searchInpiPatentsBasic, onSuccess: setData });
  const advancedMutation = useMutation({ mutationFn: searchInpiPatentsAdvanced, onSuccess: setData });
  const monitorMutation = useMutation({ mutationFn: monitorPatent });
  const isPending = basicMutation.isPending || advancedMutation.isPending;
  const error = basicMutation.error || advancedMutation.error;

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (mode === "basic") basicMutation.mutate(cleanParams(basic));
    else advancedMutation.mutate(cleanParams(advanced));
  }

  function clear() {
    setData(null);
    if (mode === "basic") setBasic(emptyBasic);
    else setAdvanced(emptyAdvanced);
  }

  return (
    <div className="space-y-5">
      <header>
        <h1 className="text-2xl font-semibold text-ink">Buscar Patentes</h1>
        <p className="mt-1 text-sm text-slate-500">Consulta controlada ao INPI com atualização e fallback para a base local.</p>
      </header>

      <div className="flex border-b border-line" role="tablist" aria-label="Tipo de pesquisa">
        <Tab active={mode === "basic"} onClick={() => setMode("basic")}>Pesquisa Básica</Tab>
        <Tab active={mode === "advanced"} onClick={() => setMode("advanced")}>Pesquisa Avançada</Tab>
      </div>

      <form className="overflow-hidden rounded-lg border border-line bg-white shadow-panel" onSubmit={submit}>
        <div className="border-b border-line px-5 py-4">
          <h2 className="text-sm font-semibold text-ink">{mode === "basic" ? "Pesquisa básica" : "Pesquisa avançada"}</h2>
          <p className="mt-1 text-xs text-slate-500">Forneça somente as chaves necessárias. Evite frases ou palavras genéricas.</p>
        </div>
        {mode === "basic" ? <BasicForm value={basic} onChange={setBasic} /> : <AdvancedForm value={advanced} onChange={setAdvanced} />}
        <div className="flex items-center gap-2 border-t border-line px-5 py-4">
          <button disabled={isPending} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
            {isPending ? <Loader2 className="animate-spin" size={16} /> : <Search size={16} />}
            Pesquisar
          </button>
          <button type="button" onClick={clear} className="h-10 rounded-md border border-line px-4 text-sm text-slate-700 hover:bg-slate-50">Limpar</button>
        </div>
      </form>

      {error ? <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">Não foi possível concluir a pesquisa. Verifique o backend e tente novamente.</div> : null}
      {data ? <SourceAlert source={data.source} warning={data.warning} /> : null}
      {data ? <Results data={data} onMonitor={(item) => item.inpiProcessNumber ? monitorMutation.mutate(item.inpiProcessNumber) : undefined} /> : null}
    </div>
  );
}

function BasicForm({ value, onChange }: { value: InpiPatentBasicSearchParams; onChange: (value: InpiPatentBasicSearchParams) => void }) {
  return (
    <div className="grid gap-4 p-5 md:grid-cols-2 xl:grid-cols-3">
      <Field label="Número do pedido"><input className="input" value={value.processNumber} onChange={(e) => onChange({ ...value, processNumber: e.target.value })} /></Field>
      <Field label="Número de recolhimento da União - GRU"><input className="input" value={value.gruNumber} onChange={(e) => onChange({ ...value, gruNumber: e.target.value })} /></Field>
      <Field label="Número do protocolo"><input className="input" value={value.protocolNumber} onChange={(e) => onChange({ ...value, protocolNumber: e.target.value })} /></Field>
      <Field label="Contenha">
        <select className="input" value={value.searchMode} onChange={(e) => onChange({ ...value, searchMode: e.target.value as InpiPatentBasicSearchParams["searchMode"] })}>
          <option value="todasPalavras">Todas as palavras</option><option value="expExata">Expressão exata</option><option value="qualquerPalavra">Qualquer palavra</option><option value="aproximacao">Aproximação</option>
        </select>
      </Field>
      <Field label="Expressão da pesquisa"><input className="input" value={value.query} onChange={(e) => onChange({ ...value, query: e.target.value })} /></Field>
      <Field label="Pesquisar no campo">
        <select className="input" value={value.searchField} onChange={(e) => onChange({ ...value, searchField: e.target.value as InpiPatentBasicSearchParams["searchField"] })}>
          <option value="Titulo">Título</option><option value="Resumo">Resumo</option><option value="NomeDepositante">Nome do depositante</option><option value="NomeInventor">Nome do inventor</option><option value="CpfCnpjDepositante">CPF/CNPJ do depositante</option>
        </select>
      </Field>
      <PageSize value={value.pageSize} onChange={(pageSize) => onChange({ ...value, pageSize })} />
    </div>
  );
}

function AdvancedForm({ value, onChange }: { value: InpiPatentAdvancedSearchParams; onChange: (value: InpiPatentAdvancedSearchParams) => void }) {
  const set = (field: keyof InpiPatentAdvancedSearchParams, fieldValue: string | boolean | number) => onChange({ ...value, [field]: fieldValue });
  return (
    <div className="divide-y divide-line">
      <Section title="Números" open>
        <div className="grid gap-4 md:grid-cols-3">
          <Field label="Número do pedido"><input className="input" value={value.processNumber} onChange={(e) => set("processNumber", e.target.value)} /></Field>
          <Field label="País / número da prioridade"><input className="input" value={value.priorityNumber} onChange={(e) => set("priorityNumber", e.target.value)} /></Field>
          <Field label="Número do depósito PCT"><input className="input" value={value.pctNumber} onChange={(e) => set("pctNumber", e.target.value)} /></Field>
        </div>
        <label className="mt-4 inline-flex items-center gap-2 text-sm text-slate-700"><input type="checkbox" checked={value.grantedOnly} onChange={(e) => set("grantedOnly", e.target.checked)} /> Patente concedida</label>
      </Section>
      <Section title="Datas">
        <DateRange label="Data do depósito" from={value.startDate} to={value.endDate} setFrom={(v) => set("startDate", v)} setTo={(v) => set("endDate", v)} />
        <DateRange label="Data da prioridade" from={value.priorityStartDate} to={value.priorityEndDate} setFrom={(v) => set("priorityStartDate", v)} setTo={(v) => set("priorityEndDate", v)} />
        <DateRange label="Data do depósito PCT" from={value.pctDepositStartDate} to={value.pctDepositEndDate} setFrom={(v) => set("pctDepositStartDate", v)} setTo={(v) => set("pctDepositEndDate", v)} />
        <DateRange label="Data da publicação PCT" from={value.pctPublicationStartDate} to={value.pctPublicationEndDate} setFrom={(v) => set("pctPublicationStartDate", v)} setTo={(v) => set("pctPublicationEndDate", v)} />
      </Section>
      <Section title="Classificação">
        <div className="grid gap-4 md:grid-cols-2"><Field label="Classificação IPC"><input className="input" value={value.ipcClass} onChange={(e) => set("ipcClass", e.target.value)} /></Field><Field label="Palavra-chave IPC"><input className="input" value={value.ipcKeyword} onChange={(e) => set("ipcKeyword", e.target.value)} /></Field></div>
      </Section>
      <Section title="Palavra-chave">
        <div className="grid gap-4 md:grid-cols-2"><Field label="Título"><input className="input" value={value.title} onChange={(e) => set("title", e.target.value)} /></Field><Field label="Resumo"><input className="input" value={value.abstract} onChange={(e) => set("abstract", e.target.value)} /></Field></div>
      </Section>
      <Section title="Depositante / titular / inventor">
        <div className="grid gap-4 md:grid-cols-3"><Field label="Nome do depositante"><input className="input" value={value.applicant} onChange={(e) => set("applicant", e.target.value)} /></Field><Field label="CPF/CNPJ do depositante"><input className="input" value={value.applicantDocument} onChange={(e) => set("applicantDocument", e.target.value)} /></Field><Field label="Nome do inventor"><input className="input" value={value.inventor} onChange={(e) => set("inventor", e.target.value)} /></Field></div>
      </Section>
      <div className="p-5"><PageSize value={value.pageSize} onChange={(pageSize) => set("pageSize", pageSize)} /></div>
    </div>
  );
}

function Section({ title, open = false, children }: { title: string; open?: boolean; children: React.ReactNode }) {
  return <details open={open} className="group"><summary className="flex cursor-pointer list-none items-center justify-between bg-slate-50 px-5 py-3 text-sm font-semibold text-ink"><span>{title}</span><ChevronDown size={16} className="transition-transform group-open:rotate-180" /></summary><div className="space-y-4 p-5">{children}</div></details>;
}

function DateRange({ label, from, to, setFrom, setTo }: { label: string; from?: string; to?: string; setFrom: (v: string) => void; setTo: (v: string) => void }) {
  return <div className="grid items-end gap-3 md:grid-cols-[220px_1fr_1fr]"><span className="pb-2 text-sm font-medium text-slate-700">{label}</span><Field label="De"><input type="date" className="input" value={from} onChange={(e) => setFrom(e.target.value)} /></Field><Field label="Até"><input type="date" className="input" value={to} onChange={(e) => setTo(e.target.value)} /></Field></div>;
}

function Field({ label, children }: { label: string; children: React.ReactNode }) { return <label className="grid gap-1.5 text-sm font-medium text-slate-700"><span>{label}</span>{children}</label>; }
function PageSize({ value, onChange }: { value: number; onChange: (value: number) => void }) { return <Field label="Processos por página"><select className="input" value={value} onChange={(e) => onChange(Number(e.target.value))}>{[20, 40, 60, 80, 100].map((n) => <option key={n}>{n}</option>)}</select></Field>; }
function Tab({ active, onClick, children }: { active: boolean; onClick: () => void; children: React.ReactNode }) { return <button type="button" role="tab" aria-selected={active} onClick={onClick} className={`border-b-2 px-4 py-3 text-sm font-medium ${active ? "border-brand text-brand" : "border-transparent text-slate-500 hover:text-slate-800"}`}>{children}</button>; }

function cleanParams<T extends Record<string, unknown>>(params: T): T { return Object.fromEntries(Object.entries(params).filter(([, value]) => value !== "" && value !== undefined)) as T; }

function Results({ data, onMonitor }: { data: InpiSearchResponse<InpiPatentResult>; onMonitor: (item: InpiPatentResult) => void }) {
  if (data.items.length === 0) return <section className="rounded-lg border border-line bg-white p-8 text-center text-sm text-slate-500 shadow-panel">Nenhum processo encontrado.</section>;
  return <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel"><div className="flex items-center justify-between border-b border-line px-4 py-3"><div><h2 className="text-sm font-semibold text-ink">Resultado da pesquisa</h2><p className="text-xs text-slate-500">{data.totalItems.toLocaleString("pt-BR")} processos encontrados</p></div></div><div className="overflow-x-auto"><table className="min-w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase text-slate-500"><tr><th className="px-4 py-3">Pedido</th><th className="px-4 py-3">Depósito</th><th className="px-4 py-3">Título</th><th className="px-4 py-3">IPC</th><th className="px-4 py-3">Depositante</th><th className="px-4 py-3 text-right">Ação</th></tr></thead><tbody className="divide-y divide-line">{data.items.map((item) => <tr key={item.inpiProcessNumber ?? item.title}><td className="whitespace-nowrap px-4 py-3 font-medium">{item.inpiProcessNumber ?? "Manual"}</td><td className="whitespace-nowrap px-4 py-3">{formatDate(item.filingDate)}</td><td className="min-w-72 px-4 py-3">{item.title}</td><td className="px-4 py-3">{item.ipcClass ?? "Não informado"}</td><td className="min-w-52 px-4 py-3">{item.applicants.join(", ") || "Não informado"}</td><td className="px-4 py-3 text-right"><button disabled={!item.inpiProcessNumber} onClick={() => onMonitor(item)} className="h-9 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100 disabled:opacity-50">Monitorar</button></td></tr>)}</tbody></table></div></section>;
}

function SourceAlert({ source, warning }: { source: string; warning: string | null }) {
  const online = source === "OnlineInpi";
  const message = online ? "Resultado obtido no INPI online e atualizado na base local." : source === "OnlineFailedLocalFallback" ? "O INPI não respondeu com segurança. Resultado consultado na base local." : "Resultado consultado na base local.";
  return <div className={`rounded-md border px-4 py-3 text-sm ${online ? "border-emerald-200 bg-emerald-50 text-emerald-800" : "border-amber-200 bg-amber-50 text-amber-800"}`}>{message}{warning ? ` ${warning}` : ""}</div>;
}
