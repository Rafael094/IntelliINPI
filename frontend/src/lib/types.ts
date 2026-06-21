export type User = {
  id: string;
  email: string;
  role: string;
};

export type LoginResponse = {
  accessToken: string;
  user: User;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
};

export type TrademarkSearchItem = {
  id: string;
  processNumber: string;
  name: string;
  status: string | null;
  niceClasses: string[];
  owners: string[];
  filingDate: string | null;
  registrationDate: string | null;
  lastDispatchDate: string | null;
  inpiDetailUrl: string | null;
};

export type TrademarkDetail = {
  id: string;
  processNumber: string;
  name: string;
  status: string | null;
  presentation: string | null;
  nature: string | null;
  legalRepresentative: string | null;
  filingDate: string | null;
  registrationDate: string | null;
  expirationDate: string | null;
  renewalWindow: {
    ordinaryStart: string | null;
    ordinaryEnd: string | null;
    extraordinaryStart: string | null;
    extraordinaryEnd: string | null;
  };
  inpiDetailUrl: string | null;
  logoUrl: string | null;
  owners: string[];
  niceClasses: Array<{
    code: string;
    classNumber: number;
    specification: string | null;
  }>;
  viennaClasses: Array<{
    edition: string;
    code: string;
    description: string | null;
  }>;
  petitions: Array<{
    protocol: string;
    filedAt: string | null;
    serviceCode: string | null;
    clientName: string | null;
    delivery: string | null;
    deliveryDate: string | null;
  }>;
  dispatches: Array<{
    rpiNumber: number | null;
    publishedAt: string;
    code: string;
    description: string;
  }>;
};

export type TrademarkSearchParams = {
  query?: string;
  niceClass?: string;
  status?: string;
  owner?: string;
  page: number;
  pageSize: number;
};

export type TrademarkAvailabilityRequest = {
  proposedName: string;
  activityDescription?: string;
};

export type NiceClassSuggestion = {
  code: string;
  title: string;
  reason: string;
  matchedKeywords: string[];
};

export type TrademarkConflict = {
  id: string;
  processNumber: string;
  name: string;
  status: string | null;
  niceClasses: string[];
  owners: string[];
  lastDispatchDate: string | null;
  similarityScore: number;
  conflictReason: string;
};

export type WebPresenceCheck = {
  source: string;
  query: string;
  url: string;
  status: string;
  notes: string;
};

export type ExternalBrandPresenceResult = {
  source: string;
  query: string;
  title: string;
  url: string;
  snippet: string | null;
  score: number | null;
  category: string;
};

export type TrademarkAvailabilityAnalysis = {
  proposedName: string;
  normalizedBrand: string;
  riskLevel: string;
  summary: string;
  conflictSearchSource: string;
  conflictSearchWarning: string | null;
  suggestedClasses: NiceClassSuggestion[];
  localConflicts: TrademarkConflict[];
  externalResults: ExternalBrandPresenceResult[];
  webPresenceChecks: WebPresenceCheck[];
};

export type MonitoredTrademark = {
  id: string;
  trademarkId: string;
  processNumber: string;
  inpiDetailUrl: string | null;
  name: string;
  notes: string | null;
  isActive: boolean;
  createdAtUtc: string;
  lastCheckedAtUtc: string | null;
  lastKnownDispatchId: string | null;
  lastKnownDispatchCode: string | null;
  lastKnownDispatchDate: string | null;
  hasPendingChanges: boolean;
  recentDispatches: Array<{
    id: string;
    rpiNumber: number | null;
    code: string;
    description: string;
    publishedAt: string;
  }>;
};

export type MonitoringCheckResult = {
  checkedCount: number;
  changedCount: number;
  eventsCreated: number;
};

export type MonitoringEvent = {
  id: string;
  processNumber: string;
  trademarkName: string;
  eventType: string;
  previousDispatchCode: string | null;
  currentDispatchCode: string | null;
  previousDispatchDate: string | null;
  currentDispatchDate: string | null;
  createdAtUtc: string;
  isRead: boolean;
};

export type ImportStatus = {
  lastJobId: string | null;
  status: string;
  source: string | null;
  startedAtUtc: string | null;
  finishedAtUtc: string | null;
};

export type ImportResult = {
  jobId: string;
  status: string;
  downloadedFiles: number;
  importedRows: number;
  failedRows: number;
  errorMessage: string | null;
};

export type RpiHistoryStatus = {
  runId: string;
  status: string;
  startRpi: number;
  endRpi: number;
  currentRpi: number;
  totalRpis: number;
  successfulRpis: number;
  failedRpis: number;
  totalDispatchesImported: number;
  percentage: number;
  startedAtUtc: string;
  finishedAtUtc: string | null;
  errorMessage: string | null;
};

export type RpiHistoryRunRequest = {
  startYear?: number;
  startRpi?: number;
  endRpi: number;
  batchSize: number;
  delaySecondsBetweenBatches: number;
};

export type OperationalDashboardEvent = {
  id: string;
  processNumber: string;
  trademarkName: string;
  previousDispatchCode: string | null;
  currentDispatchCode: string | null;
  currentDispatchDate: string | null;
  createdAtUtc: string;
  isRead: boolean;
};

export type OperationalDashboardPendingTrademark = {
  id: string;
  trademarkId: string;
  processNumber: string;
  trademarkName: string;
  lastKnownDispatchCode: string | null;
  lastKnownDispatchDate: string | null;
  lastCheckedAtUtc: string | null;
};

export type OperationalDashboard = {
  totalMonitoredIPAssets: number;
  totalMonitoredTrademarks: number;
  totalActiveMonitoredTrademarks: number;
  totalMonitoredPatents: number;
  totalActiveMonitoredPatents: number;
  totalPendingChanges: number;
  totalUnreadEvents: number;
  lastMonitoringCheckAtUtc: string | null;
  upcomingInpiDeadlines: Array<{
    id: string;
    ipAssetId: string;
    ipAssetType: string;
    ipAssetTitle: string;
    inpiProcessNumber: string | null;
    type: string;
    dueDate: string | null;
    status: string;
    isInternal: boolean;
    notes: string | null;
  }>;
  upcomingInternalDeadlines: Array<{
    id: string;
    ipAssetId: string;
    ipAssetType: string;
    ipAssetTitle: string;
    inpiProcessNumber: string | null;
    type: string;
    dueDate: string | null;
    status: string;
    isInternal: boolean;
    notes: string | null;
  }>;
  latestDispatches: Array<{
    assetType: string;
    processNumber: string;
    title: string;
    dispatchCode: string;
    dispatchDate: string | null;
    rpiNumber: number | null;
  }>;
  lastImportedRpiNumber: number | null;
  lastRpiImportStatus: string | null;
  lastRpiImportDateUtc: string | null;
  historicalImportStatus: string | null;
  historicalImportCurrentRpi: number | null;
  historicalImportPercentage: number | null;
  inpiSyncFailures: string[];
  recentMonitoringEvents: OperationalDashboardEvent[];
  monitoredTrademarksWithPendingChanges: OperationalDashboardPendingTrademark[];
};

export type Client = {
  id: string;
  name: string;
  documentNumber: string | null;
  email: string | null;
  phone: string | null;
  notes: string | null;
  createdAtUtc: string;
  isActive: boolean;
};

export type ClientPayload = {
  name: string;
  documentNumber?: string | null;
  email?: string | null;
  phone?: string | null;
  notes?: string | null;
};

export type Deadline = {
  id: string;
  title: string;
  description: string | null;
  dueDate: string;
  status: string;
  type: string;
  clientId: string | null;
  clientName: string | null;
  trademarkId: string | null;
  trademarkName: string | null;
  trademarkProcessNumber: string | null;
  inventionId: string | null;
  inventionTitle: string | null;
  createdAtUtc: string;
  isActive: boolean;
};

export type OperationalDeadline = {
  id: string;
  source: string;
  scope: string;
  type: string;
  title: string;
  description: string | null;
  dueDate: string | null;
  daysUntilDue: number | null;
  status: string;
  statusLabel: string;
  trademarkId: string | null;
  trademarkName: string | null;
  trademarkProcessNumber: string | null;
  clientId: string | null;
  clientName: string | null;
  inventionId: string | null;
  inventionTitle: string | null;
  ipAssetId: string | null;
  ipAssetTitle: string | null;
  requiresManualReview: boolean;
};

export type DeadlinePayload = {
  title: string;
  description?: string | null;
  dueDate: string;
  status: string;
  type: string;
  clientId?: string | null;
  trademarkId?: string | null;
  inventionId?: string | null;
};

export type OperationalHome = {
  pendingDeadlinesToday: number;
  upcomingDeadlines: Deadline[];
  unreadMonitoringEvents: number;
  monitoredTrademarksWithChanges: Array<{
    id: string;
    trademarkId: string;
    processNumber: string;
    trademarkName: string;
    lastKnownDispatchCode: string | null;
    lastKnownDispatchDate: string | null;
  }>;
  lastRpiImportStatus: string | null;
  lastRpiNumber: number | null;
  recentEvents: Array<{
    id: string;
    processNumber: string;
    trademarkName: string;
    previousDispatchCode: string | null;
    currentDispatchCode: string | null;
    currentDispatchDate: string | null;
    createdAtUtc: string;
    isRead: boolean;
  }>;
};

export type NitDashboardOverview = {
  totalInventions: number;
  totalDrafts: number;
  totalSubmittedToNit: number;
  totalFiledAtInpi: number;
  totalGranted: number;
  totalLicensed: number;
  totalContracts: number;
  totalRoyalties: number;
  maturityLevel: string;
  totalInstitutions: number;
  totalResearchers: number;
  totalCompanies: number;
  totalLicensedTechnologies: number;
  inventionsByStatus: NitChartItem[];
  contractsByType: NitChartItem[];
  royaltiesByPeriod: NitChartItem[];
  transferPipeline: NitChartItem[];
};

export type NitChartItem = { label: string; value: number };

export type NitUniversity = {
  id: string;
  name: string;
  cnpj: string | null;
  tier: string;
  createdAtUtc: string;
  isActive: boolean;
};

export type NitUniversityPayload = {
  name: string;
  cnpj?: string | null;
  tier: string;
};

export type NitInvention = {
  id: string;
  universityId: string;
  universityName: string;
  title: string;
  summary: string;
  inventors: string;
  depositDate: string | null;
  status: string;
  patentNumber: string | null;
  inpiProcessNumber: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  isActive: boolean;
};

export type NitInventionPayload = {
  universityId?: string | null;
  title: string;
  summary: string;
  inventors: string;
  depositDate?: string | null;
  status?: string;
  patentNumber?: string | null;
  inpiProcessNumber?: string | null;
};

export type NitContract = {
  id: string;
  inventionId: string;
  inventionTitle: string;
  universityId: string;
  companyName: string;
  cnpj: string | null;
  royaltyModel: string;
  royaltyValue: number | null;
  minimumGuarantee: number | null;
  signedAt: string | null;
  status: string;
  createdAtUtc: string;
};

export type NitContractPayload = {
  inventionId?: string;
  companyName: string;
  cnpj?: string | null;
  royaltyModel: string;
  royaltyValue?: number | null;
  minimumGuarantee?: number | null;
  signedAt?: string | null;
  status: string;
};

export type NitAuditLog = {
  id: string;
  userId: string;
  userEmail: string;
  universityId: string | null;
  universityName: string | null;
  module: string;
  entityName: string;
  entityId: string;
  action: string;
  previousValue: string | null;
  newValue: string | null;
  ipAddress: string | null;
  createdAtUtc: string;
};

export type NitInstitution = { id: string; name: string; tradeName: string | null; cnpj: string | null; tier: string; type: string; website: string | null; email: string | null; phone: string | null; contactName: string | null; status: string; createdAtUtc: string; isActive: boolean };
export type NitInstitutionPayload = Omit<NitInstitution, "id" | "createdAtUtc" | "isActive">;
export type NitResearcher = { id: string; institutionId: string; institutionName: string; name: string; cpf: string | null; email: string | null; phone: string | null; department: string | null; position: string | null; lattesUrl: string | null; orcid: string | null; specialties: string | null; technologyAreas: string | null; inventionsCount: number; createdAtUtc: string };
export type NitResearcherPayload = Omit<NitResearcher, "id" | "institutionName" | "inventionsCount" | "createdAtUtc">;
export type NitCompany = { id: string; institutionId: string | null; institutionName: string | null; legalName: string; tradeName: string | null; cnpj: string | null; segment: string | null; size: string | null; contactName: string | null; email: string | null; phone: string | null; website: string | null; notes: string | null; contractsCount: number; createdAtUtc: string };
export type NitCompanyPayload = Omit<NitCompany, "id" | "institutionName" | "contractsCount" | "createdAtUtc">;
export type NitPortfolioInvention = { id: string; institutionId: string; institutionName: string; title: string; summary: string; executiveSummary: string | null; technicalDescription: string | null; technologyArea: string | null; trl: number | null; commercialPotential: string | null; targetMarket: string | null; protectionStatus: string | null; creationDate: string | null; responsible: string | null; status: string; researcherIds: string[]; researchers: string[]; createdAtUtc: string; updatedAtUtc: string | null };
export type NitPortfolioInventionPayload = Omit<NitPortfolioInvention, "id" | "institutionName" | "researchers" | "createdAtUtc" | "updatedAtUtc">;
export type NitOperationalContract = { id: string; number: string | null; institutionId: string; institutionName: string; companyId: string; companyName: string; inventionId: string; inventionTitle: string; type: string; startDate: string | null; endDate: string | null; term: string | null; automaticRenewal: boolean; royaltyPercentage: number | null; minimumGuarantee: number | null; fixedValue: number | null; status: string; createdAtUtc: string };
export type NitOperationalContractPayload = Omit<NitOperationalContract, "id" | "institutionName" | "companyName" | "inventionTitle" | "createdAtUtc">;
export type NitRoyalty = { id: string; contractId: string; contractNumber: string; inventionTitle: string; competence: string; amountReceived: number; notes: string | null; receivedAt: string };
export type NitRoyaltyPayload = { contractId: string; competence: string; amountReceived: number; notes: string | null; receivedAt: string };
export type NitRoyaltySummary = { totalReceived: number; receivedThisYear: number; receivedThisMonth: number; topContracts: Array<{ contractId: string; contractNumber: string; total: number }> };
export type NitTransferOpportunity = { id: string; inventionId: string; inventionTitle: string; institutionId: string; institutionName: string; companyId: string | null; companyName: string | null; stage: string; notes: string | null; sortOrder: number; updatedAtUtc: string };
export type NitTransferOpportunityPayload = { inventionId: string; companyId: string | null; stage: string; notes: string | null };
export type NitDocument = { id: string; name: string; type: string; institutionId: string; institutionName: string; inventionId: string | null; inventionTitle: string | null; contractId: string | null; fileName: string; contentType: string; fileSize: number; isEncrypted: boolean; encryptionAlgorithm: string | null; uploadedAtUtc: string; uploadedBy: string };

export type IPAsset = {
  id: string;
  type: string;
  inpiProcessNumber: string | null;
  title: string;
  ownerName: string | null;
  status: string;
  filingDate: string | null;
  grantDate: string | null;
  expirationDate: string | null;
  internalDeadline: string | null;
  clientId: string | null;
  clientName: string | null;
  universityId: string | null;
  universityName: string | null;
  isMonitored: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  isActive: boolean;
  source: string | null;
  warning: string | null;
};

export type IPAssetPayload = {
  type: string;
  inpiProcessNumber?: string | null;
  title: string;
  ownerName?: string | null;
  status?: string | null;
  filingDate?: string | null;
  grantDate?: string | null;
  expirationDate?: string | null;
  internalDeadline?: string | null;
  clientId?: string | null;
  universityId?: string | null;
  isMonitored: boolean;
};

export type RegisterAndMonitorRequest = {
  type: string;
  query: string;
  clientId?: string | null;
  universityId?: string | null;
};

export type IPAssetCandidate = {
  type: string;
  inpiProcessNumber: string | null;
  title: string;
  ownerName: string | null;
  status: string | null;
  filingDate: string | null;
  grantDate: string | null;
  localId: string | null;
};

export type RegisterAndMonitorResult = {
  status: string;
  type: string;
  query: string;
  ipAssetId: string | null;
  trademarkId: string | null;
  patentId: string | null;
  monitoringId: string | null;
  isMonitored: boolean;
  source: string | null;
  warning: string | null;
  candidates: IPAssetCandidate[];
};

export type InpiSearchResponse<T> = {
  source: string;
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  warning: string | null;
};

export type InpiTrademarkResult = {
  localId: string | null;
  processNumber: string;
  name: string;
  status: string | null;
  niceClasses: string[];
  owners: string[];
  filingDate: string | null;
  registrationDate: string | null;
  lastDispatchDate: string | null;
  inpiDetailUrl: string | null;
};

export type InpiPatentResult = {
  localId: string | null;
  inpiProcessNumber: string | null;
  title: string;
  abstract: string | null;
  applicants: string[];
  inventors: string[];
  ipcClass: string | null;
  filingDate: string | null;
  publicationDate: string | null;
  grantDate: string | null;
  status: string | null;
};

export type MonitoredPatent = {
  id: string;
  patentId: string;
  inpiProcessNumber: string;
  title: string;
  notes: string | null;
  isActive: boolean;
  createdAtUtc: string;
  lastCheckedAtUtc: string | null;
  lastKnownDispatchId: string | null;
  lastKnownDispatchCode: string | null;
  lastKnownDispatchDate: string | null;
  hasPendingChanges: boolean;
};

export type PatentMonitoringEvent = {
  id: string;
  monitoredPatentId: string;
  patentId: string;
  inpiProcessNumber: string;
  title: string;
  eventType: string;
  previousDispatchCode: string | null;
  currentDispatchCode: string | null;
  previousDispatchDate: string | null;
  currentDispatchDate: string | null;
  createdAtUtc: string;
  isRead: boolean;
};

export type InpiDeadline = {
  id: string;
  ipAssetId: string;
  ipAssetType: string;
  ipAssetTitle: string;
  inpiProcessNumber: string | null;
  type: string;
  source: string;
  sourceRpiNumber: number | null;
  sourceDispatchCode: string | null;
  baseDate: string | null;
  dueDate: string | null;
  legalBasis: string | null;
  status: string;
  isInternal: boolean;
  notes: string | null;
  createdAtUtc: string;
};

export type InpiDeadlinePayload = {
  ipAssetId: string;
  type: string;
  source: string;
  sourceRpiNumber?: number | null;
  sourceDispatchCode?: string | null;
  baseDate?: string | null;
  dueDate?: string | null;
  legalBasis?: string | null;
  status?: string | null;
  isInternal: boolean;
  notes?: string | null;
};
