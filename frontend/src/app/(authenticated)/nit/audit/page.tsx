"use client";

import { useQuery } from "@tanstack/react-query";
import { Loader2 } from "lucide-react";
import { listNitAuditLogs } from "@/lib/queries";
import { useState } from "react";

export default function NitAuditPage() {
  const [entityName, setEntityName] = useState("");
  const [action, setAction] = useState("");
  const auditQuery = useQuery({
    queryKey: ["nit-audit-logs", entityName, action],
    queryFn: () => listNitAuditLogs({ entityName: entityName || undefined, action: action || undefined }),
    refetchInterval: 30000
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Auditoria</h1>
        <p className="mt-1 text-sm text-slate-500">Eventos auditáveis do módulo NIT / INOVA+.</p>
      </div>

      <div className="flex flex-wrap gap-3 rounded-lg border border-line bg-white p-4 shadow-panel">
        <label className="text-sm">Entidade<select className="input mt-1 min-w-52" value={entityName} onChange={(e) => setEntityName(e.target.value)}><option value="">Todas</option>{["Institution", "Researcher", "Invention", "Company", "TechnologyTransferContract", "RoyaltyPayment", "TechnologyTransferOpportunity", "NitDocument"].map(x => <option key={x}>{x}</option>)}</select></label>
        <label className="text-sm">Ação<select className="input mt-1 min-w-44" value={action} onChange={(e) => setAction(e.target.value)}><option value="">Todas</option>{["Created", "Updated", "Deleted", "Uploaded", "StageChanged"].map(x => <option key={x}>{x}</option>)}</select></label>
      </div>

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {auditQuery.isLoading ? <State text="Carregando auditoria..." loading /> : null}
        {auditQuery.isError ? <State text="Não foi possível carregar auditoria." /> : null}
        {auditQuery.data?.length === 0 ? <State text="Nenhum evento de auditoria encontrado." /> : null}
        {auditQuery.data && auditQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Data</th>
                  <th className="px-4 py-3">Usuário</th>
                  <th className="px-4 py-3">Instituição</th>
                  <th className="px-4 py-3">Entidade</th>
                  <th className="px-4 py-3">Ação</th>
                  <th className="px-4 py-3">IP</th>
                  <th className="px-4 py-3">Alteração</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {auditQuery.data.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-50">
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">{formatDateTime(item.createdAtUtc)}</td>
                    <td className="px-4 py-3 text-slate-700">{item.userEmail}</td>
                    <td className="px-4 py-3 text-slate-600">{item.universityName ?? "Global"}</td>
                    <td className="px-4 py-3 text-slate-600">{item.entityName}</td>
                    <td className="px-4 py-3 font-medium text-ink">{item.action}</td>
                    <td className="px-4 py-3 text-slate-600">{item.ipAddress ?? "Não informado"}</td>
                    <td className="max-w-72 px-4 py-3 text-xs text-slate-500"><details><summary className="cursor-pointer">Ver valores</summary><pre className="mt-2 max-h-40 overflow-auto whitespace-pre-wrap">Anterior: {item.previousValue ?? "-"}{"\n"}Novo: {item.newValue ?? "-"}</pre></details></td>
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

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short"
  }).format(new Date(value));
}
