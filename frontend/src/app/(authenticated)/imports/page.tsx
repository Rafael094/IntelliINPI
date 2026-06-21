"use client";

import { FormEvent, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Database, FileDown, Loader2 } from "lucide-react";
import { StatusBadge, formatDateTime } from "@/components/status-badge";
import { getImportStatus, importOpenDataTrademarks, importRpiTrademarks } from "@/lib/queries";
import type { ImportResult } from "@/lib/types";

export default function ImportsPage() {
  const queryClient = useQueryClient();
  const [rpiNumber, setRpiNumber] = useState("");
  const [lastResult, setLastResult] = useState<ImportResult | null>(null);

  const statusQuery = useQuery({
    queryKey: ["imports", "status"],
    queryFn: getImportStatus
  });

  const openDataMutation = useMutation({
    mutationFn: importOpenDataTrademarks,
    onSuccess: async (result) => {
      setLastResult(result);
      await queryClient.invalidateQueries({ queryKey: ["imports", "status"] });
    }
  });

  const rpiMutation = useMutation({
    mutationFn: importRpiTrademarks,
    onSuccess: async (result) => {
      setLastResult(result);
      await queryClient.invalidateQueries({ queryKey: ["imports", "status"] });
    }
  });

  function handleRpiSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const parsed = Number(rpiNumber);
    rpiMutation.mutate(Number.isFinite(parsed) && parsed > 0 ? parsed : null);
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Importações</h1>
        <p className="mt-1 text-sm text-slate-500">Execução manual das importações existentes no backend.</p>
      </div>

      <div className="grid gap-4 xl:grid-cols-2">
        <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
          <div className="mb-4 flex items-center gap-3">
            <Database className="text-brand" size={20} />
            <h2 className="text-base font-semibold text-ink">Dados Abertos INPI</h2>
          </div>
          <button
            type="button"
            onClick={() => openDataMutation.mutate()}
            disabled={openDataMutation.isPending}
            className="flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {openDataMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <FileDown size={16} />}
            Importar marcas
          </button>
        </section>

        <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
          <div className="mb-4 flex items-center gap-3">
            <FileDown className="text-brand" size={20} />
            <h2 className="text-base font-semibold text-ink">RPI de Marcas</h2>
          </div>
          <form className="flex flex-wrap gap-3" onSubmit={handleRpiSubmit}>
            <input
              value={rpiNumber}
              onChange={(event) => setRpiNumber(event.target.value)}
              placeholder="RPI, vazio para última"
              className="h-10 min-w-48 rounded-md border border-line px-3 text-sm outline-none focus:border-brand focus:ring-2 focus:ring-teal-100"
            />
            <button
              type="submit"
              disabled={rpiMutation.isPending}
              className="flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-70"
            >
              {rpiMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <FileDown size={16} />}
              Importar RPI
            </button>
          </form>
        </section>
      </div>

      {lastResult ? <ImportResultPanel title="Último resultado" result={lastResult} /> : null}

      <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
        <h2 className="text-base font-semibold text-ink">Status mais recente</h2>
        {statusQuery.isLoading ? <p className="mt-4 text-sm text-slate-500">Carregando status...</p> : null}
        {statusQuery.isError ? <p className="mt-4 text-sm text-red-600">Erro ao carregar status.</p> : null}
        {statusQuery.isSuccess && !statusQuery.data ? <p className="mt-4 text-sm text-slate-500">Nenhum job encontrado.</p> : null}
        {statusQuery.data ? (
          <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
            <Info label="JobId" value={statusQuery.data.lastJobId ?? "Não informado"} />
            <Info label="Status" value={<StatusBadge status={statusQuery.data.status} />} />
            <Info label="Linhas importadas" value="Não informado pelo status" />
            <Info label="Linhas com falha" value="Não informado pelo status" />
            <Info label="Origem" value={statusQuery.data.source ?? "Não informado"} />
            <Info label="Início" value={formatDateTime(statusQuery.data.startedAtUtc)} />
            <Info label="Fim" value={formatDateTime(statusQuery.data.finishedAtUtc)} />
          </div>
        ) : null}
      </section>
    </div>
  );
}

function ImportResultPanel({ title, result }: { title: string; result: ImportResult }) {
  return (
    <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
      <h2 className="text-base font-semibold text-ink">{title}</h2>
      <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        <Info label="JobId" value={result.jobId} />
        <Info label="Status" value={<StatusBadge status={result.status} />} />
        <Info label="Linhas importadas" value={result.importedRows.toLocaleString("pt-BR")} />
        <Info label="Linhas com falha" value={result.failedRows.toLocaleString("pt-BR")} />
      </div>
      {result.errorMessage ? <p className="mt-3 text-sm text-red-600">{result.errorMessage}</p> : null}
    </section>
  );
}

function Info({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="rounded-md border border-line bg-slate-50 p-3">
      <p className="text-xs uppercase text-slate-500">{label}</p>
      <div className="mt-1 break-words text-sm font-medium text-ink">{value}</div>
    </div>
  );
}
