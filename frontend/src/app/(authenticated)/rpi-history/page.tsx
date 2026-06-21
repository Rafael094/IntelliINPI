"use client";

import { FormEvent, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, PauseCircle, Play, RotateCcw } from "lucide-react";
import { StatusBadge, formatDateTime } from "@/components/status-badge";
import { getRpiHistoryStatus, resumeRpiHistory, runRpiHistory, stopRpiHistory } from "@/lib/queries";
import type { RpiHistoryRunRequest, RpiHistoryStatus } from "@/lib/types";

export default function RpiHistoryPage() {
  const queryClient = useQueryClient();
  const [startYear, setStartYear] = useState("2010");
  const [startRpi, setStartRpi] = useState("");
  const [endRpi, setEndRpi] = useState("0");
  const [batchSize, setBatchSize] = useState("25");
  const [delaySeconds, setDelaySeconds] = useState("5");

  const statusQuery = useQuery({
    queryKey: ["rpi-history", "status"],
    queryFn: getRpiHistoryStatus,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status === "Running" || status === "StopRequested" ? 5000 : false;
    }
  });

  const runMutation = useMutation({
    mutationFn: runRpiHistory,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["rpi-history", "status"] })
  });

  const resumeMutation = useMutation({
    mutationFn: resumeRpiHistory,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["rpi-history", "status"] })
  });

  const stopMutation = useMutation({
    mutationFn: stopRpiHistory,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["rpi-history", "status"] })
  });

  const isBusy = runMutation.isPending || resumeMutation.isPending || stopMutation.isPending;

  function handleRun(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const request: RpiHistoryRunRequest = {
      endRpi: Number(endRpi) || 0,
      batchSize: Number(batchSize) || 25,
      delaySecondsBetweenBatches: Number(delaySeconds) || 0
    };

    if (startRpi.trim()) {
      request.startRpi = Number(startRpi);
    } else {
      request.startYear = Number(startYear) || 2010;
    }

    runMutation.mutate(request);
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Histórico RPI</h1>
        <p className="mt-1 text-sm text-slate-500">Importação histórica sequencial com checkpoints por RPI.</p>
      </div>

      <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
        <form className="grid gap-3 md:grid-cols-2 xl:grid-cols-5" onSubmit={handleRun}>
          <NumberInput label="Ano inicial" value={startYear} onChange={setStartYear} disabled={Boolean(startRpi.trim())} />
          <NumberInput label="RPI inicial" value={startRpi} onChange={setStartRpi} placeholder="Opcional" />
          <NumberInput label="RPI final" value={endRpi} onChange={setEndRpi} />
          <NumberInput label="Lote" value={batchSize} onChange={setBatchSize} />
          <NumberInput label="Pausa entre lotes" value={delaySeconds} onChange={setDelaySeconds} />
          <div className="flex flex-wrap gap-2 md:col-span-2 xl:col-span-5">
            <ActionButton label="Executar" icon="play" pending={runMutation.isPending} disabled={isBusy} />
            <button
              type="button"
              onClick={() => resumeMutation.mutate()}
              disabled={isBusy}
              className="flex h-10 items-center gap-2 rounded-md border border-line px-4 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {resumeMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <RotateCcw size={16} />}
              Retomar
            </button>
            <button
              type="button"
              onClick={() => stopMutation.mutate()}
              disabled={isBusy}
              className="flex h-10 items-center gap-2 rounded-md border border-line px-4 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {stopMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <PauseCircle size={16} />}
              Parar
            </button>
          </div>
        </form>
      </section>

      <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
        <div className="mb-4 flex items-center justify-between gap-3">
          <h2 className="text-base font-semibold text-ink">Status da execução</h2>
          {statusQuery.isFetching ? <Loader2 className="animate-spin text-slate-400" size={18} /> : null}
        </div>
        {statusQuery.isLoading ? <p className="text-sm text-slate-500">Carregando status...</p> : null}
        {statusQuery.isError ? <p className="text-sm text-red-600">Erro ao carregar histórico.</p> : null}
        {statusQuery.isSuccess && !statusQuery.data ? <p className="text-sm text-slate-500">Nenhuma execução encontrada.</p> : null}
        {statusQuery.data ? <HistoryStatus status={statusQuery.data} /> : null}
      </section>
    </div>
  );
}

function HistoryStatus({ status }: { status: RpiHistoryStatus }) {
  return (
    <div className="space-y-4">
      <div>
        <div className="mb-2 flex items-center justify-between text-sm">
          <span className="font-medium text-ink">{status.percentage.toFixed(2)}%</span>
          <StatusBadge status={status.status} />
        </div>
        <div className="h-3 overflow-hidden rounded-full bg-slate-100">
          <div className="h-full bg-brand transition-all" style={{ width: `${Math.min(100, Math.max(0, status.percentage))}%` }} />
        </div>
      </div>

      <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        <Info label="RunId" value={status.runId} />
        <Info label="RPI inicial" value={status.startRpi} />
        <Info label="RPI final" value={status.endRpi} />
        <Info label="RPI atual" value={status.currentRpi} />
        <Info label="Total RPIs" value={status.totalRpis} />
        <Info label="Sucesso" value={status.successfulRpis} />
        <Info label="Falhas" value={status.failedRpis} />
        <Info label="Despachos importados" value={status.totalDispatchesImported.toLocaleString("pt-BR")} />
        <Info label="Início" value={formatDateTime(status.startedAtUtc)} />
        <Info label="Fim" value={formatDateTime(status.finishedAtUtc)} />
        <Info label="Erro" value={status.errorMessage ?? "Nenhum"} />
      </div>
    </div>
  );
}

function NumberInput({
  label,
  value,
  onChange,
  placeholder,
  disabled
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
}) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-medium text-slate-700">{label}</span>
      <input
        type="number"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        disabled={disabled}
        className="h-10 w-full rounded-md border border-line px-3 text-sm outline-none focus:border-brand focus:ring-2 focus:ring-teal-100 disabled:bg-slate-100"
      />
    </label>
  );
}

function ActionButton({ label, pending, disabled }: { label: string; icon: "play"; pending: boolean; disabled: boolean }) {
  return (
    <button
      type="submit"
      disabled={disabled}
      className="flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-70"
    >
      {pending ? <Loader2 className="animate-spin" size={16} /> : <Play size={16} />}
      {label}
    </button>
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
