"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { Loader2, Plus, ShieldCheck } from "lucide-react";
import { formatDate } from "@/components/status-badge";
import { listIPAssets } from "@/lib/queries";

export default function IPAssetsPage() {
  const assetsQuery = useQuery({ queryKey: ["ip-assets"], queryFn: () => listIPAssets() });

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-ink">Portfolio de PI</h1>
          <p className="mt-1 text-sm text-slate-500">Ativos de propriedade intelectual cadastrados e monitorados.</p>
        </div>
        <Link href="/ip-assets/new" className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800">
          <Plus size={16} />
          Novo ativo
        </Link>
      </div>

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        {assetsQuery.isLoading ? <State text="Carregando portfolio..." loading /> : null}
        {assetsQuery.isError ? <State text="Não foi possível carregar o portfolio." /> : null}
        {assetsQuery.data?.length === 0 ? <State text="Nenhum ativo cadastrado." /> : null}
        {assetsQuery.data && assetsQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Tipo</th>
                  <th className="px-4 py-3">Processo INPI</th>
                  <th className="px-4 py-3">Título</th>
                  <th className="px-4 py-3">Titular</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Prazo interno</th>
                  <th className="px-4 py-3">Monitorado</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-line">
                {assetsQuery.data.map((asset) => (
                  <tr key={asset.id} className="hover:bg-slate-50">
                    <td className="px-4 py-3">{formatType(asset.type)}</td>
                    <td className="whitespace-nowrap px-4 py-3 font-medium text-ink">{asset.inpiProcessNumber ?? "Manual"}</td>
                    <td className="min-w-64 px-4 py-3 text-slate-700">{asset.title}</td>
                    <td className="min-w-56 px-4 py-3 text-slate-600">{asset.ownerName ?? "Não informado"}</td>
                    <td className="px-4 py-3 text-slate-600">{asset.status}</td>
                    <td className="px-4 py-3 text-slate-600">{formatDate(asset.internalDeadline)}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center gap-1 rounded-full border px-2 py-1 text-xs font-medium ${asset.isMonitored ? "border-emerald-200 bg-emerald-50 text-emerald-700" : "border-slate-200 bg-slate-50 text-slate-600"}`}>
                        {asset.isMonitored ? <ShieldCheck size={13} /> : null}
                        {asset.isMonitored ? "Sim" : "Não"}
                      </span>
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

function formatType(value: string) {
  const labels: Record<string, string> = {
    Trademark: "Marca",
    Patent: "Patente",
    Software: "Software",
    IndustrialDesign: "Desenho industrial"
  };
  return labels[value] ?? value;
}

function State({ text, loading = false }: { text: string; loading?: boolean }) {
  return <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">{loading ? <Loader2 className="animate-spin" size={18} /> : null}{text}</div>;
}
