"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, ExternalLink, Loader2 } from "lucide-react";
import { formatDate } from "@/components/status-badge";
import { getTrademarkDetail } from "@/lib/queries";

export default function TrademarkDetailPage() {
  const params = useParams<{ processNumber: string }>();
  const processNumber = decodeURIComponent(params.processNumber);
  const detailQuery = useQuery({
    queryKey: ["trademark-detail", processNumber],
    queryFn: () => getTrademarkDetail(processNumber)
  });

  if (detailQuery.isLoading) {
    return <PageMessage text="Carregando ficha da marca..." loading />;
  }

  if (detailQuery.isError) {
    return <PageMessage text="Nao foi possivel carregar a ficha da marca." />;
  }

  const trademark = detailQuery.data;
  if (!trademark) {
    return <PageMessage text="Marca nao encontrada." />;
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <Link href="/inpi/trademarks/search" className="inline-flex items-center gap-2 text-sm text-brand hover:text-teal-800">
            <ArrowLeft size={16} />
            Voltar para busca
          </Link>
          <h1 className="mt-2 text-2xl font-semibold text-ink">Ficha da Marca</h1>
          <p className="mt-1 text-sm text-slate-500">Dados salvos localmente a partir das fontes do INPI.</p>
        </div>
        {trademark.inpiDetailUrl ? (
          <a
            href={trademark.inpiDetailUrl}
            target="_blank"
            rel="noreferrer"
            className="inline-flex h-10 items-center gap-2 rounded-md border border-line bg-white px-4 text-sm font-medium text-slate-700 hover:bg-slate-50"
          >
            Abrir no INPI
            <ExternalLink size={16} />
          </a>
        ) : null}
      </div>

      <section className="rounded-lg border border-line bg-white p-5 shadow-panel">
        <div className="grid gap-5 lg:grid-cols-[1fr_260px]">
          <div className="space-y-3">
            <InfoRow label="No do Processo" value={trademark.processNumber} highlight />
            <InfoRow label="Marca" value={trademark.name || "Sem nome"} />
            <InfoRow label="Situacao" value={trademark.status ?? "Nao informado"} />
            <InfoRow label="Apresentacao" value={trademark.presentation ?? "Nao importado"} />
            <InfoRow label="Natureza" value={trademark.nature ?? "Nao importado"} />
          </div>
          <div className="flex min-h-44 items-center justify-center rounded-md border border-dashed border-line bg-slate-50 p-3 text-center text-sm text-slate-500">
            {trademark.logoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={trademark.logoUrl}
                alt={`Logo da marca ${trademark.name}`}
                className="max-h-56 max-w-full object-contain"
              />
            ) : (
              "Imagem da marca nao importada localmente"
            )}
          </div>
        </div>
      </section>

      <DetailTable
        title="Classificacao de Produtos / Servicos"
        headers={["Classe de Nice", "Situacao da Classe", "Especificacao"]}
        rows={trademark.niceClasses.map((item) => [
          `NCL(${item.classNumber || item.code}) ${Number(item.code).toString() === item.code ? "" : item.code}`.trim(),
          trademark.status ?? "Nao informado",
          item.specification || "Nao importado"
        ])}
        emptyText="Classificacao Nice nao importada."
      />

      <DetailTable
        title="Classificacao Internacional de Viena"
        headers={["Edicao", "Codigo", "Descricao"]}
        rows={trademark.viennaClasses.map((item) => [
          item.edition,
          item.code,
          item.description ?? "Descricao nao importada no XML/RPI"
        ])}
        emptyText="Classificacao de Viena nao importada."
      />

      <DetailTable
        title="Titulares"
        headers={["", "Nome"]}
        rows={trademark.owners.map((owner, index) => [`Titular(${index + 1})`, owner])}
        emptyText="Titular nao importado."
      />

      <DetailTable
        title="Representante Legal"
        headers={["", "Nome"]}
        rows={[["Procurador", trademark.legalRepresentative ?? "Nao importado"]]}
      />

      <DetailTable
        title="Datas"
        headers={["Data de Deposito", "Data de Concessao", "Data de Vigencia"]}
        rows={[[
          formatDate(trademark.filingDate),
          formatDate(trademark.registrationDate),
          formatDate(trademark.expirationDate)
        ]]}
      />

      <DetailTable
        title="Prazos para prorrogacao de registro de marca"
        headers={["", "Prazo Ordinario", "Prazo Extraordinario"]}
        rows={[
          ["Inicio", formatDate(trademark.renewalWindow.ordinaryStart), formatDate(trademark.renewalWindow.extraordinaryStart)],
          ["Fim", formatDate(trademark.renewalWindow.ordinaryEnd), formatDate(trademark.renewalWindow.extraordinaryEnd)]
        ]}
      />

      <DetailTable
        title="Petições"
        headers={["Protocolo", "Data", "Serviço", "Cliente", "Delivery", "Data"]}
        rows={trademark.petitions.map((item) => [
          item.protocol,
          formatDate(item.filedAt),
          item.serviceCode || "Nao informado",
          item.clientName || "Nao informado",
          item.delivery || "-",
          formatDate(item.deliveryDate)
        ])}
        emptyText="Petições não importadas."
      />

      <DetailTable
        title="Publicacoes / Despachos"
        headers={["RPI", "Data RPI", "Despacho", "Complemento do Despacho"]}
        rows={trademark.dispatches.map((item) => [
          item.rpiNumber?.toString() ?? "Nao informado",
          formatDate(item.publishedAt),
          item.code,
          item.description
        ])}
        emptyText="Nenhum despacho importado."
      />
    </div>
  );
}

function InfoRow({ label, value, highlight }: { label: string; value: string; highlight?: boolean }) {
  return (
    <div className="grid gap-2 text-sm sm:grid-cols-[180px_1fr]">
      <div className="font-medium text-slate-600">{label}:</div>
      <div className={highlight ? "font-semibold text-amber-700" : "text-ink"}>{value}</div>
    </div>
  );
}

function DetailTable({ title, headers, rows, emptyText }: { title: string; headers: string[]; rows: string[][]; emptyText?: string }) {
  return (
    <section className="overflow-hidden rounded-lg border border-line bg-white shadow-panel">
      <div className="border-b border-line bg-slate-100 px-4 py-2 text-sm font-semibold text-teal-800">{title}</div>
      {rows.length === 0 ? (
        <div className="px-4 py-5 text-sm text-slate-500">{emptyText ?? "Nao informado."}</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full text-left text-sm">
            <thead className="bg-emerald-100 text-xs text-slate-700">
              <tr>
                {headers.map((header) => (
                  <th key={header} className="border border-white px-3 py-2 font-semibold">{header}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rows.map((row, index) => (
                <tr key={`${title}-${index}`} className={index % 2 === 0 ? "bg-slate-50" : "bg-white"}>
                  {row.map((cell, cellIndex) => (
                    <td key={`${title}-${index}-${cellIndex}`} className="border border-white px-3 py-2 align-top text-slate-800">
                      {cell}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

function PageMessage({ text, loading }: { text: string; loading?: boolean }) {
  return (
    <div className="flex min-h-80 items-center justify-center gap-2 text-sm text-slate-500">
      {loading ? <Loader2 className="animate-spin" size={18} /> : null}
      {text}
    </div>
  );
}
