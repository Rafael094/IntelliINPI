"use client";

import { useRouter } from "next/navigation";
import { useMutation } from "@tanstack/react-query";
import { getApiErrorMessage } from "@/lib/api-error";
import { createNitInvention } from "@/lib/queries";
import { InventionForm } from "../invention-form";

export default function NewNitInventionPage() {
  const router = useRouter();
  const mutation = useMutation({
    mutationFn: createNitInvention,
    onSuccess: (invention) => router.push(`/nit/inventions/${invention.id}`)
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-ink">Nova invenção</h1>
        <p className="mt-1 text-sm text-slate-500">Cadastre uma invenção para acompanhamento do NIT.</p>
      </div>
      {mutation.isError ? (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {getApiErrorMessage(mutation.error, "Não foi possível salvar a invenção.")}
        </div>
      ) : null}
      <InventionForm onSubmit={(payload) => mutation.mutate(payload)} isSubmitting={mutation.isPending} />
    </div>
  );
}
