"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useMutation } from "@tanstack/react-query";
import { Loader2, Search } from "lucide-react";
import { formatDate } from "@/components/status-badge";
import { monitorTrademark, searchInpiTrademarksAdvanced, searchInpiTrademarksBasic } from "@/lib/queries";
import type { InpiSearchResponse, InpiTrademarkResult } from "@/lib/types";

type SearchMode = "basic" | "advanced";

export default function InpiTrademarkSearchPage() {
  const params = useSearchParams();
  const [mode, setMode] = useState<SearchMode>("basic");
  const [brand, setBrand] = useState(params.get("query") ?? "");
  const [niceClass, setNiceClass] = useState("");
  const [exact, setExact] = useState(true);
  const [pageSize, setPageSize] = useState(20);
  const [liveOnly, setLiveOnly] = useState(false);
  const [presentation, setPresentation] = useState("0");
  const [nature, setNature] = useState("0");

  const searchMutation = useMutation<InpiSearchResponse<InpiTrademarkResult>, Error>({
    mutationFn: () => mode === "basic"
      ? searchInpiTrademarksBasic({ query: brand.trim(), niceClass: niceClass.trim() || undefined, exact, page: 1, pageSize })
      : searchInpiTrademarksAdvanced({ trademarkName: brand.trim(), niceClass: niceClass.trim() || undefined, exact, liveOnly, presentation, nature, page: 1, pageSize })
  });
  const monitorMutation = useMutation({ mutationFn: monitorTrademark });

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    searchMutation.mutate();
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Buscar Marcas</h1>
        <p className="mt-1 text-sm text-slate-500">Consulta vinculada ao fluxo público do INPI, com fallback local controlado.</p>
      </div>

      <section className="rounded-lg border border-line bg-white shadow-panel">
        <div className="flex border-b border-line">
          <button type="button" onClick={() => setMode("basic")} className={`h-11 px-4 text-sm font-medium ${mode === "basic" ? "border-b-2 border-brand text-brand" : "text-slate-500"}`}>
            Pesquisa Básica
          </button>
          <button type="button" onClick={() => setMode("advanced")} className={`h-11 px-4 text-sm font-medium ${mode === "advanced" ? "border-b-2 border-brand text-brand" : "text-slate-500"}`}>
            Pesquisa Avançada
          </button>
        </div>

        <form className="space-y-4 p-4" onSubmit={submit}>
          <div className="rounded-md border border-slate-200 bg-slate-50 p-3">
            {mode === "basic" ? (
              <div className="flex flex-wrap items-center gap-4 text-sm">
                <span className="font-medium text-slate-700">Tipo de Pesquisa:</span>
                <label className="inline-flex items-center gap-2"><input type="radio" checked={exact} onChange={() => setExact(true)} /> Exata</label>
                <label className="inline-flex items-center gap-2"><input type="radio" checked={!exact} onChange={() => setExact(false)} /> Radical</label>
              </div>
            ) : (
              <div className="flex flex-wrap items-center gap-4 text-sm">
                <span className="font-medium text-slate-700">Tipo de Pesquisa Textual:</span>
                <label className="inline-flex items-center gap-2"><input type="radio" checked={exact} onChange={() => setExact(true)} /> Booleana/Exata</label>
                <label className="inline-flex items-center gap-2"><input type="radio" checked={!exact} onChange={() => setExact(false)} /> Fuzzy/Aproximação</label>
                <label className="ml-auto inline-flex items-center gap-2"><input type="checkbox" checked={liveOnly} onChange={(event) => setLiveOnly(event.target.checked)} /> Pedidos vivos</label>
              </div>
            )}
          </div>

          <div className="grid gap-4 lg:grid-cols-[1fr_180px_180px]">
            <Field label="Marca">
              <input value={brand} onChange={(event) => setBrand(event.target.value)} required maxLength={mode === "basic" ? 40 : 60} className="input" />
            </Field>
            <Field label="Classificação Nice - NCL">
              <input value={niceClass} onChange={(event) => setNiceClass(event.target.value.replace(/\D/g, ""))} maxLength={2} className="input" />
            </Field>
            <Field label="Processos por página">
              <select value={pageSize} onChange={(event) => setPageSize(Number(event.target.value))} className="input">
                {[20, 40, 60, 80, 100].map((value) => <option key={value} value={value}>{value}</option>)}
              </select>
            </Field>
          </div>

          {mode === "advanced" ? (
            <div className="grid gap-4 lg:grid-cols-2">
              <Field label="Apresentação">
                <select value={presentation} onChange={(event) => setPresentation(event.target.value)} className="input">
                  <option value="0">Qualquer Apresentação</option>
                  <option value="1">Nominativa</option>
                  <option value="2">Figurativa</option>
                  <option value="3">Mista</option>
                  <option value="4">Tridimensional</option>
                  <option value="5">Posição</option>
                </select>
              </Field>
              <Field label="Natureza">
                <select value={nature} onChange={(event) => setNature(event.target.value)} className="input">
                  <option value="0">Qualquer natureza</option>
                  <option value="1">Produto</option>
                  <option value="2">Serviço</option>
                  <option value="3">Coletiva</option>
                  <option value="4">Certificação</option>
                </select>
              </Field>
            </div>
          ) : null}

          <div className="flex gap-2">
            <button disabled={searchMutation.isPending} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
              {searchMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <Search size={16} />}
              Pesquisar
            </button>
            <button type="button" onClick={() => { setBrand(""); setNiceClass(""); }} className="h-10 rounded-md border border-line px-4 text-sm text-slate-700 hover:bg-slate-100">Limpar</button>
          </div>

        </form>
      </section>

      {searchMutation.data ? <SourceAlert source={searchMutation.data.source} warning={searchMutation.data.warning} /> : null}
      {searchMutation.isError ? <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">Não foi possível consultar o INPI.</div> : null}
      <Results items={searchMutation.data?.items ?? []} onMonitor={(item) => item.localId ? monitorMutation.mutate(item.localId) : undefined} />
    </div>
  );
}

function Results({ items, onMonitor }: { items: InpiTrademarkResult[]; onMonitor: (item: InpiTrademarkResult) => void }) {
  if (items.length === 0) {
    return <section className="rounded-lg border border-line bg-white p-8 text-center text-sm text-slate-500 shadow-panel">Nenhum resultado para exibir.</section>;
  }

  return (
    <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
      <div className="overflow-x-auto">
        <table className="min-w-full text-left text-sm">
          <thead className="bg-slate-50 text-xs uppercase text-slate-500">
            <tr>
              <th className="px-4 py-3">Número</th>
              <th className="px-4 py-3">Prioridade</th>
              <th className="px-4 py-3">Marca</th>
              <th className="px-4 py-3">Situação</th>
              <th className="px-4 py-3">Titular</th>
              <th className="px-4 py-3">Classe</th>
              <th className="px-4 py-3 text-right">Ação</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-line">
            {items.map((item) => (
              <tr key={item.processNumber}>
                <td className="px-4 py-3 font-medium">
                  <Link href={`/inpi/trademarks/${encodeURIComponent(item.processNumber)}`} className="text-brand hover:text-teal-800">
                    {item.processNumber}
                  </Link>
                </td>
                <td className="px-4 py-3">{formatDate(item.filingDate)}</td>
                <td className="min-w-56 px-4 py-3">{item.name}</td>
                <td className="min-w-44 px-4 py-3">{item.status ?? "Não informado"}</td>
                <td className="min-w-64 px-4 py-3">{item.owners.join(", ") || "Não informado"}</td>
                <td className="px-4 py-3">{item.niceClasses.join(", ") || "Não informado"}</td>
                <td className="px-4 py-3 text-right">
                  <button disabled={!item.localId} onClick={() => onMonitor(item)} className="h-9 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100 disabled:opacity-50">Monitorar</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
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

function SourceAlert({ source, warning }: { source: string; warning: string | null }) {
  const className = source === "OnlineInpi"
    ? "border-emerald-200 bg-emerald-50 text-emerald-700"
    : source === "OnlineFailedLocalFallback"
      ? "border-amber-200 bg-amber-50 text-amber-800"
      : "border-sky-200 bg-sky-50 text-sky-700";
  const message = source === "OnlineInpi"
    ? "Resultado consultado diretamente no INPI e salvo na base local."
    : source === "OnlineFailedLocalFallback"
      ? "INPI indisponível, sem sessão válida ou sem retorno seguro. Resultado veio do fallback local."
      : "Resultado consultado no banco local.";

  return <div className={`rounded-md border px-4 py-3 text-sm ${className}`}>{message} {warning}</div>;
}
