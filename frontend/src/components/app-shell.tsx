"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import {
  Bell,
  Building2,
  ChevronDown,
  ClipboardList,
  Database,
  FileClock,
  FileText,
  FolderOpen,
  Handshake,
  History,
  Home,
  LogOut,
  Radar,
  Search,
  ShieldCheck,
  Star,
  Users,
  WalletCards
} from "lucide-react";
import { useAuth } from "@/lib/auth";
import { listMonitoringEvents } from "@/lib/queries";

const navigationGroups = [
  {
    label: "Geral",
    items: [
      { href: "/home", label: "Início", icon: Home },
      // Portfolio de PI oculto temporariamente; a rota permanece disponivel.
    ]
  },
  {
    label: "INPI",
    items: [
      { href: "/clients", label: "Clientes", icon: Users },
      { href: "/inpi/trademarks/search", label: "Buscar Marcas", icon: Search },
      { href: "/inpi/patents/search", label: "Buscar Patentes", icon: Search },
      { href: "/brand-analysis", label: "Análise de Marca", icon: Radar },
      { href: "/monitoring", label: "Monitoramento", icon: Star }
    ]
  },
  {
    label: "NIT / INOVA+",
    items: [
      { href: "/nit/dashboard", label: "Dashboard NIT", icon: ClipboardList },
      { href: "/nit/institutions", label: "Instituições", icon: Building2 },
      { href: "/nit/researchers", label: "Pesquisadores", icon: Users },
      { href: "/nit/inventions", label: "Invenções", icon: ClipboardList },
      { href: "/nit/companies", label: "Empresas", icon: Building2 },
      { href: "/nit/contracts", label: "Contratos", icon: FileText },
      { href: "/nit/royalties", label: "Royalties", icon: WalletCards },
      { href: "/nit/transfer", label: "Transferência Tecnológica", icon: Handshake },
      { href: "/nit/documents", label: "Documentos", icon: FolderOpen },
      { href: "/nit/audit", label: "Auditoria", icon: History }
    ]
  },
  {
    label: "Configurações",
    items: [
      { href: "/imports", label: "Importações", icon: Database },
      { href: "/rpi-history", label: "Histórico RPI", icon: FileClock }
    ]
  }
];

export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { user, logout } = useAuth();
  const activeGroupLabel = navigationGroups.find((group) =>
    group.items.some((item) => pathname === item.href || pathname.startsWith(`${item.href}/`))
  )?.label;
  const [openGroups, setOpenGroups] = useState<Record<string, boolean>>(() =>
    Object.fromEntries(navigationGroups.map((group) => [group.label, group.label === activeGroupLabel]))
  );
  const unreadEventsQuery = useQuery({
    queryKey: ["events", true],
    queryFn: () => listMonitoringEvents(true),
    refetchInterval: 30_000,
    refetchOnWindowFocus: true
  });
  const unreadEventsCount = unreadEventsQuery.data?.length ?? 0;

  useEffect(() => {
    if (!activeGroupLabel) return;

    setOpenGroups((current) => ({ ...current, [activeGroupLabel]: true }));
  }, [activeGroupLabel]);

  function toggleGroup(label: string) {
    setOpenGroups((current) => ({ ...current, [label]: !current[label] }));
  }

  return (
    <div className="min-h-screen bg-slate-100 text-ink">
      <aside className="fixed inset-y-0 left-0 hidden w-64 border-r border-line bg-white lg:block">
        <div className="flex h-16 items-center gap-3 border-b border-line px-5">
          <div className="flex h-9 w-9 items-center justify-center rounded-md bg-brand text-white">
            <ShieldCheck size={20} />
          </div>
          <div>
            <p className="text-sm font-semibold">IntelliINPI</p>
            <p className="text-xs text-slate-500">Operação local</p>
          </div>
        </div>
        <nav className="space-y-4 overflow-y-auto p-3">
          {navigationGroups.map((group) => (
            <div key={group.label}>
              <button
                type="button"
                onClick={() => toggleGroup(group.label)}
                aria-expanded={Boolean(openGroups[group.label])}
                className="flex h-8 w-full items-center justify-between rounded-md px-3 text-xs font-semibold uppercase text-slate-400 transition hover:bg-slate-50 hover:text-slate-600"
              >
                <span>{group.label}</span>
                <ChevronDown
                  size={15}
                  className={`transition-transform ${openGroups[group.label] ? "rotate-180" : ""}`}
                />
              </button>
              <div className={`space-y-1 overflow-hidden ${openGroups[group.label] ? "mt-1 block" : "hidden"}`}>
                {group.items.map((item) => {
                  const Icon = item.icon;
                  const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);

                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      className={`flex h-10 items-center gap-3 rounded-md px-3 text-sm transition ${
                        isActive ? "bg-teal-50 text-brand" : "text-slate-600 hover:bg-slate-100 hover:text-ink"
                      }`}
                    >
                      <Icon size={18} />
                      {item.label}
                    </Link>
                  );
                })}
              </div>
            </div>
          ))}
        </nav>
      </aside>

      <div className="lg:pl-64">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-line bg-white px-4 shadow-panel lg:px-8">
          <div>
            <p className="text-sm font-semibold">IntelliINPI</p>
            <p className="text-xs text-slate-500 lg:hidden">Operação local</p>
          </div>
          <div className="flex items-center gap-3">
            <Link
              href="/events"
              aria-label={
                unreadEventsCount > 0
                  ? `${unreadEventsCount} evento(s) não lido(s)`
                  : "Nenhum evento não lido"
              }
              title="Eventos"
              className={`relative flex h-9 w-9 shrink-0 items-center justify-center rounded-md border transition ${
                pathname.startsWith("/events")
                  ? "border-brand bg-teal-50 text-brand"
                  : unreadEventsCount > 0
                    ? "border-red-200 bg-red-50 text-red-600 hover:bg-red-100"
                    : "border-line text-slate-600 hover:bg-slate-50 hover:text-ink"
              }`}
            >
              <Bell size={18} />
              {unreadEventsCount > 0 && (
                <span className="absolute -right-2 -top-2 flex min-h-5 min-w-5 items-center justify-center rounded-full bg-red-600 px-1 text-[10px] font-semibold leading-none text-white ring-2 ring-white">
                  {unreadEventsCount > 99 ? "99+" : unreadEventsCount}
                </span>
              )}
            </Link>
            <div className="hidden text-right sm:block">
              <p className="text-sm font-medium">{user?.email ?? "Usuário"}</p>
              <p className="text-xs text-slate-500">{user?.role ?? "Autenticado"}</p>
            </div>
            <button
              type="button"
              onClick={logout}
              className="flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-50"
            >
              <LogOut size={16} />
              Sair
            </button>
          </div>
        </header>
        <main className="px-4 py-6 lg:px-8">{children}</main>
      </div>
    </div>
  );
}
