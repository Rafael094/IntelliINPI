"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { AlertCircle, ArrowRight, Clock3, FileClock, FileText, Loader2, Search, Star, TriangleAlert } from "lucide-react";
import { StatusBadge, formatDate, formatDateTime } from "@/components/status-badge";
import { getOperationalDashboard } from "@/lib/queries";

export default function DashboardPage() {
  const [quickSearch, setQuickSearch] = useState("");
  const dashboardQuery = useQuery({
    queryKey: ["dashboard", "operational"],
    queryFn: getOperationalDashboard,
    refetchInterval: 30000
  });

  function handleQuickSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = quickSearch.trim();
    if (value) {
      window.location.href = `/inpi/trademarks/search?query=${encodeURIComponent(value)}`;
    }
  }

  if (dashboardQuery.isLoading) {
    return <PageMessage text="Carregando dashboard operacional..." loading />;
  }

  if (dashboardQuery.isError || !dashboardQuery.data) {
    return <PageMessage text="Erro ao carregar dashboard operacional." />;
  }

  const data = dashboardQuery.data;
  const cards = [
    { label: "Ativos de PI monitorados", value: data.totalMonitoredIPAssets, detail: "Marcas e patentes", icon: Star },
    { label: "Marcas monitoradas", value: data.totalActiveMonitoredTrademarks, detail: `${data.totalMonitoredTrademarks} no total`, icon: Search },
    { label: "Patentes monitoradas", value: data.totalActiveMonitoredPatents, detail: `${data.totalMonitoredPatents} no total`, icon: FileText },
    { label: "Mudanças pendentes", value: data.totalPendingChanges, detail: formatDateTime(data.lastMonitoringCheckAtUtc), icon: TriangleAlert },
    { label: "Eventos não lidos", value: data.totalUnreadEvents, detail: "Aguardando revisão", icon: AlertCircle },
    { label: "Última RPI importada", value: data.lastImportedRpiNumber ?? 0, detail: data.lastRpiImportStatus ?? "Sem status", icon: Clock3 },
    { label: "Carga histórica", value: `${(data.historicalImportPercentage ?? 0).toFixed(2)}%`, detail: data.historicalImportCurrentRpi ? `RPI atual ${data.historicalImportCurrentRpi}` : "Sem execução", icon: FileClock }
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-ink">Dashboard operacional</h1>
          <p className="mt-1 text-sm text-slate-500">Uso operacional: monitoramento, prazos, eventos e importação RPI.</p>
        </div>
        <form className="flex w-full gap-2 sm:w-auto" onSubmit={handleQuickSearch}>
          <input value={quickSearch} onChange={(event) => setQuickSearch(event.target.value)} placeholder="Processo ou marca" className="h-10 min-w-0 flex-1 rounded-md border border-line px-3 text-sm outline-none focus:border-brand focus:ring-2 focus:ring-teal-100 sm:w-72" />
          <button className="inline-flex h-10 items-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800">
            <Search size={16} />
            Buscar
          </button>
        </form>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => {
          const Icon = card.icon;
          return (
            <section key={card.label} className="rounded-lg border border-line bg-white p-5 shadow-panel">
              <div className="flex items-center justify-between gap-3">
                <p className="text-sm text-slate-500">{card.label}</p>
                <Icon className="text-brand" size={18} />
              </div>
              <p className="mt-4 text-3xl font-semibold text-ink">{typeof card.value === "number" ? card.value.toLocaleString("pt-BR") : card.value}</p>
              <p className="mt-2 text-xs text-slate-500">{card.detail}</p>
            </section>
          );
        })}
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <ListSection title="Prazos INPI próximos" href="/deadlines" empty="Nenhum prazo INPI próximo.">
          {data.upcomingInpiDeadlines.map((item) => (
            <Row key={item.id} title={`${item.ipAssetTitle} · ${item.inpiProcessNumber ?? "sem processo"}`} detail={`${formatDate(item.dueDate)} · ${item.status === "RevisaoManualNecessaria" ? "Revisão manual necessária" : item.status}`} badge="Prazo INPI" />
          ))}
        </ListSection>
        <ListSection title="Prazos internos próximos" href="/deadlines" empty="Nenhum prazo interno próximo.">
          {data.upcomingInternalDeadlines.map((item) => (
            <Row key={item.id} title={`${item.ipAssetTitle} · ${item.inpiProcessNumber ?? "sem processo"}`} detail={`${formatDate(item.dueDate)} · ${item.status}`} badge="Prazo interno" />
          ))}
        </ListSection>
        <ListSection title="Últimos despachos" href="/events" empty="Nenhum despacho encontrado.">
          {data.latestDispatches.map((item) => (
            <Row key={`${item.assetType}-${item.processNumber}-${item.dispatchCode}-${item.dispatchDate}`} title={`${item.processNumber} · ${item.title}`} detail={`${item.dispatchCode} · ${formatDate(item.dispatchDate)} · RPI ${item.rpiNumber ?? "-"}`} badge={item.assetType === "Patent" ? "Patente" : "Marca"} />
          ))}
        </ListSection>
        <ListSection title="Falhas de sincronização INPI" href="/imports" empty="Nenhuma falha recente.">
          {data.inpiSyncFailures.map((item) => (
            <Row key={item} title={item} detail="Verifique o log de importações." badge="Atenção" />
          ))}
        </ListSection>
      </div>

      <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="text-base font-semibold text-ink">Importação RPI</h2>
            <p className="mt-1 text-sm text-slate-500">Última RPI: {data.lastImportedRpiNumber ?? "--"} · {formatDateTime(data.lastRpiImportDateUtc)}</p>
          </div>
          <div className="flex items-center gap-2">
            <StatusBadge status={data.lastRpiImportStatus} />
            <StatusBadge status={data.historicalImportStatus} />
          </div>
        </div>
      </section>
    </div>
  );
}

function ListSection({ title, href, empty, children }: { title: string; href: string; empty: string; children: React.ReactNode[] }) {
  return (
    <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
      <div className="flex items-center justify-between border-b border-line px-4 py-3">
        <h2 className="text-base font-semibold text-ink">{title}</h2>
        <Link href={href} className="inline-flex items-center gap-1 text-sm font-medium text-brand hover:text-teal-800">
          Abrir
          <ArrowRight size={15} />
        </Link>
      </div>
      {children.length === 0 ? <div className="px-4 py-8 text-sm text-slate-500">{empty}</div> : <div className="divide-y divide-line">{children}</div>}
    </section>
  );
}

function Row({ title, detail, badge }: { title: string; detail: string; badge: string }) {
  return (
    <div className="px-4 py-3">
      <div className="flex flex-wrap items-center gap-2">
        <p className="text-sm font-medium text-ink">{title}</p>
        <span className="rounded-full border border-slate-200 bg-slate-50 px-2 py-1 text-xs font-medium text-slate-600">{badge}</span>
      </div>
      <p className="mt-1 text-xs text-slate-500">{detail}</p>
    </div>
  );
}

function PageMessage({ text, loading }: { text: string; loading?: boolean }) {
  return (
    <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">
      {loading ? <Loader2 className="animate-spin" size={18} /> : null}
      {text}
    </div>
  );
}
