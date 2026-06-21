"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { Loader2, RefreshCw, Trash2 } from "lucide-react";
import { checkMonitoringNow, listMonitoredTrademarks, removeMonitoredTrademark } from "@/lib/queries";
import { formatDate, formatDateTime } from "@/components/status-badge";

export default function MonitoringPage() {
  const queryClient = useQueryClient();
  const monitoredQuery = useQuery({
    queryKey: ["monitoring", "trademarks"],
    queryFn: listMonitoredTrademarks
  });

  const checkMutation = useMutation({
    mutationFn: checkMonitoringNow,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["monitoring"] });
      await queryClient.invalidateQueries({ queryKey: ["events"] });
    }
  });

  const removeMutation = useMutation({
    mutationFn: removeMonitoredTrademark,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["monitoring", "trademarks"] })
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-ink">Monitoramento</h1>
          <p className="mt-1 text-sm text-slate-500">Marcas acompanhadas e último despacho conhecido.</p>
        </div>
        <button
          type="button"
          onClick={() => checkMutation.mutate()}
          disabled={checkMutation.isPending}
          className="flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {checkMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <RefreshCw size={16} />}
          Verificar agora
        </button>
      </div>

      {checkMutation.data ? (
        <div className="rounded-md border border-teal-200 bg-teal-50 px-4 py-3 text-sm text-teal-800">
          Verificação concluída: {checkMutation.data.checkedCount} marcas verificadas, {checkMutation.data.changedCount} alterações.
        </div>
      ) : null}

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {monitoredQuery.isLoading ? <TableMessage text="Carregando monitoramento..." loading /> : null}
        {monitoredQuery.isError ? <TableMessage text="Erro ao carregar marcas monitoradas." /> : null}
        {monitoredQuery.isSuccess && monitoredQuery.data.length === 0 ? <TableMessage text="Nenhuma marca monitorada." /> : null}

        {monitoredQuery.isSuccess && monitoredQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full border-collapse text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3 font-semibold">Processo</th>
                  <th className="px-4 py-3 font-semibold">Marca</th>
                  <th className="px-4 py-3 font-semibold">Último despacho</th>
                  <th className="px-4 py-3 font-semibold">Mudança pendente</th>
                  <th className="px-4 py-3 font-semibold">Última verificação</th>
                  <th className="px-4 py-3 text-right font-semibold">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {monitoredQuery.data.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-50">
                    <td className="whitespace-nowrap px-4 py-3 font-medium text-ink">
                      <Link href={`/inpi/trademarks/${encodeURIComponent(item.processNumber)}`} className="text-brand hover:text-teal-800">
                        {item.processNumber}
                      </Link>
                    </td>
                    <td className="min-w-56 px-4 py-3 text-slate-700">{item.name || "Sem nome"}</td>
                    <td className="px-4 py-3 text-slate-600">
                      <div>{item.lastKnownDispatchCode ?? "Não informado"}</div>
                      <div className="text-xs text-slate-400">{formatDate(item.lastKnownDispatchDate)}</div>
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex rounded-full border px-2 py-1 text-xs font-medium ${
                          item.hasPendingChanges
                            ? "border-amber-200 bg-amber-50 text-amber-700"
                            : "border-emerald-200 bg-emerald-50 text-emerald-700"
                        }`}
                      >
                        {item.hasPendingChanges ? "Sim" : "Não"}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">{formatDateTime(item.lastCheckedAtUtc)}</td>
                    <td className="px-4 py-3 text-right">
                      <button
                        type="button"
                        onClick={() => removeMutation.mutate(item.id)}
                        disabled={removeMutation.isPending}
                        className="inline-flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-60"
                      >
                        <Trash2 size={16} />
                        Remover
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="border-t border-line bg-slate-50 px-4 py-4">
              <h2 className="text-sm font-semibold text-ink">Linha do tempo dos despachos monitorados</h2>
              <div className="mt-3 grid gap-3 lg:grid-cols-2">
                {monitoredQuery.data.map((item) => (
                  <div key={`${item.id}-timeline`} className="rounded-md border border-line bg-white p-3">
                    <div className="flex items-center justify-between gap-3">
                      <Link href={`/inpi/trademarks/${encodeURIComponent(item.processNumber)}`} className="font-medium text-brand hover:text-teal-800">
                        {item.processNumber}
                      </Link>
                      <span className="text-xs text-slate-500">{item.name || "Sem nome"}</span>
                    </div>
                    {item.recentDispatches.length === 0 ? (
                      <div className="mt-3 text-sm text-slate-500">Nenhum despacho importado para esta marca.</div>
                    ) : (
                      <ol className="mt-3 space-y-3">
                        {item.recentDispatches.map((dispatch) => (
                          <li key={dispatch.id} className="border-l-2 border-teal-200 pl-3">
                            <div className="flex flex-wrap items-center gap-2 text-xs text-slate-500">
                              <span>{formatDate(dispatch.publishedAt)}</span>
                              <span>RPI {dispatch.rpiNumber ?? "Nao informada"}</span>
                              <span className="rounded-full bg-slate-100 px-2 py-0.5 font-medium text-slate-700">{dispatch.code}</span>
                            </div>
                            <div className="mt-1 text-sm text-slate-700">{dispatch.description}</div>
                          </li>
                        ))}
                      </ol>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>
        ) : null}
      </section>
    </div>
  );
}

function TableMessage({ text, loading }: { text: string; loading?: boolean }) {
  return (
    <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">
      {loading ? <Loader2 className="animate-spin" size={18} /> : null}
      {text}
    </div>
  );
}
