"use client";

import { useQuery } from "@tanstack/react-query";
import { Building2, ClipboardList, FileText, Loader2, Scale, Users, type LucideIcon } from "lucide-react";
import { getNitDashboardOverview } from "@/lib/queries";

export default function NitDashboardPage() {
  const overviewQuery = useQuery({
    queryKey: ["nit-dashboard-overview"],
    queryFn: getNitDashboardOverview,
    refetchInterval: 30000
  });

  const overview = overviewQuery.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Dashboard NIT</h1>
        <p className="mt-1 text-sm text-slate-500">Visão operacional do núcleo de inovação e transferência de tecnologia.</p>
      </div>

      {overviewQuery.isLoading ? (
        <div className="flex min-h-72 items-center justify-center gap-2 rounded-lg border border-line bg-white text-sm text-slate-500 shadow-panel">
          <Loader2 className="animate-spin" size={18} />
          Carregando dashboard NIT...
        </div>
      ) : null}

      {overviewQuery.isError ? (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          Não foi possível carregar o dashboard NIT.
        </div>
      ) : null}

      {overview ? (
        <>
          <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <MetricCard title="Instituições" value={overview.totalInstitutions} icon={Building2} />
            <MetricCard title="Pesquisadores" value={overview.totalResearchers} icon={Users} />
            <MetricCard title="Invenções" value={overview.totalInventions} icon={ClipboardList} />
            <MetricCard title="Contratos" value={overview.totalContracts} icon={FileText} />
            <MetricCard title="Empresas" value={overview.totalCompanies} icon={Building2} />
            <MetricCard title="Royalties" value={formatCurrency(overview.totalRoyalties)} icon={Scale} />
            <MetricCard title="Tecnologias licenciadas" value={overview.totalLicensedTechnologies} icon={Scale} />
          </section>

          <section className="grid gap-4 lg:grid-cols-2">
            <OperationalChart title="Invenções por status" items={overview.inventionsByStatus} />
            <OperationalChart title="Contratos por tipo" items={overview.contractsByType} />
            <OperationalChart title="Royalties por período" items={overview.royaltiesByPeriod} currency />
            <OperationalChart title="Pipeline de transferência" items={overview.transferPipeline} />
          </section>
        </>
      ) : null}
    </div>
  );
}

function MetricCard({
  title,
  value,
  icon: Icon
}: {
  title: string;
  value: string | number;
  icon: LucideIcon;
}) {
  return (
    <div className="rounded-lg border border-line bg-white p-4 shadow-panel">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-500">{title}</p>
        <Icon className="text-brand" size={18} />
      </div>
      <p className="mt-3 text-2xl font-semibold text-ink">{value}</p>
    </div>
  );
}

function OperationalChart({ title, items, currency = false }: { title: string; items: Array<{ label: string; value: number }>; currency?: boolean }) {
  const max = Math.max(...items.map(x => x.value), 1);
  return <div className="rounded-lg border border-line bg-white p-4 shadow-panel"><h2 className="text-sm font-semibold">{title}</h2><div className="mt-4 space-y-3">{items.length ? items.map(item => <div key={item.label}><div className="mb-1 flex justify-between gap-3 text-xs"><span className="truncate text-slate-600">{item.label}</span><strong>{currency ? formatCurrency(item.value) : item.value}</strong></div><div className="h-2 overflow-hidden rounded bg-slate-100"><div className="h-full bg-brand" style={{ width: `${Math.max((item.value / max) * 100, 2)}%` }} /></div></div>) : <p className="py-8 text-center text-sm text-slate-500">Sem dados no período.</p>}</div></div>;
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL"
  }).format(value);
}
