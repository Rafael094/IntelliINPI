"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { AlertCircle, CalendarDays, Clock3, Loader2, Star } from "lucide-react";
import { StatusBadge, formatDate, formatDateTime } from "@/components/status-badge";
import { getOperationalHome } from "@/lib/queries";

export default function OperationalHomePage() {
  const homeQuery = useQuery({
    queryKey: ["operational-home"],
    queryFn: getOperationalHome,
    refetchInterval: 30000
  });

  if (homeQuery.isLoading) {
    return <PageMessage text="Carregando início operacional..." loading />;
  }

  if (homeQuery.isError) {
    return <PageMessage text="Não foi possível carregar o início operacional." />;
  }

  const data = homeQuery.data;
  if (!data) {
    return <PageMessage text="Nenhum dado operacional encontrado." />;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Início</h1>
        <p className="mt-1 text-sm text-slate-500">Visão operacional de prazos, monitoramento e importação RPI.</p>
      </div>

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Metric title="Prazos pendentes hoje" value={data.pendingDeadlinesToday} icon={CalendarDays} />
        <Metric title="Eventos não lidos" value={data.unreadMonitoringEvents} icon={AlertCircle} />
        <Metric title="Marcas com mudança" value={data.monitoredTrademarksWithChanges.length} icon={Star} />
        <div className="rounded-lg border border-line bg-white p-4 shadow-panel">
          <div className="flex items-center justify-between gap-3">
            <p className="text-sm text-slate-500">Última RPI</p>
            <Clock3 className="text-brand" size={18} />
          </div>
          <p className="mt-3 text-2xl font-semibold text-ink">{data.lastRpiNumber ?? "--"}</p>
          <div className="mt-2">{data.lastRpiImportStatus ? <StatusBadge status={data.lastRpiImportStatus} /> : <span className="text-xs text-slate-500">Sem status</span>}</div>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-2">
        <Panel title="Próximos prazos" href="/deadlines">
          {data.upcomingDeadlines.length === 0 ? (
            <Empty text="Nenhum prazo próximo." />
          ) : (
            data.upcomingDeadlines.map((deadline) => (
              <div key={deadline.id} className="border-b border-line px-4 py-3 last:border-b-0">
                <p className="text-sm font-medium text-ink">{deadline.title}</p>
                <p className="mt-1 text-xs text-slate-500">{formatDeadlineType(deadline.type)} · vence em {formatDate(deadline.dueDate)}</p>
                <p className="mt-1 text-xs text-slate-400">{deadline.clientName ?? deadline.trademarkProcessNumber ?? deadline.inventionTitle ?? "Sem vínculo"}</p>
              </div>
            ))
          )}
        </Panel>

        <Panel title="Eventos recentes" href="/events">
          {data.recentEvents.length === 0 ? (
            <Empty text="Nenhum evento recente." />
          ) : (
            data.recentEvents.map((event) => (
              <div key={event.id} className="border-b border-line px-4 py-3 last:border-b-0">
                <p className="text-sm font-medium text-ink">{event.processNumber} · {event.trademarkName || "Sem nome"}</p>
                <p className="mt-1 text-xs text-slate-500">
                  {event.previousDispatchCode ?? "Não informado"} → {event.currentDispatchCode ?? "Não informado"} · {formatDate(event.currentDispatchDate)}
                </p>
                <p className="mt-1 text-xs text-slate-400">{event.isRead ? "Lido" : "Pendente"} · {formatDateTime(event.createdAtUtc)}</p>
              </div>
            ))
          )}
        </Panel>

        <Panel title="Marcas com mudança pendente" href="/monitoring">
          {data.monitoredTrademarksWithChanges.length === 0 ? (
            <Empty text="Nenhuma marca com mudança pendente." />
          ) : (
            data.monitoredTrademarksWithChanges.map((item) => (
              <div key={item.id} className="border-b border-line px-4 py-3 last:border-b-0">
                <p className="text-sm font-medium text-ink">{item.processNumber} · {item.trademarkName || "Sem nome"}</p>
                <p className="mt-1 text-xs text-slate-500">Último despacho {item.lastKnownDispatchCode ?? "Não informado"} · {formatDate(item.lastKnownDispatchDate)}</p>
              </div>
            ))
          )}
        </Panel>
      </div>
    </div>
  );
}

function Metric({ title, value, icon: Icon }: { title: string; value: number; icon: typeof CalendarDays }) {
  return (
    <div className="rounded-lg border border-line bg-white p-4 shadow-panel">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-500">{title}</p>
        <Icon className="text-brand" size={18} />
      </div>
      <p className="mt-3 text-2xl font-semibold text-ink">{value.toLocaleString("pt-BR")}</p>
    </div>
  );
}

function Panel({ title, href, children }: { title: string; href: string; children: React.ReactNode }) {
  return (
    <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
      <div className="flex items-center justify-between border-b border-line px-4 py-3">
        <h2 className="text-base font-semibold text-ink">{title}</h2>
        <Link href={href} className="text-sm font-medium text-brand hover:text-teal-800">Abrir</Link>
      </div>
      {children}
    </section>
  );
}

function Empty({ text }: { text: string }) {
  return <div className="px-4 py-8 text-sm text-slate-500">{text}</div>;
}

function PageMessage({ text, loading }: { text: string; loading?: boolean }) {
  return (
    <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">
      {loading ? <Loader2 className="animate-spin" size={18} /> : null}
      {text}
    </div>
  );
}

function formatDeadlineType(value: string) {
  const labels: Record<string, string> = {
    INPIRequirement: "Exigência INPI",
    Opposition: "Oposição",
    Appeal: "Recurso",
    Annuity: "Anuidade",
    Renewal: "Renovação",
    ContractExpiration: "Vencimento de contrato",
    Other: "Outro"
  };

  return labels[value] ?? value;
}
