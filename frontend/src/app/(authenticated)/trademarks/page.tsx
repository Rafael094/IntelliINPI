"use client";

import { FormEvent, useMemo, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertCircle, ChevronLeft, ChevronRight, Loader2, Search, Star } from "lucide-react";
import { AxiosError } from "axios";
import { monitorTrademark, searchTrademarks } from "@/lib/queries";
import type { TrademarkSearchParams } from "@/lib/types";

type Filters = {
  query: string;
  niceClass: string;
  status: string;
  owner: string;
};

const initialFilters: Filters = {
  query: "",
  niceClass: "",
  status: "",
  owner: ""
};

export default function TrademarksPage() {
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const initialQuery = searchParams.get("query") ?? "";
  const [filters, setFilters] = useState({ ...initialFilters, query: initialQuery });
  const [appliedFilters, setAppliedFilters] = useState({ ...initialFilters, query: initialQuery });
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [monitoringMessage, setMonitoringMessage] = useState<string | null>(null);

  const params = useMemo<TrademarkSearchParams>(
    () => ({
      query: appliedFilters.query || undefined,
      niceClass: appliedFilters.niceClass || undefined,
      status: appliedFilters.status || undefined,
      owner: appliedFilters.owner || undefined,
      page,
      pageSize
    }),
    [appliedFilters, page, pageSize]
  );
  const hasAppliedFilters = Object.values(appliedFilters).some((value) => value.trim().length > 0);

  const trademarksQuery = useQuery({
    queryKey: ["trademarks", params],
    queryFn: () => searchTrademarks(params),
    enabled: hasAppliedFilters
  });

  const monitorMutation = useMutation({
    mutationFn: monitorTrademark,
    onSuccess: () => {
      setMonitoringMessage("Marca adicionada ao monitoramento.");
      void queryClient.invalidateQueries({ queryKey: ["monitoring"] });
    },
    onError: (error) => {
      const axiosError = error as AxiosError<{ message?: string; error?: string; errors?: string[] }>;
      setMonitoringMessage(
        axiosError.response?.data?.message
          ?? axiosError.response?.data?.error
          ?? axiosError.response?.data?.errors?.join(" ")
          ?? "Não foi possível monitorar a marca."
      );
    }
  });

  const totalItems = hasAppliedFilters ? trademarksQuery.data?.totalItems ?? 0 : 0;
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPage(1);
    setMonitoringMessage(null);
    setAppliedFilters({
      query: filters.query.trim(),
      niceClass: filters.niceClass.trim(),
      status: filters.status.trim(),
      owner: filters.owner.trim()
    });
  }

  function updateFilter(key: keyof Filters, value: string) {
    setFilters((current) => ({ ...current, [key]: value }));
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Buscar Marcas</h1>
        <p className="mt-1 text-sm text-slate-500">Busca local no PostgreSQL com dados importados do INPI.</p>
      </div>

      <section className="rounded-lg border border-line bg-white p-4 shadow-panel">
        <form className="grid gap-3 lg:grid-cols-[2fr_1fr_1fr_1fr_auto]" onSubmit={handleSubmit}>
          <TextInput
            label="Busca"
            value={filters.query}
            onChange={(value) => updateFilter("query", value)}
            placeholder="Processo ou marca"
          />
          <TextInput
            label="Classe Nice"
            value={filters.niceClass}
            onChange={(value) => updateFilter("niceClass", value)}
            placeholder="Ex: 35"
          />
          <TextInput
            label="Status"
            value={filters.status}
            onChange={(value) => updateFilter("status", value)}
            placeholder="Status"
          />
          <TextInput
            label="Titular"
            value={filters.owner}
            onChange={(value) => updateFilter("owner", value)}
            placeholder="Titular"
          />
          <div className="flex items-end">
            <button
              type="submit"
              className="flex h-10 w-full items-center justify-center gap-2 rounded-md bg-brand px-4 text-sm font-medium text-white hover:bg-teal-800"
            >
              <Search size={16} />
              Buscar
            </button>
          </div>
        </form>
      </section>

      {monitoringMessage ? (
        <div className="rounded-md border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          {monitoringMessage}
        </div>
      ) : null}

      <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
          <div>
            <p className="text-sm font-medium text-ink">Resultado da busca</p>
            <p className="text-xs text-slate-500">
              {hasAppliedFilters
                ? `${totalItems.toLocaleString("pt-BR")} marcas encontradas`
                : "Informe um filtro para consultar a base local"}
            </p>
          </div>
          <label className="flex items-center gap-2 text-sm text-slate-600">
            Linhas
            <select
              value={pageSize}
              onChange={(event) => {
                setPageSize(Number(event.target.value));
                setPage(1);
              }}
              className="h-9 rounded-md border border-line bg-white px-2 outline-none focus:border-brand"
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
              <option value={100}>100</option>
            </select>
          </label>
        </div>

        {!hasAppliedFilters ? <InitialState /> : null}
        {hasAppliedFilters && trademarksQuery.isLoading ? <LoadingState /> : null}
        {hasAppliedFilters && trademarksQuery.isError ? <ErrorState onRetry={() => void trademarksQuery.refetch()} /> : null}
        {hasAppliedFilters && trademarksQuery.isSuccess && trademarksQuery.data.items.length === 0 ? <EmptyState /> : null}

        {hasAppliedFilters && trademarksQuery.isSuccess && trademarksQuery.data.items.length > 0 ? (
          <>
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-slate-50 text-xs uppercase text-slate-500">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Processo</th>
                    <th className="px-4 py-3 font-semibold">Marca</th>
                    <th className="px-4 py-3 font-semibold">Status</th>
                    <th className="px-4 py-3 font-semibold">Classe Nice</th>
                    <th className="px-4 py-3 font-semibold">Titular</th>
                    <th className="px-4 py-3 font-semibold">Último despacho</th>
                    <th className="px-4 py-3 text-right font-semibold">Ações</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-line">
                  {trademarksQuery.data.items.map((trademark) => (
                    <tr key={trademark.id} className="hover:bg-slate-50">
                        <td className="whitespace-nowrap px-4 py-3 font-medium">
                          <Link href={`/inpi/trademarks/${encodeURIComponent(trademark.processNumber)}`} className="text-brand hover:text-teal-800">
                            {trademark.processNumber}
                          </Link>
                        </td>
                      <td className="min-w-56 px-4 py-3 text-slate-800">{trademark.name || "Sem nome"}</td>
                      <td className="min-w-44 px-4 py-3 text-slate-600">{trademark.status ?? "Não informado"}</td>
                      <td className="px-4 py-3 text-slate-600">{formatList(trademark.niceClasses)}</td>
                      <td className="min-w-56 px-4 py-3 text-slate-600">{formatList(trademark.owners)}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{formatDate(trademark.lastDispatchDate)}</td>
                      <td className="px-4 py-3 text-right">
                        <button
                          type="button"
                          onClick={() => monitorMutation.mutate(trademark.id)}
                          disabled={monitorMutation.isPending}
                          className="inline-flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-60"
                        >
                          <Star size={16} />
                          Monitorar
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex flex-wrap items-center justify-between gap-3 border-t border-line px-4 py-3">
              <p className="text-sm text-slate-500">
                Página {page} de {totalPages}
              </p>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => setPage((current) => Math.max(1, current - 1))}
                  disabled={page <= 1}
                  className="flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <ChevronLeft size={16} />
                  Anterior
                </button>
                <button
                  type="button"
                  onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
                  disabled={page >= totalPages}
                  className="flex h-9 items-center gap-2 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  Próxima
                  <ChevronRight size={16} />
                </button>
              </div>
            </div>
          </>
        ) : null}
      </section>
    </div>
  );
}

function TextInput({
  label,
  value,
  onChange,
  placeholder
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  placeholder: string;
}) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-medium text-slate-700">{label}</span>
      <input
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        className="h-10 w-full rounded-md border border-line px-3 text-sm outline-none focus:border-brand focus:ring-2 focus:ring-teal-100"
      />
    </label>
  );
}

function LoadingState() {
  return (
    <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500">
      <Loader2 className="animate-spin" size={18} />
      Carregando marcas...
    </div>
  );
}

function InitialState() {
  return (
    <div className="flex min-h-64 flex-col items-center justify-center px-4 text-center">
      <Search className="text-slate-400" size={28} />
      <p className="mt-3 text-sm font-medium text-ink">Pesquise uma marca ou processo</p>
      <p className="mt-1 max-w-md text-sm text-slate-500">
        A importação histórica da RPI cria muitos registros mínimos. Use ao menos um filtro para consultar a base local.
      </p>
    </div>
  );
}

function ErrorState({ onRetry }: { onRetry: () => void }) {
  return (
    <div className="flex min-h-64 flex-col items-center justify-center gap-3 px-4 text-center">
      <AlertCircle className="text-red-600" size={26} />
      <div>
        <p className="text-sm font-medium text-ink">Erro ao buscar marcas</p>
        <p className="mt-1 text-sm text-slate-500">Verifique se o backend está acessível em localhost:5076.</p>
      </div>
      <button
        type="button"
        onClick={onRetry}
        className="h-9 rounded-md border border-line px-3 text-sm text-slate-700 hover:bg-slate-100"
      >
        Tentar novamente
      </button>
    </div>
  );
}

function EmptyState() {
  return (
    <div className="flex min-h-64 flex-col items-center justify-center px-4 text-center">
      <p className="text-sm font-medium text-ink">Nenhuma marca encontrada</p>
      <p className="mt-1 text-sm text-slate-500">Ajuste os filtros ou importe novas RPIs.</p>
    </div>
  );
}

function formatList(values: string[]) {
  return values.length > 0 ? values.join(", ") : "Não informado";
}

function formatDate(value: string | null) {
  if (!value) {
    return "Não informado";
  }

  return new Intl.DateTimeFormat("pt-BR", { timeZone: "UTC" }).format(new Date(`${value}T00:00:00Z`));
}
