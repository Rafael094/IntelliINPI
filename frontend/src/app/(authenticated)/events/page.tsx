"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Loader2 } from "lucide-react";
import { formatDate, formatDateTime } from "@/components/status-badge";
import { listMonitoringEvents, markMonitoringEventAsRead } from "@/lib/queries";

export default function EventsPage() {
  const [unreadOnly, setUnreadOnly] = useState(false);
  const queryClient = useQueryClient();

  const eventsQuery = useQuery({
    queryKey: ["events", unreadOnly],
    queryFn: () => listMonitoringEvents(unreadOnly)
  });

  const markReadMutation = useMutation({
    mutationFn: markMonitoringEventAsRead,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["events"] });
      await queryClient.invalidateQueries({ queryKey: ["monitoring"] });
    }
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-ink">Eventos</h1>
          <p className="mt-1 text-sm text-slate-500">Alterações detectadas nos despachos das marcas monitoradas.</p>
        </div>
        <label className="flex items-center gap-2 text-sm text-slate-700">
          <input
            type="checkbox"
            checked={unreadOnly}
            onChange={(event) => setUnreadOnly(event.target.checked)}
            className="h-4 w-4 rounded border-line text-brand"
          />
          Apenas não lidos
        </label>
      </div>

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {eventsQuery.isLoading ? <TableMessage text="Carregando eventos..." loading /> : null}
        {eventsQuery.isError ? <TableMessage text="Erro ao carregar eventos." /> : null}
        {eventsQuery.isSuccess && eventsQuery.data.length === 0 ? <TableMessage text="Nenhum evento encontrado." /> : null}

        {eventsQuery.isSuccess && eventsQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full border-collapse text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3 font-semibold">Processo</th>
                  <th className="px-4 py-3 font-semibold">Marca</th>
                  <th className="px-4 py-3 font-semibold">Despacho anterior</th>
                  <th className="px-4 py-3 font-semibold">Novo despacho</th>
                  <th className="px-4 py-3 font-semibold">Data</th>
                  <th className="px-4 py-3 font-semibold">Lido</th>
                  <th className="px-4 py-3 text-right font-semibold">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {eventsQuery.data.map((event) => (
                  <tr key={event.id} className="hover:bg-slate-50">
                    <td className="whitespace-nowrap px-4 py-3 font-medium text-ink">{event.processNumber}</td>
                    <td className="min-w-56 px-4 py-3 text-slate-700">{event.trademarkName || "Sem nome"}</td>
                    <td className="px-4 py-3 text-slate-600">
                      <div>{event.previousDispatchCode ?? "Não informado"}</div>
                      <div className="text-xs text-slate-400">{formatDate(event.previousDispatchDate)}</div>
                    </td>
                    <td className="px-4 py-3 text-slate-600">
                      <div>{event.currentDispatchCode ?? "Não informado"}</div>
                      <div className="text-xs text-slate-400">{formatDate(event.currentDispatchDate)}</div>
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">{formatDateTime(event.createdAtUtc)}</td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex rounded-full border px-2 py-1 text-xs font-medium ${
                          event.isRead
                            ? "border-emerald-200 bg-emerald-50 text-emerald-700"
                            : "border-amber-200 bg-amber-50 text-amber-700"
                        }`}
                      >
                        {event.isRead ? "Sim" : "Não"}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        type="button"
                        onClick={() => markReadMutation.mutate(event.id)}
                        disabled={event.isRead || markReadMutation.isPending}
                        className="inline-flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        <Check size={16} />
                        Marcar lido
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

function TableMessage({ text, loading }: { text: string; loading?: boolean }) {
  return (
    <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">
      {loading ? <Loader2 className="animate-spin" size={18} /> : null}
      {text}
    </div>
  );
}
