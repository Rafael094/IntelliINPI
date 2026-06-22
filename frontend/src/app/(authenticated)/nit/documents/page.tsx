"use client";

import { useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Download, FileCheck2, FileText, FileUp, Loader2, LockKeyhole, Paperclip, Trash2 } from "lucide-react";
import { api } from "@/lib/api";
import { getApiErrorMessage } from "@/lib/api-error";
import { deleteNitDocument, listNitDocuments, listNitInstitutions, uploadNitDocument } from "@/lib/queries";

const acceptedFiles = ".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg";

export default function DocumentsPage() {
  const queryClient = useQueryClient();
  const fileInput = useRef<HTMLInputElement>(null);
  const documents = useQuery({ queryKey: ["nit-documents"], queryFn: listNitDocuments });
  const institutions = useQuery({ queryKey: ["nit-institutions"], queryFn: listNitInstitutions });
  const [name, setName] = useState("");
  const [type, setType] = useState("Contratos");
  const [institutionId, setInstitutionId] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [encrypt, setEncrypt] = useState(false);
  const [downloadError, setDownloadError] = useState<string | null>(null);

  const upload = useMutation({
    mutationFn: () => {
      const form = new FormData();
      form.append("name", name);
      form.append("type", type);
      form.append("institutionId", institutionId);
      form.append("encrypt", String(encrypt));
      form.append("file", file!);
      return uploadNitDocument(form);
    },
    onSuccess: async () => {
      setName(""); setFile(null); setEncrypt(false);
      if (fileInput.current) fileInput.current.value = "";
      await queryClient.invalidateQueries({ queryKey: ["nit-documents"] });
    }
  });
  const remove = useMutation({ mutationFn: deleteNitDocument, onSuccess: () => queryClient.invalidateQueries({ queryKey: ["nit-documents"] }) });

  async function download(id: string, fileName: string) {
    try {
      setDownloadError(null);
      const response = await api.get(`/api/nit/documents/${id}/download`, { responseType: "blob" });
      const url = URL.createObjectURL(response.data);
      const anchor = document.createElement("a");
      anchor.href = url; anchor.download = fileName; anchor.click();
      URL.revokeObjectURL(url);
    } catch (error) {
      setDownloadError(getApiErrorMessage(error, "Não foi possível baixar o documento."));
    }
  }

  return <div className="space-y-6">
    <div className="flex items-center gap-3"><div className="flex h-10 w-10 items-center justify-center rounded-md bg-emerald-50 text-emerald-700"><FileText size={20} /></div><div><h1 className="text-2xl font-semibold">Documentos</h1><p className="text-sm text-slate-500">Repositório seguro de arquivos do NIT.</p></div></div>

    <form onSubmit={(event) => { event.preventDefault(); upload.mutate(); }} className="form-panel space-y-4">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <label className="text-sm xl:col-span-2">Nome do documento<input className="input mt-1" required value={name} onChange={(event) => setName(event.target.value)} placeholder="Ex.: Contrato de licenciamento" /></label>
        <label className="text-sm">Tipo<select className="input mt-1" value={type} onChange={(event) => setType(event.target.value)}>{["Contratos", "Pareceres", "Certificados", "NDAs", "Relatórios", "Outros"].map((item) => <option key={item}>{item}</option>)}</select></label>
        <label className="text-sm">Instituição<select className="input mt-1" required value={institutionId} onChange={(event) => setInstitutionId(event.target.value)}><option value="">Selecione</option>{institutions.data?.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      </div>
      <div className="flex flex-col gap-3 rounded-md border border-dashed border-slate-300 bg-slate-50 p-3 lg:flex-row lg:items-center">
        <input ref={fileInput} id="nit-document-file" type="file" accept={acceptedFiles} className="sr-only" required onChange={(event) => setFile(event.target.files?.[0] ?? null)} />
        <label htmlFor="nit-document-file" className="btn-secondary cursor-pointer"><Paperclip size={16} />Selecionar arquivo</label>
        <div className="min-w-0 flex-1"><p className="truncate text-sm font-medium text-ink">{file?.name ?? "Nenhum arquivo selecionado"}</p><p className="text-xs text-slate-500">PDF, Word, Excel ou imagem. Tamanho máximo validado pelo servidor.</p></div>
        <label className={`flex h-10 cursor-pointer items-center gap-2 rounded-md border px-3 text-sm transition ${encrypt ? "border-emerald-300 bg-emerald-50 text-emerald-800" : "border-line bg-white text-slate-700"}`}><input type="checkbox" checked={encrypt} onChange={(event) => setEncrypt(event.target.checked)} /><LockKeyhole size={16} />Criptografar documento</label>
        <button className="btn-primary shrink-0" disabled={!file || upload.isPending}>{upload.isPending ? <Loader2 className="animate-spin" size={16} /> : <FileUp size={16} />}{upload.isPending ? "Enviando..." : "Enviar documento"}</button>
      </div>
      {upload.isError && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{getApiErrorMessage(upload.error, "Não foi possível enviar o documento.")}</p>}
    </form>

    {downloadError && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{downloadError}</p>}
    <div className="data-panel">
      <div className="flex items-center justify-between border-b border-line px-4 py-3"><div><h2 className="text-sm font-semibold text-ink">Arquivos cadastrados</h2><p className="text-xs text-slate-500">{documents.data?.length ?? 0} documentos disponíveis</p></div><FileCheck2 size={19} className="text-emerald-600" /></div>
      <div className="overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="bg-slate-50 text-left text-xs uppercase text-slate-500"><th className="p-3">Documento</th><th>Tipo</th><th>Instituição</th><th>Proteção</th><th>Upload</th><th>Tamanho</th><th className="pr-3 text-right">Ações</th></tr></thead><tbody>{documents.data?.map((item) => <tr key={item.id} className="border-t border-line hover:bg-slate-50/70"><td className="p-3 font-medium text-ink">{item.name}<span className="block max-w-sm truncate text-xs font-normal text-slate-500">{item.fileName}</span></td><td>{item.type}</td><td>{item.institutionName}</td><td><span className={`inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-xs font-medium ${item.isEncrypted ? "bg-emerald-50 text-emerald-700" : "bg-slate-100 text-slate-600"}`}>{item.isEncrypted && <LockKeyhole size={12} />}{item.isEncrypted ? "Criptografado" : "Normal"}</span></td><td>{new Date(item.uploadedAtUtc).toLocaleString("pt-BR")}</td><td>{formatFileSize(item.fileSize)}</td><td className="whitespace-nowrap pr-3 text-right"><button type="button" className="icon-button" title="Baixar documento" onClick={() => download(item.id, item.fileName)}><Download size={16} /></button><button type="button" className="icon-button ml-1 text-red-600" title="Excluir documento" onClick={() => remove.mutate(item.id)}><Trash2 size={16} /></button></td></tr>)}</tbody></table></div>
      {documents.isLoading && <p className="p-8 text-center text-sm text-slate-500">Carregando documentos...</p>}
      {!documents.isLoading && !documents.data?.length && <p className="p-8 text-center text-sm text-slate-500">Nenhum documento cadastrado.</p>}
    </div>
  </div>;
}

function formatFileSize(bytes: number) {
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}
