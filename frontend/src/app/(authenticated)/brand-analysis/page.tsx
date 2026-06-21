"use client";

import { FormEvent, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { AlertCircle, ExternalLink, Loader2, Radar, Search, ShieldAlert, Tags } from "lucide-react";
import { analyzeTrademarkAvailability } from "@/lib/queries";
import type { TrademarkAvailabilityAnalysis } from "@/lib/types";

export default function BrandAnalysisPage() {
  const [proposedName, setProposedName] = useState("Dogão Filmes Ltda");
  const [activityDescription, setActivityDescription] = useState("produção de filmes, vídeos e conteúdo audiovisual");

  const analysisMutation = useMutation({
    mutationFn: analyzeTrademarkAvailability
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    analysisMutation.mutate({
      proposedName: proposedName.trim(),
      activityDescription: activityDescription.trim() || undefined
    });
  }

  const analysis = analysisMutation.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Análise de Marca</h1>
        <p className="mt-1 text-sm text-slate-500">
          Triagem para nova marca usando primeiro o INPI online, fallback local e verificações externas controladas.
        </p>
      </div>

      <section className="rounded-lg border border-line bg-white p-4 shadow-panel">
        <form className="grid gap-3 lg:grid-cols-[1.1fr_1.6fr_auto]" onSubmit={handleSubmit}>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">Nome pretendido</span>
            <input
              value={proposedName}
              onChange={(event) => setProposedName(event.target.value)}
            placeholder="Ex: Dogão Filmes Ltda"
              className="h-10 w-full rounded-md border border-line px-3 text-sm outline-none focus:border-brand focus:ring-2 focus:ring-teal-100"
            />
          </label>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">Atividade, produtos ou serviços</span>
            <input
              value={activityDescription}
              onChange={(event) => setActivityDescription(event.target.value)}
              placeholder="Ex: produção audiovisual, streaming, publicidade..."
              className="h-10 w-full rounded-md border border-line px-3 text-sm outline-none focus:border-brand focus:ring-2 focus:ring-teal-100"
            />
          </label>
          <div className="flex items-end">
            <button
              type="submit"
              disabled={analysisMutation.isPending || proposedName.trim().length === 0}
              className="flex h-10 w-full items-center justify-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {analysisMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <Search size={16} />}
              Analisar
            </button>
          </div>
        </form>
      </section>

      {analysisMutation.isError ? (
        <div className="flex items-center gap-2 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          <AlertCircle size={18} />
          Não foi possível analisar a marca agora.
        </div>
      ) : null}

      {!analysis && !analysisMutation.isPending ? <InitialState /> : null}
      {analysis ? <AnalysisResult analysis={analysis} /> : null}
    </div>
  );
}

function InitialState() {
  return (
    <section className="flex min-h-72 flex-col items-center justify-center rounded-lg border border-line bg-white px-4 text-center shadow-panel">
      <Radar className="text-slate-400" size={32} />
      <p className="mt-3 text-sm font-medium text-ink">Informe a marca e a atividade</p>
      <p className="mt-1 max-w-xl text-sm text-slate-500">
        A análise consulta o INPI online primeiro, cruza nomes parecidos, sugere classes Nice prováveis e monta uma trilha de verificação web.
      </p>
    </section>
  );
}

function AnalysisResult({ analysis }: { analysis: TrademarkAvailabilityAnalysis }) {
  return (
    <div className="space-y-6">
      <section className="grid gap-4 lg:grid-cols-3">
        <Metric title="Marca normalizada" value={analysis.normalizedBrand} />
        <Metric title="Risco preliminar" value={formatRisk(analysis.riskLevel)} tone={riskTone(analysis.riskLevel)} />
        <Metric title="Conflitos INPI" value={analysis.localConflicts.length.toString()} />
      </section>

      <section className="rounded-lg border border-line bg-white p-4 shadow-panel">
        <div className="flex items-start gap-3">
          <ShieldAlert className="mt-0.5 text-brand" size={20} />
          <div>
            <p className="text-sm font-semibold text-ink">Resumo</p>
            <p className="mt-1 text-sm text-slate-600">{analysis.summary}</p>
            <p className="mt-2 text-xs text-slate-500">
              Fonte da busca de conflitos: {formatConflictSource(analysis.conflictSearchSource)}
              {analysis.conflictSearchWarning ? ` - ${analysis.conflictSearchWarning}` : ""}
            </p>
          </div>
        </div>
      </section>

      <section className="rounded-lg border border-line bg-white shadow-panel">
        <div className="flex items-center gap-2 border-b border-line px-4 py-3">
          <Tags size={18} className="text-brand" />
          <p className="text-sm font-semibold text-ink">Classes Nice sugeridas</p>
        </div>
        <div className="divide-y divide-line">
          {analysis.suggestedClasses.map((item) => (
            <div key={item.code} className="grid gap-2 px-4 py-3 md:grid-cols-[80px_1fr]">
              <div className="flex h-9 w-14 items-center justify-center rounded-md bg-teal-50 text-sm font-semibold text-brand">
                {item.code}
              </div>
              <div>
                <p className="text-sm font-medium text-ink">{item.title}</p>
                <p className="mt-1 text-sm text-slate-600">{item.reason}</p>
                {item.matchedKeywords.length > 0 ? (
                  <p className="mt-1 text-xs text-slate-500">Termos encontrados: {item.matchedKeywords.join(", ")}</p>
                ) : null}
              </div>
            </div>
          ))}
        </div>
      </section>

      <section className="rounded-lg border border-line bg-white shadow-panel">
        <div className="border-b border-line px-4 py-3">
          <p className="text-sm font-semibold text-ink">Possíveis conflitos no INPI</p>
          <p className="mt-1 text-xs text-slate-500">O sistema tenta consultar o INPI online primeiro. Se o INPI falhar, usa o banco local como fallback.</p>
        </div>
        {analysis.localConflicts.length === 0 ? (
          <div className="px-4 py-8 text-center text-sm text-slate-500">Nenhum conflito relevante encontrado.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full border-collapse text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3 font-semibold">Score</th>
                  <th className="px-4 py-3 font-semibold">Processo</th>
                  <th className="px-4 py-3 font-semibold">Marca</th>
                  <th className="px-4 py-3 font-semibold">Classe</th>
                  <th className="px-4 py-3 font-semibold">Status</th>
                  <th className="px-4 py-3 font-semibold">Motivo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {analysis.localConflicts.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-50">
                    <td className="whitespace-nowrap px-4 py-3 font-semibold text-ink">{item.similarityScore}%</td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-700">{item.processNumber}</td>
                    <td className="min-w-56 px-4 py-3 text-slate-800">{item.name || "Sem nome"}</td>
                    <td className="px-4 py-3 text-slate-600">{formatList(item.niceClasses)}</td>
                    <td className="min-w-40 px-4 py-3 text-slate-600">{item.status ?? "Não informado"}</td>
                    <td className="min-w-64 px-4 py-3 text-slate-600">{item.conflictReason}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="rounded-lg border border-line bg-white shadow-panel">
        <div className="border-b border-line px-4 py-3">
          <p className="text-sm font-semibold text-ink">Resultados externos encontrados</p>
          <p className="mt-1 text-xs text-slate-500">Busca via Exa com foco Brasil: web, Instagram, Facebook e domínios .br/.com.br.</p>
        </div>
        {analysis.externalResults.length === 0 ? (
          <div className="px-4 py-8 text-center text-sm text-slate-500">
            Nenhum resultado externo retornado ou provedor externo não configurado.
          </div>
        ) : (
          <div className="divide-y divide-line">
            {analysis.externalResults.map((item) => (
              <a
                key={`${item.category}-${item.url}`}
                href={item.url}
                target="_blank"
                rel="noreferrer"
                className="grid gap-2 px-4 py-3 hover:bg-slate-50 md:grid-cols-[120px_1fr_auto]"
              >
                <div>
                  <p className="text-sm font-semibold text-brand">{item.category}</p>
                  <p className="text-xs text-slate-500">{item.source}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-ink">{item.title}</p>
                  <p className="mt-1 line-clamp-2 text-sm text-slate-600">{item.snippet ?? item.query}</p>
                  <p className="mt-1 break-all text-xs text-slate-400">{item.url}</p>
                </div>
                <span className="flex items-center gap-2 text-sm text-brand">
                  Abrir <ExternalLink size={15} />
                </span>
              </a>
            ))}
          </div>
        )}
      </section>

      <section className="rounded-lg border border-line bg-white shadow-panel">
        <div className="border-b border-line px-4 py-3">
          <p className="text-sm font-semibold text-ink">Presença web e uso indevido</p>
          <p className="mt-1 text-xs text-slate-500">Links de verificação controlada, sem scraping automatizado de Google, Instagram ou Facebook.</p>
        </div>
        <div className="divide-y divide-line">
          {analysis.webPresenceChecks.map((item) => (
            <a
              key={`${item.source}-${item.url}`}
              href={item.url}
              target="_blank"
              rel="noreferrer"
              className="grid gap-2 px-4 py-3 hover:bg-slate-50 md:grid-cols-[180px_1fr_auto]"
            >
              <div>
                <p className="text-sm font-medium text-ink">{item.source}</p>
                <p className="text-xs text-slate-500">{item.status}</p>
              </div>
              <p className="text-sm text-slate-600">{item.notes}</p>
              <span className="flex items-center gap-2 text-sm text-brand">
                Abrir <ExternalLink size={15} />
              </span>
            </a>
          ))}
        </div>
      </section>
    </div>
  );
}

function Metric({ title, value, tone = "default" }: { title: string; value: string; tone?: "default" | "warning" | "danger" }) {
  const toneClass = tone === "danger" ? "text-red-700" : tone === "warning" ? "text-amber-700" : "text-ink";

  return (
    <div className="rounded-lg border border-line bg-white p-4 shadow-panel">
      <p className="text-xs uppercase text-slate-500">{title}</p>
      <p className={`mt-2 text-xl font-semibold ${toneClass}`}>{value}</p>
    </div>
  );
}

function formatRisk(value: string) {
  const labels: Record<string, string> = {
    High: "Alto",
    Medium: "Médio",
    Low: "Baixo",
    NoLocalConflictFound: "Sem conflito local forte"
  };

  return labels[value] ?? value;
}

function formatConflictSource(value: string) {
  const labels: Record<string, string> = {
    OnlineInpi: "INPI online",
    LocalDatabaseFallback: "Banco local por fallback",
    LocalDatabase: "Banco local"
  };

  return labels[value] ?? value;
}

function riskTone(value: string): "default" | "warning" | "danger" {
  if (value === "High") {
    return "danger";
  }

  if (value === "Medium") {
    return "warning";
  }

  return "default";
}

function formatList(values: string[]) {
  return values.length > 0 ? values.join(", ") : "Não informado";
}
