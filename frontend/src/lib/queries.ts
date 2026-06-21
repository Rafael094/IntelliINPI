import { api } from "@/lib/api";
import type {
  Client,
  ClientPayload,
  Deadline,
  DeadlinePayload,
  ImportResult,
  ImportStatus,
  InpiDeadline,
  InpiDeadlinePayload,
  InpiPatentResult,
  InpiSearchResponse,
  InpiTrademarkResult,
  IPAsset,
  IPAssetPayload,
  MonitoredTrademark,
  MonitoredPatent,
  MonitoringCheckResult,
  MonitoringEvent,
  NitAuditLog,
  NitContract,
  NitContractPayload,
  NitDashboardOverview,
  NitInvention,
  NitInventionPayload,
  NitUniversity,
  NitUniversityPayload,
  NitInstitution,
  NitInstitutionPayload,
  NitResearcher,
  NitResearcherPayload,
  NitCompany,
  NitCompanyPayload,
  NitPortfolioInvention,
  NitPortfolioInventionPayload,
  NitOperationalContract,
  NitOperationalContractPayload,
  NitRoyalty,
  NitRoyaltyPayload,
  NitRoyaltySummary,
  NitTransferOpportunity,
  NitTransferOpportunityPayload,
  NitDocument,
  OperationalDashboard,
  OperationalDeadline,
  OperationalHome,
  PatentMonitoringEvent,
  PagedResult,
  RegisterAndMonitorRequest,
  RegisterAndMonitorResult,
  RpiHistoryRunRequest,
  RpiHistoryStatus,
  TrademarkAvailabilityAnalysis,
  TrademarkAvailabilityRequest,
  TrademarkDetail,
  TrademarkSearchItem,
  TrademarkSearchParams
} from "@/lib/types";

export async function listNitInstitutions() { return (await api.get<NitInstitution[]>("/api/nit/institutions")).data; }
export async function saveNitInstitution(payload: NitInstitutionPayload, id?: string) { return (await (id ? api.put<NitInstitution>(`/api/nit/institutions/${id}`, payload) : api.post<NitInstitution>("/api/nit/institutions", payload))).data; }
export async function deleteNitInstitution(id: string) { await api.delete(`/api/nit/institutions/${id}`); }
export async function listNitResearchers(params?: { search?: string; institutionId?: string; technologyArea?: string; page?: number; pageSize?: number }) { return (await api.get<PagedResult<NitResearcher>>("/api/nit/researchers", { params })).data; }
export async function saveNitResearcher(payload: NitResearcherPayload, id?: string) { return (await (id ? api.put<NitResearcher>(`/api/nit/researchers/${id}`, payload) : api.post<NitResearcher>("/api/nit/researchers", payload))).data; }
export async function deleteNitResearcher(id: string) { await api.delete(`/api/nit/researchers/${id}`); }
export async function listNitCompanies(search?: string) { return (await api.get<NitCompany[]>("/api/nit/companies", { params: { search } })).data; }
export async function saveNitCompany(payload: NitCompanyPayload, id?: string) { return (await (id ? api.put<NitCompany>(`/api/nit/companies/${id}`, payload) : api.post<NitCompany>("/api/nit/companies", payload))).data; }
export async function deleteNitCompany(id: string) { await api.delete(`/api/nit/companies/${id}`); }
export async function listNitPortfolioInventions() { return (await api.get<NitPortfolioInvention[]>("/api/nit/inventions/portfolio")).data; }
export async function saveNitPortfolioInvention(payload: NitPortfolioInventionPayload, id?: string) { return (await (id ? api.put<NitPortfolioInvention>(`/api/nit/inventions/${id}/portfolio`, payload) : api.post<NitPortfolioInvention>("/api/nit/inventions/portfolio", payload))).data; }
export async function listNitOperationalContracts() { return (await api.get<NitOperationalContract[]>("/api/nit/contracts/operational")).data; }
export async function saveNitOperationalContract(payload: NitOperationalContractPayload, id?: string) { return (await (id ? api.put<NitOperationalContract>(`/api/nit/contracts/${id}/operational`, payload) : api.post<NitOperationalContract>("/api/nit/contracts/operational", payload))).data; }
export async function listNitRoyalties() { return (await api.get<NitRoyalty[]>("/api/nit/royalties")).data; }
export async function getNitRoyaltySummary() { return (await api.get<NitRoyaltySummary>("/api/nit/royalties/summary")).data; }
export async function saveNitRoyalty(payload: NitRoyaltyPayload, id?: string) { return (await (id ? api.put<NitRoyalty>(`/api/nit/royalties/${id}`, payload) : api.post<NitRoyalty>("/api/nit/royalties", payload))).data; }
export async function deleteNitRoyalty(id: string) { await api.delete(`/api/nit/royalties/${id}`); }
export async function listNitTransferPipeline() { return (await api.get<NitTransferOpportunity[]>("/api/nit/transfer-pipeline")).data; }
export async function saveNitTransferOpportunity(payload: NitTransferOpportunityPayload, id?: string) { return (await (id ? api.put<NitTransferOpportunity>(`/api/nit/transfer-pipeline/${id}`, payload) : api.post<NitTransferOpportunity>("/api/nit/transfer-pipeline", payload))).data; }
export async function moveNitTransferOpportunity(id: string, stage: string, sortOrder = 0) { return (await api.patch<NitTransferOpportunity>(`/api/nit/transfer-pipeline/${id}/stage`, { stage, sortOrder })).data; }
export async function listNitDocuments() { return (await api.get<NitDocument[]>("/api/nit/documents")).data; }
export async function uploadNitDocument(form: FormData) { return (await api.post<NitDocument>("/api/nit/documents", form)).data; }
export async function deleteNitDocument(id: string) { await api.delete(`/api/nit/documents/${id}`); }

export async function getOperationalHome() {
  const response = await api.get<OperationalHome>("/api/operational-home");
  return response.data;
}

export async function listClients() {
  const response = await api.get<Client[]>("/api/clients");
  return response.data;
}

export async function getClient(id: string) {
  const response = await api.get<Client>(`/api/clients/${id}`);
  return response.data;
}

export async function createClient(payload: ClientPayload) {
  const response = await api.post<Client>("/api/clients", payload);
  return response.data;
}

export async function updateClient(id: string, payload: ClientPayload) {
  const response = await api.put<Client>(`/api/clients/${id}`, payload);
  return response.data;
}

export async function deleteClient(id: string) {
  await api.delete(`/api/clients/${id}`);
}

export async function listDeadlines() {
  const response = await api.get<Deadline[]>("/api/deadlines");
  return response.data;
}

export async function listOperationalDeadlines(params?: { daysAhead?: number; includeManualReview?: boolean }) {
  const response = await api.get<OperationalDeadline[]>("/api/deadlines/operational", { params });
  return response.data;
}

export async function getDeadline(id: string) {
  const response = await api.get<Deadline>(`/api/deadlines/${id}`);
  return response.data;
}

export async function createDeadline(payload: DeadlinePayload) {
  const response = await api.post<Deadline>("/api/deadlines", payload);
  return response.data;
}

export async function updateDeadline(id: string, payload: DeadlinePayload) {
  const response = await api.put<Deadline>(`/api/deadlines/${id}`, payload);
  return response.data;
}

export async function deleteDeadline(id: string) {
  await api.delete(`/api/deadlines/${id}`);
}

export async function listIPAssets(params?: { type?: string; query?: string }) {
  const response = await api.get<IPAsset[]>("/api/ip-assets", { params });
  return response.data;
}

export async function getIPAsset(id: string) {
  const response = await api.get<IPAsset>(`/api/ip-assets/${id}`);
  return response.data;
}

export async function createIPAsset(payload: IPAssetPayload) {
  const response = await api.post<IPAsset>("/api/ip-assets", payload);
  return response.data;
}

export async function registerAndMonitorIPAsset(payload: RegisterAndMonitorRequest) {
  const response = await api.post<RegisterAndMonitorResult>("/api/ip-assets/register-and-monitor", payload);
  return response.data;
}

export async function listInpiDeadlines(params?: { isInternal?: boolean; daysAhead?: number }) {
  const response = await api.get<InpiDeadline[]>("/api/inpi-deadlines", { params });
  return response.data;
}

export async function createInpiDeadline(payload: InpiDeadlinePayload) {
  const response = await api.post<InpiDeadline>("/api/inpi-deadlines", payload);
  return response.data;
}

export async function updateInpiDeadline(id: string, payload: InpiDeadlinePayload) {
  const response = await api.put<InpiDeadline>(`/api/inpi-deadlines/${id}`, payload);
  return response.data;
}

export async function deleteInpiDeadline(id: string) {
  await api.delete(`/api/inpi-deadlines/${id}`);
}

export async function getOperationalDashboard() {
  const response = await api.get<OperationalDashboard>("/api/dashboard/operational");
  return response.data;
}

export async function getNitDashboardOverview() {
  const response = await api.get<NitDashboardOverview>("/api/nit/dashboard/overview");
  return response.data;
}

export async function listNitInventions() {
  const response = await api.get<NitInvention[]>("/api/nit/inventions");
  return response.data;
}

export async function listNitUniversities() {
  const response = await api.get<NitUniversity[]>("/api/nit/universities");
  return response.data;
}

export async function getNitUniversity(id: string) {
  const response = await api.get<NitUniversity>(`/api/nit/universities/${id}`);
  return response.data;
}

export async function createNitUniversity(payload: NitUniversityPayload) {
  const response = await api.post<NitUniversity>("/api/nit/universities", payload);
  return response.data;
}

export async function updateNitUniversity(id: string, payload: NitUniversityPayload) {
  const response = await api.put<NitUniversity>(`/api/nit/universities/${id}`, payload);
  return response.data;
}

export async function deleteNitUniversity(id: string) {
  await api.delete(`/api/nit/universities/${id}`);
}

export async function getNitInvention(id: string) {
  const response = await api.get<NitInvention>(`/api/nit/inventions/${id}`);
  return response.data;
}

export async function createNitInvention(payload: NitInventionPayload) {
  const response = await api.post<NitInvention>("/api/nit/inventions", payload);
  return response.data;
}

export async function updateNitInvention(id: string, payload: NitInventionPayload) {
  const response = await api.put<NitInvention>(`/api/nit/inventions/${id}`, payload);
  return response.data;
}

export async function deleteNitInvention(id: string) {
  await api.delete(`/api/nit/inventions/${id}`);
}

export async function listNitContracts() {
  const response = await api.get<NitContract[]>("/api/nit/contracts");
  return response.data;
}

export async function getNitContract(id: string) {
  const response = await api.get<NitContract>(`/api/nit/contracts/${id}`);
  return response.data;
}

export async function createNitContract(payload: NitContractPayload) {
  const response = await api.post<NitContract>("/api/nit/contracts", payload);
  return response.data;
}

export async function updateNitContract(id: string, payload: NitContractPayload) {
  const response = await api.put<NitContract>(`/api/nit/contracts/${id}`, payload);
  return response.data;
}

export async function listNitAuditLogs(params?: { entityName?: string; action?: string; startAtUtc?: string; endAtUtc?: string }) {
  const response = await api.get<NitAuditLog[]>("/api/nit/audit-logs", { params });
  return response.data;
}

export async function searchTrademarks(params: TrademarkSearchParams) {
  const response = await api.get<PagedResult<TrademarkSearchItem>>("/api/trademarks/search", {
    params
  });

  return response.data;
}

export async function getTrademarkDetail(processNumber: string) {
  const response = await api.get<TrademarkDetail>(`/api/trademarks/${processNumber}/detail`);
  return response.data;
}

export async function searchInpiTrademarksBasic(params: { query?: string; niceClass?: string; exact?: boolean; page: number; pageSize: number }) {
  const response = await api.get<InpiSearchResponse<InpiTrademarkResult>>("/api/inpi/search/trademarks/basic", { params });
  return response.data;
}

export async function searchInpiTrademarksAdvanced(params: { trademarkName?: string; niceClass?: string; exact?: boolean; liveOnly?: boolean; presentation?: string; nature?: string; page: number; pageSize: number }) {
  const response = await api.get<InpiSearchResponse<InpiTrademarkResult>>("/api/inpi/search/trademarks/advanced", { params });
  return response.data;
}

export type InpiPatentBasicSearchParams = {
  query?: string;
  processNumber?: string;
  gruNumber?: string;
  protocolNumber?: string;
  searchMode?: "todasPalavras" | "expExata" | "qualquerPalavra" | "aproximacao";
  searchField?: "Titulo" | "Resumo" | "NomeDepositante" | "NomeInventor" | "CpfCnpjDepositante";
  page: number;
  pageSize: number;
};

export type InpiPatentAdvancedSearchParams = {
  processNumber?: string;
  priorityNumber?: string;
  pctNumber?: string;
  startDate?: string;
  endDate?: string;
  priorityStartDate?: string;
  priorityEndDate?: string;
  pctDepositStartDate?: string;
  pctDepositEndDate?: string;
  pctPublicationStartDate?: string;
  pctPublicationEndDate?: string;
  ipcClass?: string;
  ipcKeyword?: string;
  title?: string;
  abstract?: string;
  applicant?: string;
  applicantDocument?: string;
  inventor?: string;
  grantedOnly?: boolean;
  page: number;
  pageSize: number;
};

export async function searchInpiPatentsBasic(params: InpiPatentBasicSearchParams) {
  const response = await api.get<InpiSearchResponse<InpiPatentResult>>("/api/inpi/search/patents/basic", { params });
  return response.data;
}

export async function searchInpiPatentsAdvanced(params: InpiPatentAdvancedSearchParams) {
  const response = await api.get<InpiSearchResponse<InpiPatentResult>>("/api/inpi/search/patents/advanced", { params });
  return response.data;
}

export async function analyzeTrademarkAvailability(request: TrademarkAvailabilityRequest) {
  const response = await api.post<TrademarkAvailabilityAnalysis>("/api/trademarks/availability-analysis", request);
  return response.data;
}

export async function monitorTrademark(trademarkId: string) {
  const response = await api.post<string>("/api/monitoring/trademarks", { trademarkId });
  return response.data;
}

export async function listMonitoredTrademarks() {
  const response = await api.get<MonitoredTrademark[]>("/api/monitoring/trademarks");
  return response.data;
}

export async function checkMonitoringNow() {
  const response = await api.post<MonitoringCheckResult>("/api/monitoring/check");
  return response.data;
}

export async function removeMonitoredTrademark(id: string) {
  await api.delete(`/api/monitoring/trademarks/${id}`);
}

export async function monitorPatent(inpiProcessNumber: string) {
  const response = await api.post<string>("/api/monitoring/patents", { inpiProcessNumber });
  return response.data;
}

export async function listMonitoredPatents() {
  const response = await api.get<MonitoredPatent[]>("/api/monitoring/patents");
  return response.data;
}

export async function checkPatentMonitoringNow() {
  const response = await api.post<MonitoringCheckResult>("/api/monitoring/patents/check");
  return response.data;
}

export async function removeMonitoredPatent(id: string) {
  await api.delete(`/api/monitoring/patents/${id}`);
}

export async function listPatentMonitoringEvents() {
  const response = await api.get<PatentMonitoringEvent[]>("/api/monitoring/patents/events");
  return response.data;
}

export async function listMonitoringEvents(unreadOnly: boolean) {
  const url = unreadOnly ? "/api/monitoring/events/unread" : "/api/monitoring/events";
  const response = await api.get<MonitoringEvent[]>(url);
  return response.data;
}

export async function markMonitoringEventAsRead(id: string) {
  await api.post(`/api/monitoring/events/${id}/read`);
}

export async function getImportStatus() {
  const response = await api.get<ImportStatus | null>("/api/import/inpi/status");
  return response.data;
}

export async function importOpenDataTrademarks() {
  const response = await api.post<ImportResult>("/api/import/inpi/trademarks");
  return response.data;
}

export async function importRpiTrademarks(rpiNumber: number | null) {
  const response = await api.post<ImportResult>("/api/import/inpi/rpi/trademarks", { rpiNumber });
  return response.data;
}

export async function getRpiHistoryStatus() {
  const response = await api.get<RpiHistoryStatus | null>("/api/import/inpi/rpi/history/status");
  return response.data;
}

export async function runRpiHistory(request: RpiHistoryRunRequest) {
  const response = await api.post<RpiHistoryStatus>("/api/import/inpi/rpi/history/run", request);
  return response.data;
}

export async function resumeRpiHistory() {
  const response = await api.post<RpiHistoryStatus>("/api/import/inpi/rpi/history/resume");
  return response.data;
}

export async function stopRpiHistory() {
  const response = await api.post<RpiHistoryStatus>("/api/import/inpi/rpi/history/stop");
  return response.data;
}
