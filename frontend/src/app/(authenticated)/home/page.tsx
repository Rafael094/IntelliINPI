"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import {
  Activity,
  AlertCircle,
  ArrowRight,
  Building2,
  CalendarDays,
  Clock3,
  FilePlus2,
  Loader2,
  Radar,
  RefreshCw,
  Search,
  Server,
  ShieldCheck,
  Star
} from "lucide-react";
import { StatusBadge, formatDate, formatDateTime } from "@/components/status-badge";
import { useAuth } from "@/lib/auth";
import { getOperationalHome } from "@/lib/queries";

const quickActions = [
  {
    href: "/inpi/trademarks/search",
    title: "Buscar marca",
    description: "Consultar processos e titulares no INPI.",
    icon: Search,
    color: "bg-teal-50 text-teal-700"
  },
  {
    href: "/clients",
    title: "Cadastrar cliente",
    description: "Organizar clientes e dados de contato.",
    icon: Building2,
    color: "bg-sky-50 text-sky-700"
  },
  {
    href: "/monitoring",
    title: "Monitoramento",
    description: "Acompanhar marcas e novos despachos.",
    icon: Radar,
    color: "bg-amber-50 text-amber-700"
  },
  {
    href: "/nit/inventions",
    title: "Nova invenção",
    description: "Registrar uma tecnologia no módulo NIT.",
    icon: FilePlus2,
    color: "bg-violet-50 text-violet-700"
  }
];

export default function OperationalHomePage() {
  const { user } = useAuth();
  const homeQuery = useQuery({
    queryKey: ["operational-home"],
    queryFn: getOperationalHome,
    refetchInterval: 30_000,
    retry: 1
  });
  const data = homeQuery.data;

  return (
    <div className="space-y-6">
      <section className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-brand">Central operacional</p>
          <h1 className="mt-1 text-2xl font-semibold text-ink">Olá, {displayName(user?.email)}</h1>
          <p className="mt-1 text-sm text-slate-500">Acompanhe as prioridades do dia e acesse rapidamente as principais operações.</p>
        </div>
        <div className="flex items-center gap-2 rounded-md border border-line bg-white px-3 py-2 shadow-panel">
          {homeQuery.isFetching ? <Loader2 className="animate-spin text-brand" size={16} /> : <Activity className="text-brand" size={16} />}
          <span className="text-xs text-slate-600">Atualização automática a cada 30 segundos</span>
        </div>
      </section>

      {homeQuery.isError ? (
        <div className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-amber-200 bg-amber-50 px-4 py-3">
          <div className="flex items-start gap-3">
            <AlertCircle className="mt-0.5 shrink-0 text-amber-700" size={18} />
            <div>
              <p className="text-sm font-medium text-amber-900">Os indicadores operacionais estão temporariamente indisponíveis.</p>
              <p className="mt-0.5 text-xs text-amber-700">Os atalhos continuam funcionando normalmente. Verifique a conexão com o backend e tente novamente.</p>
            </div>
          </div>
          <button type="button" onClick={() => homeQuery.refetch()} className="inline-flex h-9 items-center gap-2 rounded-md border border-amber-300 bg-white px-3 text-sm font-medium text-amber-800 hover:bg-amber-100">
            <RefreshCw size={15} />
            Tentar novamente
          </button>
        </div>
      ) : null}

      <section>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-base font-semibold text-ink">Acesso rápido</h2>
          <span className="text-xs text-slate-500">Operações frequentes</span>
        </div>
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          {quickActions.map((action) => {
            const Icon = action.icon;
            return (
              <Link key={action.href} href={action.href} className="group flex min-h-28 items-start gap-3 rounded-lg border border-line bg-white p-4 shadow-panel transition hover:border-teal-200 hover:shadow-md">
                <span className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-md ${action.color}`}><Icon size={19} /></span>
                <span className="min-w-0">
                  <span className="flex items-center gap-2 text-sm font-semibold text-ink">{action.title}<ArrowRight className="text-slate-300 transition group-hover:translate-x-0.5 group-hover:text-brand" size={14} /></span>
                  <span className="mt-1 block text-xs leading-5 text-slate-500">{action.description}</span>
                </span>
              </Link>
            );
          })}
        </div>
      </section>

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Metric title="Prazos pendentes hoje" value={data?.pendingDeadlinesToday} icon={CalendarDays} loading={homeQuery.isLoading} />
        <Metric title="Eventos não lidos" value={data?.unreadMonitoringEvents} icon={AlertCircle} loading={homeQuery.isLoading} />
        <Metric title="Marcas com mudança" value={data?.monitoredTrademarksWithChanges.length} icon={Star} loading={homeQuery.isLoading} />
        <div className="min-h-32 rounded-lg border border-line bg-white p-4 shadow-panel">
          <div className="flex items-center justify-between gap-3"><p className="text-sm text-slate-500">Última RPI</p><Clock3 className="text-brand" size={18} /></div>
          {homeQuery.isLoading ? <LoadingBar /> : <p className="mt-3 text-2xl font-semibold text-ink">{data?.lastRpiNumber ?? "--"}</p>}
          <div className="mt-2">{data?.lastRpiImportStatus ? <StatusBadge status={data.lastRpiImportStatus} /> : <span className="text-xs text-slate-500">Sem importação registrada</span>}</div>
        </div>
      </section>

      <section className="grid gap-4 lg:grid-cols-3">
        <HealthItem title="Backend" description={homeQuery.isError ? "Sem resposta da API" : homeQuery.isLoading ? "Verificando conexão" : "API operacional"} healthy={!homeQuery.isError} loading={homeQuery.isLoading} icon={Server} />
        <HealthItem title="Importação RPI" description={data?.lastRpiNumber ? `Última edição registrada: ${data.lastRpiNumber}` : "Nenhuma edição identificada"} healthy={Boolean(data?.lastRpiNumber)} loading={homeQuery.isLoading} icon={ShieldCheck} />
        <HealthItem title="Monitoramento" description={data ? `${data.unreadMonitoringEvents} evento(s) aguardando leitura` : "Aguardando indicadores"} healthy={data?.unreadMonitoringEvents === 0} loading={homeQuery.isLoading} icon={Radar} />
      </section>

      <div className="grid gap-6 xl:grid-cols-2">
        <Panel title="Próximos prazos" href="/deadlines" loading={homeQuery.isLoading}>
          {!data || data.upcomingDeadlines.length === 0 ? <Empty text="Nenhum prazo próximo." /> : data.upcomingDeadlines.map((deadline) => (
            <div key={deadline.id} className="border-b border-line px-4 py-3 last:border-b-0">
              <p className="text-sm font-medium text-ink">{deadline.title}</p>
              <p className="mt-1 text-xs text-slate-500">{formatDeadlineType(deadline.type)} · vence em {formatDate(deadline.dueDate)}</p>
              <p className="mt-1 text-xs text-slate-400">{deadline.clientName ?? deadline.trademarkProcessNumber ?? deadline.inventionTitle ?? "Sem vínculo"}</p>
            </div>
          ))}
        </Panel>

        <Panel title="Eventos recentes" href="/events" loading={homeQuery.isLoading}>
          {!data || data.recentEvents.length === 0 ? <Empty text="Nenhum evento recente." /> : data.recentEvents.map((event) => (
            <div key={event.id} className="border-b border-line px-4 py-3 last:border-b-0">
              <p className="text-sm font-medium text-ink">{event.processNumber} · {event.trademarkName || "Sem nome"}</p>
              <p className="mt-1 text-xs text-slate-500">{event.previousDispatchCode ?? "Não informado"} → {event.currentDispatchCode ?? "Não informado"} · {formatDate(event.currentDispatchDate)}</p>
              <p className="mt-1 text-xs text-slate-400">{event.isRead ? "Lido" : "Pendente"} · {formatDateTime(event.createdAtUtc)}</p>
            </div>
          ))}
        </Panel>

        <Panel title="Marcas com mudança pendente" href="/monitoring" loading={homeQuery.isLoading}>
          {!data || data.monitoredTrademarksWithChanges.length === 0 ? <Empty text="Nenhuma marca com mudança pendente." /> : data.monitoredTrademarksWithChanges.map((item) => (
            <div key={item.id} className="border-b border-line px-4 py-3 last:border-b-0">
              <p className="text-sm font-medium text-ink">{item.processNumber} · {item.trademarkName || "Sem nome"}</p>
              <p className="mt-1 text-xs text-slate-500">Último despacho {item.lastKnownDispatchCode ?? "Não informado"} · {formatDate(item.lastKnownDispatchDate)}</p>
            </div>
          ))}
        </Panel>
      </div>
    </div>
  );
}

function Metric({ title, value, icon: Icon, loading }: { title: string; value?: number; icon: typeof CalendarDays; loading: boolean }) {
  return <div className="min-h-32 rounded-lg border border-line bg-white p-4 shadow-panel"><div className="flex items-center justify-between gap-3"><p className="text-sm text-slate-500">{title}</p><Icon className="text-brand" size={18} /></div>{loading ? <LoadingBar /> : <p className="mt-3 text-2xl font-semibold text-ink">{value === undefined ? "--" : value.toLocaleString("pt-BR")}</p>}</div>;
}

function HealthItem({ title, description, healthy, loading, icon: Icon }: { title: string; description: string; healthy: boolean; loading: boolean; icon: typeof Server }) {
  const state = loading ? "bg-slate-300" : healthy ? "bg-emerald-500" : "bg-amber-500";
  return <div className="flex items-center gap-3 rounded-lg border border-line bg-white p-4 shadow-panel"><span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-slate-50 text-slate-600"><Icon size={18} /></span><div className="min-w-0"><div className="flex items-center gap-2"><span className={`h-2 w-2 rounded-full ${state}`} /><p className="text-sm font-semibold text-ink">{title}</p></div><p className="mt-0.5 truncate text-xs text-slate-500">{description}</p></div></div>;
}

function Panel({ title, href, loading, children }: { title: string; href: string; loading: boolean; children: React.ReactNode }) {
  return <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel"><div className="flex items-center justify-between border-b border-line px-4 py-3"><h2 className="text-base font-semibold text-ink">{title}</h2><Link href={href} className="inline-flex items-center gap-1 text-sm font-medium text-brand hover:text-teal-800">Abrir<ArrowRight size={14} /></Link></div>{loading ? <div className="flex min-h-28 items-center justify-center gap-2 text-sm text-slate-500"><Loader2 className="animate-spin" size={17} />Carregando...</div> : children}</section>;
}

function Empty({ text }: { text: string }) { return <div className="px-4 py-8 text-sm text-slate-500">{text}</div>; }
function LoadingBar() { return <div className="mt-4 h-7 w-20 animate-pulse rounded bg-slate-100" />; }
function displayName(email?: string | null) { const value = email?.split("@")[0]?.trim(); return value ? value.charAt(0).toUpperCase() + value.slice(1) : "bem-vindo"; }

function formatDeadlineType(value: string) {
  const labels: Record<string, string> = { INPIRequirement: "Exigência INPI", Opposition: "Oposição", Appeal: "Recurso", Annuity: "Anuidade", Renewal: "Renovação", ContractExpiration: "Vencimento de contrato", Other: "Outro" };
  return labels[value] ?? value;
}
