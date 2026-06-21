export const inventionStatuses = [
  { value: "Draft", label: "Rascunho" },
  { value: "SubmittedToNit", label: "Submetida ao NIT" },
  { value: "UnderReview", label: "Em análise" },
  { value: "FiledAtInpi", label: "Depositada no INPI" },
  { value: "Granted", label: "Concedida" },
  { value: "Licensed", label: "Licenciada" },
  { value: "Archived", label: "Arquivada" }
];

export const royaltyModels = [
  { value: "FixedPercentage", label: "Percentual fixo" },
  { value: "MinimumGuarantee", label: "Garantia mínima" },
  { value: "EquityParticipation", label: "Participação societária" },
  { value: "Hybrid", label: "Híbrido" }
];

export function formatInventionStatus(value: string) {
  return inventionStatuses.find((item) => item.value === value)?.label ?? value;
}

export function formatRoyaltyModel(value: string) {
  return royaltyModels.find((item) => item.value === value)?.label ?? value;
}

export function formatContractStatus(value: string) {
  const statuses: Record<string, string> = {
    Draft: "Rascunho",
    Active: "Ativo",
    Signed: "Assinado",
    Cancelled: "Cancelado",
    Finished: "Encerrado"
  };

  return statuses[value] ?? value;
}

export function formatMaturityLevel(value: string) {
  if (value === "Intermediario") {
    return "Intermediário";
  }

  return value;
}
