"use client";

import { useParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2 } from "lucide-react";
import { getApiErrorMessage } from "@/lib/api-error";
import { getNitInvention, updateNitInvention } from "@/lib/queries";
import { InventionForm } from "../invention-form";
import type { NitInventionPayload } from "@/lib/types";

export default function EditNitInventionPage() {
  const params = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const inventionQuery = useQuery({
    queryKey: ["nit-invention", params.id],
    queryFn: () => getNitInvention(params.id)
  });
  const mutation = useMutation({
    mutationFn: (payload: NitInventionPayload) => updateNitInvention(params.id, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["nit-invention", params.id] });
      void queryClient.invalidateQueries({ queryKey: ["nit-inventions"] });
    }
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Editar invenção</h1>
        <p className="mt-1 text-sm text-slate-500">Atualize dados técnicos e status da invenção.</p>
      </div>
      {inventionQuery.isLoading ? <div className="flex min-h-64 items-center justify-center gap-2 text-sm text-slate-500"><Loader2 className="animate-spin" size={18} />Carregando invenção...</div> : null}
      {inventionQuery.isError ? <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">Não foi possível carregar a invenção.</div> : null}
      {mutation.isError ? (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {getApiErrorMessage(mutation.error, "Não foi possível salvar a invenção.")}
        </div>
      ) : null}
      {inventionQuery.data ? <InventionForm initialValue={inventionQuery.data} onSubmit={(payload) => mutation.mutate(payload)} isSubmitting={mutation.isPending} /> : null}
    </div>
  );
}
