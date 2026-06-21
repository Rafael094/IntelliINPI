"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Loader2, Search, ShieldCheck } from "lucide-react";
import { formatDate } from "@/components/status-badge";
import { getApiErrorMessage } from "@/lib/api-error";
import { registerAndMonitorIPAsset } from "@/lib/queries";
import type { RegisterAndMonitorResult } from "@/lib/types";

export default function NewIPAssetPage() {
  const queryClient = useQueryClient();
  const [type, setType] = useState("Trademark");
  const [query, setQuery] = useState("");
  const mutation = useMutation({
    mutationFn: registerAndMonitorIPAsset,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["ip-assets"] });
      await queryClient.invalidateQueries({ queryKey: ["monitoring"] });
    }
  });

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    mutation.mutate({ type, query: query.trim() });
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Link href="/ip-assets" className="inline-flex h-9 w-9 items-center justify-center rounded-md border border-line bg-white text-slate-600 hover:bg-slate-50">
          <ArrowLeft size={17} />
        </Link>
        <div>
          <h1 className="text-2xl font-semibold text-ink">Cadastrar ativo de PI</h1>
          <p className="mt-1 text-sm text-slate-500">Busque por nome ou processo. O sistema salva e ativa o monitoramento quando encontra um resultado claro.</p>
        </div>
      </div>

      <form className="rounded-lg border border-line bg-white p-4 shadow-panel" onSubmit={submit}>
        <div className="grid gap-4 lg:grid-cols-[220px_1fr_auto]">
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">Tipo</span>
            <select value={type} onChange={(event) => setType(event.target.value)} className="input">
              <option value="Trademark">Marca</option>
              <option value="Patent">Patente</option>
            </select>
          </label>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">Nome ou processo</span>
            <input value={query} onChange={(event) => setQuery(event.target.value)} required className="input" placeholder="Ex: 935957162 ou nome da marca/patente" />
          </label>
          <div className="flex items-end">
            <button disabled={mutation.isPending} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
              {mutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <Search size={16} />}
              Buscar e monitorar
            </button>
          </div>
        </div>
      </form>

      {mutation.error ? (
        <Alert tone="error">{getApiErrorMessage(mutation.error, "Não foi possível cadastrar o ativo.")}</Alert>
      ) : null}
      {mutation.data ? <ResultPanel result={mutation.data} /> : null}
    </div>
  );
}

function ResultPanel({ result }: { result: RegisterAndMonitorResult }) {
  const isFallback = result.source === "OnlineFailedLocalFallback";
  const isLocal = result.source === "LocalDatabase";
  const isManual = result.status.includes("ManualDraft");
  const isMultiple = result.status === "MultipleResults";

  return (
    <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
      <div className="border-b border-line px-4 py-3">
        <div className="flex flex-wrap items-center gap-2">
          <h2 className="text-base font-semibold text-ink">Resultado</h2>
          <SourceBadge source={result.source} />
        </div>
        {result.warning ? <p className="mt-2 text-sm text-amber-700">{result.warning}</p> : null}
      </div>
      {isFallback ? <Alert tone="warning">INPI online indisponível ou não configurado. Resultado veio do fallback local controlado.</Alert> : null}
      {isLocal ? <Alert tone="info">Resultado consultado no banco local.</Alert> : null}
      {isManual ? <Alert tone="warning">Cadastro salvo como Draft/manual. Revisão manual necessária.</Alert> : null}
      {isMultiple ? <Alert tone="warning">Foram encontrados múltiplos resultados. Escolha manualmente antes de confirmar o cadastro definitivo.</Alert> : null}

      {result.ipAssetId ? (
        <div className="px-4 py-4 text-sm text-slate-700">
          <p className="font-medium text-ink">Ativo criado: {result.ipAssetId}</p>
          <p className="mt-1">Monitoramento: {result.isMonitored ? "ativado" : "não ativado"}</p>
        </div>
      ) : null}

      {result.candidates.length > 0 ? (
        <div className="overflow-x-auto">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">Processo</th>
                <th className="px-4 py-3">Título</th>
                <th className="px-4 py-3">Titular</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Data base</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-line">
              {result.candidates.map((candidate) => (
                <tr key={`${candidate.type}-${candidate.inpiProcessNumber ?? candidate.title}`}>
                  <td className="px-4 py-3 font-medium">{candidate.inpiProcessNumber ?? "Manual"}</td>
                  <td className="px-4 py-3">{candidate.title}</td>
                  <td className="px-4 py-3">{candidate.ownerName ?? "Não informado"}</td>
                  <td className="px-4 py-3">{candidate.status ?? "Não informado"}</td>
                  <td className="px-4 py-3">{formatDate(candidate.filingDate)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </section>
  );
}

function SourceBadge({ source }: { source: string | null }) {
  const className = source === "OnlineInpi"
    ? "border-emerald-200 bg-emerald-50 text-emerald-700"
    : source === "OnlineFailedLocalFallback"
      ? "border-amber-200 bg-amber-50 text-amber-700"
      : "border-slate-200 bg-slate-50 text-slate-700";

  const label = source === "OnlineInpi" ? "INPI online" : source === "OnlineFailedLocalFallback" ? "Fallback local" : "Banco local";
  return <span className={`inline-flex items-center gap-1 rounded-full border px-2 py-1 text-xs font-medium ${className}`}><ShieldCheck size={13} />{label}</span>;
}

function Alert({ children, tone }: { children: React.ReactNode; tone: "warning" | "error" | "info" }) {
  const className = tone === "error"
    ? "border-red-200 bg-red-50 text-red-700"
    : tone === "info"
      ? "border-sky-200 bg-sky-50 text-sky-700"
      : "border-amber-200 bg-amber-50 text-amber-800";

  return <div className={`m-4 rounded-md border px-4 py-3 text-sm ${className}`}>{children}</div>;
}
