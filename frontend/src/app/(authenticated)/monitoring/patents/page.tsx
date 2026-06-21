"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, RefreshCw, Trash2 } from "lucide-react";
import { formatDate, formatDateTime } from "@/components/status-badge";
import { checkPatentMonitoringNow, listMonitoredPatents, removeMonitoredPatent } from "@/lib/queries";

export default function PatentMonitoringPage() {
  const queryClient = useQueryClient();
  const monitoredQuery = useQuery({ queryKey: ["monitoring", "patents"], queryFn: listMonitoredPatents });
  const checkMutation = useMutation({
    mutationFn: checkPatentMonitoringNow,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["monitoring", "patents"] });
    }
  });
  const removeMutation = useMutation({
    mutationFn: removeMonitoredPatent,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["monitoring", "patents"] })
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-ink">Monitoramento de Patentes</h1>
          <p className="mt-1 text-sm text-slate-500">Patentes acompanhadas e último despacho conhecido.</p>
        </div>
        <button onClick={() => checkMutation.mutate()} disabled={checkMutation.isPending} className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800 disabled:opacity-60">
          {checkMutation.isPending ? <Loader2 className="animate-spin" size={16} /> : <RefreshCw size={16} />}
          Verificar agora
        </button>
      </div>

      {checkMutation.data ? (
        <div className="rounded-md border border-teal-200 bg-teal-50 px-4 py-3 text-sm text-teal-800">
          Verificação concluída: {checkMutation.data.checkedCount} patentes verificadas, {checkMutation.data.changedCount} alterações.
        </div>
      ) : null}

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {monitoredQuery.isLoading ? <State text="Carregando patentes monitoradas..." loading /> : null}
        {monitoredQuery.isError ? <State text="Não foi possível carregar patentes monitoradas." /> : null}
        {monitoredQuery.data?.length === 0 ? <State text="Nenhuma patente monitorada." /> : null}
        {monitoredQuery.data && monitoredQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Processo INPI</th>
                  <th className="px-4 py-3">Patente</th>
                  <th className="px-4 py-3">Último despacho</th>
                  <th className="px-4 py-3">Mudança pendente</th>
                  <th className="px-4 py-3">Última verificação</th>
                  <th className="px-4 py-3 text-right">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {monitoredQuery.data.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium">{item.inpiProcessNumber}</td>
                    <td className="min-w-64 px-4 py-3">{item.title}</td>
                    <td className="px-4 py-3">{item.lastKnownDispatchCode ?? "Não informado"}<div className="text-xs text-slate-400">{formatDate(item.lastKnownDispatchDate)}</div></td>
                    <td className="px-4 py-3">{item.hasPendingChanges ? "Sim" : "Não"}</td>
                    <td className="px-4 py-3">{formatDateTime(item.lastCheckedAtUtc)}</td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => removeMutation.mutate(item.id)} className="inline-flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100">
                        <Trash2 size={15} />
                        Remover
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : null}
      </section>
    </div>
  );
}

function State({ text, loading = false }: { text: string; loading?: boolean }) {
  return <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">{loading ? <Loader2 className="animate-spin" size={18} /> : null}{text}</div>;
}
