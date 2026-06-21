CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "ImportJobs" (
        "Id" uuid NOT NULL,
        "Source" character varying(120) NOT NULL,
        "Status" character varying(40) NOT NULL,
        "StartedAtUtc" timestamp with time zone NOT NULL,
        "FinishedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_ImportJobs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "TrademarkOwners" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Document" character varying(40),
        CONSTRAINT "PK_TrademarkOwners" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "TrademarkStatuses" (
        "Id" uuid NOT NULL,
        "Code" character varying(40) NOT NULL,
        "Description" character varying(200) NOT NULL,
        CONSTRAINT "PK_TrademarkStatuses" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Email" character varying(160) NOT NULL,
        "PasswordHash" text NOT NULL,
        "Role" character varying(40) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "ImportJobLogs" (
        "Id" uuid NOT NULL,
        "ImportJobId" uuid NOT NULL,
        "Message" character varying(2000) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ImportJobLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ImportJobLogs_ImportJobs_ImportJobId" FOREIGN KEY ("ImportJobId") REFERENCES "ImportJobs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "Trademarks" (
        "Id" uuid NOT NULL,
        "ProcessNumber" character varying(40) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "OwnerId" uuid,
        "StatusId" uuid,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Trademarks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Trademarks_TrademarkOwners_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "TrademarkOwners" ("Id"),
        CONSTRAINT "FK_Trademarks_TrademarkStatuses_StatusId" FOREIGN KEY ("StatusId") REFERENCES "TrademarkStatuses" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "MonitoredTrademarks" (
        "Id" uuid NOT NULL,
        "TrademarkId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_MonitoredTrademarks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MonitoredTrademarks_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_MonitoredTrademarks_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "TrademarkDispatches" (
        "Id" uuid NOT NULL,
        "TrademarkId" uuid NOT NULL,
        "Code" character varying(40) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "PublishedAt" date NOT NULL,
        CONSTRAINT "PK_TrademarkDispatches" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrademarkDispatches_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE TABLE "TrademarkNiceClasses" (
        "Id" uuid NOT NULL,
        "TrademarkId" uuid NOT NULL,
        "ClassNumber" integer NOT NULL,
        "Specification" character varying(1000),
        CONSTRAINT "PK_TrademarkNiceClasses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrademarkNiceClasses_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE INDEX "IX_ImportJobLogs_ImportJobId" ON "ImportJobLogs" ("ImportJobId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE INDEX "IX_MonitoredTrademarks_TrademarkId" ON "MonitoredTrademarks" ("TrademarkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_MonitoredTrademarks_UserId_TrademarkId" ON "MonitoredTrademarks" ("UserId", "TrademarkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE INDEX "IX_TrademarkDispatches_TrademarkId" ON "TrademarkDispatches" ("TrademarkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE INDEX "IX_TrademarkNiceClasses_TrademarkId" ON "TrademarkNiceClasses" ("TrademarkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE INDEX "IX_Trademarks_OwnerId" ON "Trademarks" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Trademarks_ProcessNumber" ON "Trademarks" ("ProcessNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE INDEX "IX_Trademarks_StatusId" ON "Trademarks" ("StatusId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_TrademarkStatuses_Code" ON "TrademarkStatuses" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611153506_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611153506_InitialCreate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    DROP INDEX "IX_TrademarkNiceClasses_TrademarkId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    DROP INDEX "IX_TrademarkDispatches_TrademarkId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    ALTER TABLE "Trademarks" ADD "FilingDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    ALTER TABLE "Trademarks" ADD "RegistrationDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    ALTER TABLE "TrademarkNiceClasses" ADD "Code" character varying(20) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    CREATE INDEX "IX_Trademarks_Name" ON "Trademarks" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    CREATE INDEX "IX_TrademarkOwners_Name" ON "TrademarkOwners" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    CREATE INDEX "IX_TrademarkNiceClasses_Code" ON "TrademarkNiceClasses" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    CREATE UNIQUE INDEX "IX_TrademarkNiceClasses_TrademarkId_Code" ON "TrademarkNiceClasses" ("TrademarkId", "Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    CREATE UNIQUE INDEX "IX_TrademarkDispatches_TrademarkId_Code_PublishedAt" ON "TrademarkDispatches" ("TrademarkId", "Code", "PublishedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611154924_Phase2InpiOpenDataImport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611154924_Phase2InpiOpenDataImport', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611163939_AddImportJobSourceType') THEN
    ALTER TABLE "ImportJobs" ADD "SourceType" character varying(40) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611163939_AddImportJobSourceType') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611163939_AddImportJobSourceType', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611165827_AddRpiTrademarkDispatchImport') THEN
    DROP INDEX "IX_TrademarkDispatches_TrademarkId_Code_PublishedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611165827_AddRpiTrademarkDispatchImport') THEN
    ALTER TABLE "TrademarkDispatches" ADD "RpiNumber" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611165827_AddRpiTrademarkDispatchImport') THEN
    CREATE UNIQUE INDEX "IX_TrademarkDispatches_TrademarkId_RpiNumber_Code" ON "TrademarkDispatches" ("TrademarkId", "RpiNumber", "Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611165827_AddRpiTrademarkDispatchImport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611165827_AddRpiTrademarkDispatchImport', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "HasPendingChanges" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "LastCheckedAtUtc" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "LastKnownDispatchCode" character varying(40);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "LastKnownDispatchDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "LastKnownDispatchId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "Notes" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD "ProcessNumber" character varying(40) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    UPDATE "MonitoredTrademarks" AS mt
    SET "IsActive" = TRUE,
        "ProcessNumber" = t."ProcessNumber"
    FROM "Trademarks" AS t
    WHERE mt."TrademarkId" = t."Id";

    WITH latest_dispatch AS (
        SELECT DISTINCT ON ("TrademarkId")
            "TrademarkId",
            "Id",
            "Code",
            "PublishedAt"
        FROM "TrademarkDispatches"
        ORDER BY "TrademarkId", "PublishedAt" DESC, COALESCE("RpiNumber", 0) DESC, "Id" DESC
    )
    UPDATE "MonitoredTrademarks" AS mt
    SET "LastKnownDispatchId" = latest_dispatch."Id",
        "LastKnownDispatchCode" = latest_dispatch."Code",
        "LastKnownDispatchDate" = latest_dispatch."PublishedAt",
        "LastCheckedAtUtc" = NOW()
    FROM latest_dispatch
    WHERE mt."TrademarkId" = latest_dispatch."TrademarkId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE TABLE "TrademarkMonitoringEvents" (
        "Id" uuid NOT NULL,
        "MonitoredTrademarkId" uuid NOT NULL,
        "TrademarkId" uuid NOT NULL,
        "DispatchId" uuid NOT NULL,
        "ProcessNumber" character varying(40) NOT NULL,
        "EventType" character varying(80) NOT NULL,
        "PreviousDispatchCode" character varying(40),
        "CurrentDispatchCode" character varying(40),
        "PreviousDispatchDate" date,
        "CurrentDispatchDate" date,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "IsRead" boolean NOT NULL,
        CONSTRAINT "PK_TrademarkMonitoringEvents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrademarkMonitoringEvents_MonitoredTrademarks_MonitoredTrad~" FOREIGN KEY ("MonitoredTrademarkId") REFERENCES "MonitoredTrademarks" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TrademarkMonitoringEvents_TrademarkDispatches_DispatchId" FOREIGN KEY ("DispatchId") REFERENCES "TrademarkDispatches" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_TrademarkMonitoringEvents_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE INDEX "IX_MonitoredTrademarks_LastKnownDispatchId" ON "MonitoredTrademarks" ("LastKnownDispatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE INDEX "IX_MonitoredTrademarks_ProcessNumber" ON "MonitoredTrademarks" ("ProcessNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE INDEX "IX_MonitoredTrademarks_UserId" ON "MonitoredTrademarks" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE INDEX "IX_TrademarkMonitoringEvents_DispatchId" ON "TrademarkMonitoringEvents" ("DispatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE INDEX "IX_TrademarkMonitoringEvents_IsRead" ON "TrademarkMonitoringEvents" ("IsRead");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE INDEX "IX_TrademarkMonitoringEvents_MonitoredTrademarkId" ON "TrademarkMonitoringEvents" ("MonitoredTrademarkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    CREATE INDEX "IX_TrademarkMonitoringEvents_TrademarkId" ON "TrademarkMonitoringEvents" ("TrademarkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    ALTER TABLE "MonitoredTrademarks" ADD CONSTRAINT "FK_MonitoredTrademarks_TrademarkDispatches_LastKnownDispatchId" FOREIGN KEY ("LastKnownDispatchId") REFERENCES "TrademarkDispatches" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611181024_AddTrademarkMonitoringEvents') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611181024_AddTrademarkMonitoringEvents', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611182514_AddRpiHistoricalImport') THEN
    CREATE TABLE "RpiHistoricalImportRuns" (
        "Id" uuid NOT NULL,
        "StartRpi" integer NOT NULL,
        "EndRpi" integer NOT NULL,
        "CurrentRpi" integer NOT NULL,
        "BatchSize" integer NOT NULL,
        "Status" character varying(40) NOT NULL,
        "StartedAtUtc" timestamp with time zone NOT NULL,
        "FinishedAtUtc" timestamp with time zone,
        "TotalRpis" integer NOT NULL,
        "SuccessfulRpis" integer NOT NULL,
        "FailedRpis" integer NOT NULL,
        "TotalDispatchesImported" integer NOT NULL,
        "ErrorMessage" character varying(2000),
        CONSTRAINT "PK_RpiHistoricalImportRuns" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611182514_AddRpiHistoricalImport') THEN
    CREATE TABLE "RpiImportCheckpoints" (
        "Id" uuid NOT NULL,
        "RpiNumber" integer NOT NULL,
        "Status" character varying(40) NOT NULL,
        "StartedAtUtc" timestamp with time zone NOT NULL,
        "FinishedAtUtc" timestamp with time zone,
        "DispatchesImported" integer NOT NULL,
        "FailedRows" integer NOT NULL,
        "ErrorMessage" character varying(2000),
        "ZipPath" character varying(1000),
        "XmlOrTxtFilesCount" integer NOT NULL,
        CONSTRAINT "PK_RpiImportCheckpoints" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611182514_AddRpiHistoricalImport') THEN
    CREATE INDEX "IX_RpiHistoricalImportRuns_Status" ON "RpiHistoricalImportRuns" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611182514_AddRpiHistoricalImport') THEN
    CREATE UNIQUE INDEX "IX_RpiImportCheckpoints_RpiNumber" ON "RpiImportCheckpoints" ("RpiNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611182514_AddRpiHistoricalImport') THEN
    CREATE INDEX "IX_RpiImportCheckpoints_Status" ON "RpiImportCheckpoints" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611182514_AddRpiHistoricalImport') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611182514_AddRpiHistoricalImport', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611211351_AddRpiHistoryDiagnostics') THEN
    ALTER TABLE "RpiImportCheckpoints" ADD "DuplicateDispatches" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611211351_AddRpiHistoryDiagnostics') THEN
    ALTER TABLE "RpiHistoricalImportRuns" ADD "DuplicateDispatches" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611211351_AddRpiHistoryDiagnostics') THEN
    ALTER TABLE "RpiHistoricalImportRuns" ADD "LastErrorSummary" character varying(2000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611211351_AddRpiHistoryDiagnostics') THEN
    ALTER TABLE "RpiHistoricalImportRuns" ADD "SkippedRpis" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611211351_AddRpiHistoryDiagnostics') THEN
    UPDATE "RpiImportCheckpoints"
    SET "DuplicateDispatches" = "FailedRows"
    WHERE "Status" = 'Completed'
      AND "DispatchesImported" = 0
      AND "FailedRows" > 0;

    UPDATE "RpiHistoricalImportRuns" AS run
    SET "DuplicateDispatches" = COALESCE((
            SELECT SUM(checkpoint."DuplicateDispatches")
            FROM "RpiImportCheckpoints" AS checkpoint
            WHERE checkpoint."RpiNumber" BETWEEN run."StartRpi" AND run."EndRpi"
        ), 0),
        "LastErrorSummary" = (
            SELECT CONCAT(error_group.count, ' RPI(s), ', error_group.first_rpi, '-', error_group.last_rpi, ': ', error_group.error_message)
            FROM (
                SELECT
                    checkpoint."ErrorMessage" AS error_message,
                    COUNT(*) AS count,
                    MIN(checkpoint."RpiNumber") AS first_rpi,
                    MAX(checkpoint."RpiNumber") AS last_rpi
                FROM "RpiImportCheckpoints" AS checkpoint
                WHERE checkpoint."RpiNumber" BETWEEN run."StartRpi" AND run."EndRpi"
                  AND checkpoint."Status" = 'Failed'
                  AND checkpoint."ErrorMessage" IS NOT NULL
                GROUP BY checkpoint."ErrorMessage"
                ORDER BY COUNT(*) DESC, MIN(checkpoint."RpiNumber")
                LIMIT 1
            ) AS error_group
        );

    UPDATE "RpiHistoricalImportRuns"
    SET "Status" = CASE
        WHEN "FailedRpis" > "SuccessfulRpis" THEN 'Failed'
        WHEN "FailedRpis" > 0 OR "DuplicateDispatches" > 0 OR "SkippedRpis" > 0 THEN 'CompletedWithWarnings'
        ELSE "Status"
    END
    WHERE "Status" = 'Completed';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260611211351_AddRpiHistoryDiagnostics') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260611211351_AddRpiHistoryDiagnostics', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614114750_AddTrademarkOwnerLinks') THEN
    CREATE TABLE "TrademarkOwnerLinks" (
        "Id" uuid NOT NULL,
        "TrademarkId" uuid NOT NULL,
        "OwnerId" uuid NOT NULL,
        CONSTRAINT "PK_TrademarkOwnerLinks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrademarkOwnerLinks_TrademarkOwners_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "TrademarkOwners" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TrademarkOwnerLinks_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614114750_AddTrademarkOwnerLinks') THEN
    CREATE INDEX "IX_TrademarkOwnerLinks_OwnerId" ON "TrademarkOwnerLinks" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614114750_AddTrademarkOwnerLinks') THEN
    CREATE UNIQUE INDEX "IX_TrademarkOwnerLinks_TrademarkId_OwnerId" ON "TrademarkOwnerLinks" ("TrademarkId", "OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614114750_AddTrademarkOwnerLinks') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260614114750_AddTrademarkOwnerLinks', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE TABLE "Universities" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Cnpj" character varying(20),
        "Tier" character varying(40) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Universities" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "UniversityId" uuid,
        "Module" character varying(80) NOT NULL,
        "EntityName" character varying(120) NOT NULL,
        "EntityId" uuid NOT NULL,
        "Action" character varying(80) NOT NULL,
        "IpAddress" character varying(80),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditLogs_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id"),
        CONSTRAINT "FK_AuditLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE TABLE "Inventions" (
        "Id" uuid NOT NULL,
        "UniversityId" uuid NOT NULL,
        "Title" character varying(240) NOT NULL,
        "Summary" character varying(4000) NOT NULL,
        "Inventors" character varying(2000) NOT NULL,
        "DepositDate" date,
        "Status" character varying(40) NOT NULL,
        "PatentNumber" character varying(80),
        "InpiProcessNumber" character varying(80),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Inventions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Inventions_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE TABLE "NitUserProfiles" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "UniversityId" uuid NOT NULL,
        "Role" character varying(60) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_NitUserProfiles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_NitUserProfiles_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_NitUserProfiles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE TABLE "InventionDocuments" (
        "Id" uuid NOT NULL,
        "InventionId" uuid NOT NULL,
        "Type" character varying(80) NOT NULL,
        "FileName" character varying(260) NOT NULL,
        "FileHash" character varying(128) NOT NULL,
        "StoragePath" character varying(1000) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_InventionDocuments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InventionDocuments_Inventions_InventionId" FOREIGN KEY ("InventionId") REFERENCES "Inventions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE TABLE "TechnologyTransferContracts" (
        "Id" uuid NOT NULL,
        "InventionId" uuid NOT NULL,
        "CompanyName" character varying(200) NOT NULL,
        "Cnpj" character varying(20),
        "RoyaltyModel" character varying(120) NOT NULL,
        "RoyaltyValue" numeric(18,4),
        "MinimumGuarantee" numeric(18,2),
        "SignedAt" date,
        "Status" character varying(40) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_TechnologyTransferContracts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TechnologyTransferContracts_Inventions_InventionId" FOREIGN KEY ("InventionId") REFERENCES "Inventions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_AuditLogs_Module_EntityName_EntityId" ON "AuditLogs" ("Module", "EntityName", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_AuditLogs_UniversityId" ON "AuditLogs" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_InventionDocuments_FileHash" ON "InventionDocuments" ("FileHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_InventionDocuments_InventionId" ON "InventionDocuments" ("InventionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_Inventions_InpiProcessNumber" ON "Inventions" ("InpiProcessNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_Inventions_IsActive" ON "Inventions" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_Inventions_Status" ON "Inventions" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_Inventions_UniversityId" ON "Inventions" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_NitUserProfiles_UniversityId" ON "NitUserProfiles" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_NitUserProfiles_UserId" ON "NitUserProfiles" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE UNIQUE INDEX "IX_NitUserProfiles_UserId_UniversityId" ON "NitUserProfiles" ("UserId", "UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_TechnologyTransferContracts_InventionId" ON "TechnologyTransferContracts" ("InventionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_TechnologyTransferContracts_Status" ON "TechnologyTransferContracts" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_Universities_Cnpj" ON "Universities" ("Cnpj");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    CREATE INDEX "IX_Universities_IsActive" ON "Universities" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614225633_AddNitInovaPlusModule') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260614225633_AddNitInovaPlusModule', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE TABLE "Clients" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "DocumentNumber" character varying(40),
        "Email" character varying(160),
        "Phone" character varying(40),
        "Notes" character varying(2000),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Clients" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE TABLE "Deadlines" (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000),
        "DueDate" date NOT NULL,
        "Status" character varying(40) NOT NULL,
        "Type" character varying(80) NOT NULL,
        "ClientId" uuid,
        "TrademarkId" uuid,
        "InventionId" uuid,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Deadlines" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Deadlines_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Deadlines_Inventions_InventionId" FOREIGN KEY ("InventionId") REFERENCES "Inventions" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Deadlines_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Clients_DocumentNumber" ON "Clients" ("DocumentNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Clients_IsActive" ON "Clients" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Clients_Name" ON "Clients" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Deadlines_ClientId" ON "Deadlines" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Deadlines_DueDate" ON "Deadlines" ("DueDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Deadlines_InventionId" ON "Deadlines" ("InventionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Deadlines_IsActive" ON "Deadlines" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Deadlines_Status" ON "Deadlines" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Deadlines_TrademarkId" ON "Deadlines" ("TrademarkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    CREATE INDEX "IX_Deadlines_Type" ON "Deadlines" ("Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614235317_AddOperationalClientsDeadlines') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260614235317_AddOperationalClientsDeadlines', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    CREATE TABLE "IPAssets" (
        "Id" uuid NOT NULL,
        "Type" character varying(40) NOT NULL,
        "InpiProcessNumber" character varying(80),
        "Title" character varying(240) NOT NULL,
        "OwnerName" character varying(240),
        "Status" character varying(80) NOT NULL,
        "FilingDate" date,
        "GrantDate" date,
        "ExpirationDate" date,
        "InternalDeadline" date,
        "ClientId" uuid,
        "UniversityId" uuid,
        "IsMonitored" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_IPAssets" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_IPAssets_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_IPAssets_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    CREATE INDEX "IX_IPAssets_ClientId" ON "IPAssets" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    CREATE INDEX "IX_IPAssets_InpiProcessNumber" ON "IPAssets" ("InpiProcessNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    CREATE INDEX "IX_IPAssets_IsActive" ON "IPAssets" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    CREATE INDEX "IX_IPAssets_IsMonitored" ON "IPAssets" ("IsMonitored");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    CREATE INDEX "IX_IPAssets_Type" ON "IPAssets" ("Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    CREATE INDEX "IX_IPAssets_UniversityId" ON "IPAssets" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615151838_AddIPAssetsPortfolio') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260615151838_AddIPAssetsPortfolio', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE TABLE "Patents" (
        "Id" uuid NOT NULL,
        "InpiProcessNumber" character varying(80) NOT NULL,
        "Title" character varying(240) NOT NULL,
        "Abstract" character varying(4000),
        "Applicants" character varying(2000),
        "Inventors" character varying(2000),
        "IpcClass" character varying(120),
        "FilingDate" date,
        "PublicationDate" date,
        "GrantDate" date,
        "Status" character varying(80),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Patents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE TABLE "PatentDispatches" (
        "Id" uuid NOT NULL,
        "PatentId" uuid NOT NULL,
        "RpiNumber" integer,
        "DispatchCode" character varying(40) NOT NULL,
        "DispatchDescription" character varying(1000) NOT NULL,
        "DispatchDate" date,
        "Complement" character varying(2000),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PatentDispatches" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PatentDispatches_Patents_PatentId" FOREIGN KEY ("PatentId") REFERENCES "Patents" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE TABLE "MonitoredPatents" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "PatentId" uuid NOT NULL,
        "InpiProcessNumber" character varying(80) NOT NULL,
        "Notes" character varying(1000),
        "IsActive" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "LastCheckedAtUtc" timestamp with time zone,
        "LastKnownDispatchId" uuid,
        "LastKnownDispatchCode" character varying(40),
        "LastKnownDispatchDate" date,
        "HasPendingChanges" boolean NOT NULL,
        CONSTRAINT "PK_MonitoredPatents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MonitoredPatents_PatentDispatches_LastKnownDispatchId" FOREIGN KEY ("LastKnownDispatchId") REFERENCES "PatentDispatches" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_MonitoredPatents_Patents_PatentId" FOREIGN KEY ("PatentId") REFERENCES "Patents" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_MonitoredPatents_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE TABLE "PatentMonitoringEvents" (
        "Id" uuid NOT NULL,
        "MonitoredPatentId" uuid NOT NULL,
        "PatentId" uuid NOT NULL,
        "DispatchId" uuid NOT NULL,
        "InpiProcessNumber" character varying(80) NOT NULL,
        "EventType" character varying(80) NOT NULL,
        "PreviousDispatchCode" character varying(40),
        "CurrentDispatchCode" character varying(40),
        "PreviousDispatchDate" date,
        "CurrentDispatchDate" date,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "IsRead" boolean NOT NULL,
        CONSTRAINT "PK_PatentMonitoringEvents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PatentMonitoringEvents_MonitoredPatents_MonitoredPatentId" FOREIGN KEY ("MonitoredPatentId") REFERENCES "MonitoredPatents" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_PatentMonitoringEvents_PatentDispatches_DispatchId" FOREIGN KEY ("DispatchId") REFERENCES "PatentDispatches" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PatentMonitoringEvents_Patents_PatentId" FOREIGN KEY ("PatentId") REFERENCES "Patents" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_MonitoredPatents_InpiProcessNumber" ON "MonitoredPatents" ("InpiProcessNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_MonitoredPatents_LastKnownDispatchId" ON "MonitoredPatents" ("LastKnownDispatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_MonitoredPatents_PatentId" ON "MonitoredPatents" ("PatentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_MonitoredPatents_UserId" ON "MonitoredPatents" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE UNIQUE INDEX "IX_MonitoredPatents_UserId_PatentId" ON "MonitoredPatents" ("UserId", "PatentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_PatentDispatches_PatentId" ON "PatentDispatches" ("PatentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE UNIQUE INDEX "IX_PatentDispatches_PatentId_RpiNumber_DispatchCode" ON "PatentDispatches" ("PatentId", "RpiNumber", "DispatchCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_PatentMonitoringEvents_DispatchId" ON "PatentMonitoringEvents" ("DispatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_PatentMonitoringEvents_IsRead" ON "PatentMonitoringEvents" ("IsRead");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_PatentMonitoringEvents_MonitoredPatentId" ON "PatentMonitoringEvents" ("MonitoredPatentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_PatentMonitoringEvents_PatentId" ON "PatentMonitoringEvents" ("PatentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE UNIQUE INDEX "IX_Patents_InpiProcessNumber" ON "Patents" ("InpiProcessNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_Patents_IsActive" ON "Patents" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_Patents_Status" ON "Patents" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    CREATE INDEX "IX_Patents_Title" ON "Patents" ("Title");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615152745_AddPatentsAndPatentMonitoring') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260615152745_AddPatentsAndPatentMonitoring', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615153258_AddInpiDeadlines') THEN
    CREATE TABLE "InpiDeadlines" (
        "Id" uuid NOT NULL,
        "IPAssetId" uuid NOT NULL,
        "Type" character varying(80) NOT NULL,
        "Source" character varying(80) NOT NULL,
        "SourceRpiNumber" integer,
        "SourceDispatchCode" character varying(40),
        "BaseDate" date,
        "DueDate" date,
        "LegalBasis" character varying(1000),
        "Status" character varying(80) NOT NULL,
        "IsInternal" boolean NOT NULL,
        "Notes" character varying(2000),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_InpiDeadlines" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InpiDeadlines_IPAssets_IPAssetId" FOREIGN KEY ("IPAssetId") REFERENCES "IPAssets" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615153258_AddInpiDeadlines') THEN
    CREATE INDEX "IX_InpiDeadlines_DueDate" ON "InpiDeadlines" ("DueDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615153258_AddInpiDeadlines') THEN
    CREATE INDEX "IX_InpiDeadlines_IPAssetId" ON "InpiDeadlines" ("IPAssetId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615153258_AddInpiDeadlines') THEN
    CREATE INDEX "IX_InpiDeadlines_IsInternal" ON "InpiDeadlines" ("IsInternal");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615153258_AddInpiDeadlines') THEN
    CREATE INDEX "IX_InpiDeadlines_Status" ON "InpiDeadlines" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615153258_AddInpiDeadlines') THEN
    CREATE INDEX "IX_InpiDeadlines_Type" ON "InpiDeadlines" ("Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615153258_AddInpiDeadlines') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260615153258_AddInpiDeadlines', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615154500_AddTrademarkInpiDetailUrl') THEN
    ALTER TABLE "Trademarks" ADD "InpiDetailUrl" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260615154500_AddTrademarkInpiDetailUrl') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260615154500_AddTrademarkInpiDetailUrl', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    ALTER TABLE "Trademarks" ADD "ExpirationDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    ALTER TABLE "Trademarks" ADD "LegalRepresentative" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    ALTER TABLE "Trademarks" ADD "Nature" character varying(120);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    ALTER TABLE "Trademarks" ADD "Presentation" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    CREATE TABLE "TrademarkViennaClasses" (
        "Id" uuid NOT NULL,
        "TrademarkId" uuid NOT NULL,
        "Edition" character varying(20) NOT NULL,
        "Code" character varying(40) NOT NULL,
        "Description" character varying(300),
        CONSTRAINT "PK_TrademarkViennaClasses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrademarkViennaClasses_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    CREATE INDEX "IX_TrademarkViennaClasses_Code" ON "TrademarkViennaClasses" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    CREATE UNIQUE INDEX "IX_TrademarkViennaClasses_TrademarkId_Edition_Code" ON "TrademarkViennaClasses" ("TrademarkId", "Edition", "Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618191728_AddTrademarkDetailedFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260618191728_AddTrademarkDetailedFields', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618192629_AddTrademarkPetitions') THEN
    CREATE TABLE "TrademarkPetitions" (
        "Id" uuid NOT NULL,
        "TrademarkId" uuid NOT NULL,
        "Protocol" character varying(80) NOT NULL,
        "FiledAt" date,
        "ServiceCode" character varying(40),
        "ClientName" character varying(200),
        "Delivery" character varying(80),
        "DeliveryDate" date,
        CONSTRAINT "PK_TrademarkPetitions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TrademarkPetitions_Trademarks_TrademarkId" FOREIGN KEY ("TrademarkId") REFERENCES "Trademarks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618192629_AddTrademarkPetitions') THEN
    CREATE INDEX "IX_TrademarkPetitions_Protocol" ON "TrademarkPetitions" ("Protocol");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618192629_AddTrademarkPetitions') THEN
    CREATE UNIQUE INDEX "IX_TrademarkPetitions_TrademarkId_Protocol" ON "TrademarkPetitions" ("TrademarkId", "Protocol");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618192629_AddTrademarkPetitions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260618192629_AddTrademarkPetitions', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618193026_AddTrademarkLogoStorage') THEN
    ALTER TABLE "Trademarks" ADD "LogoContentType" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618193026_AddTrademarkLogoStorage') THEN
    ALTER TABLE "Trademarks" ADD "LogoPath" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618193026_AddTrademarkLogoStorage') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260618193026_AddTrademarkLogoStorage', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Universities" ADD "ContactName" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Universities" ADD "Email" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Universities" ADD "Phone" character varying(40);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Universities" ADD "Status" character varying(40) NOT NULL DEFAULT 'Ativa';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Universities" ADD "TradeName" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Universities" ADD "Type" character varying(60) NOT NULL DEFAULT 'Universidade';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Universities" ADD "Website" character varying(300);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "AutomaticRenewal" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "CompanyId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "EndDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "FixedValue" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "Number" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "RoyaltyPercentage" numeric(8,4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "StartDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "Term" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "Type" character varying(80) NOT NULL DEFAULT 'Licenciamento';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "UniversityId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD "UpdatedAtUtc" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "CommercialPotential" character varying(2000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "CreationDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "ExecutiveSummary" character varying(4000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "ProtectionStatus" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "Responsible" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "TargetMarket" character varying(2000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "TechnicalDescription" character varying(12000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "TechnologyArea" character varying(160);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "Inventions" ADD "Trl" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "AuditLogs" ADD "NewValue" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "AuditLogs" ADD "PreviousValue" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE TABLE "Companies" (
        "Id" uuid NOT NULL,
        "UniversityId" uuid,
        "LegalName" character varying(200) NOT NULL,
        "TradeName" character varying(200),
        "Cnpj" character varying(20),
        "Segment" character varying(120),
        "Size" character varying(40),
        "ContactName" character varying(200),
        "Email" character varying(200),
        "Phone" character varying(40),
        "Website" character varying(300),
        "Notes" character varying(2000),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Companies" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Companies_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE TABLE "NitDocuments" (
        "Id" uuid NOT NULL,
        "Name" character varying(240) NOT NULL,
        "Type" character varying(80) NOT NULL,
        "UniversityId" uuid NOT NULL,
        "InventionId" uuid,
        "ContractId" uuid,
        "FileName" character varying(260) NOT NULL,
        "ContentType" character varying(120) NOT NULL,
        "FileSize" bigint NOT NULL,
        "StoragePath" character varying(1000) NOT NULL,
        "UploadedByUserId" uuid NOT NULL,
        "UploadedAtUtc" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_NitDocuments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_NitDocuments_Inventions_InventionId" FOREIGN KEY ("InventionId") REFERENCES "Inventions" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_NitDocuments_TechnologyTransferContracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "TechnologyTransferContracts" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_NitDocuments_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_NitDocuments_Users_UploadedByUserId" FOREIGN KEY ("UploadedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE TABLE "Researchers" (
        "Id" uuid NOT NULL,
        "UniversityId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Cpf" character varying(20),
        "Email" character varying(200),
        "Phone" character varying(40),
        "Department" character varying(160),
        "Position" character varying(120),
        "LattesUrl" character varying(500),
        "Orcid" character varying(40),
        "Specialties" character varying(2000),
        "TechnologyAreas" character varying(2000),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Researchers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Researchers_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE TABLE "RoyaltyPayments" (
        "Id" uuid NOT NULL,
        "ContractId" uuid NOT NULL,
        "Competence" date NOT NULL,
        "AmountReceived" numeric(18,2) NOT NULL,
        "Notes" character varying(2000),
        "ReceivedAt" date NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_RoyaltyPayments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RoyaltyPayments_TechnologyTransferContracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "TechnologyTransferContracts" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE TABLE "TechnologyTransferOpportunities" (
        "Id" uuid NOT NULL,
        "InventionId" uuid NOT NULL,
        "UniversityId" uuid NOT NULL,
        "CompanyId" uuid,
        "Stage" character varying(80) NOT NULL,
        "Notes" character varying(2000),
        "SortOrder" integer NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_TechnologyTransferOpportunities" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TechnologyTransferOpportunities_Companies_CompanyId" FOREIGN KEY ("CompanyId") REFERENCES "Companies" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_TechnologyTransferOpportunities_Inventions_InventionId" FOREIGN KEY ("InventionId") REFERENCES "Inventions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TechnologyTransferOpportunities_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE TABLE "InventionResearchers" (
        "InventionId" uuid NOT NULL,
        "ResearcherId" uuid NOT NULL,
        CONSTRAINT "PK_InventionResearchers" PRIMARY KEY ("InventionId", "ResearcherId"),
        CONSTRAINT "FK_InventionResearchers_Inventions_InventionId" FOREIGN KEY ("InventionId") REFERENCES "Inventions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_InventionResearchers_Researchers_ResearcherId" FOREIGN KEY ("ResearcherId") REFERENCES "Researchers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_TechnologyTransferContracts_CompanyId" ON "TechnologyTransferContracts" ("CompanyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_TechnologyTransferContracts_UniversityId" ON "TechnologyTransferContracts" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Companies_Cnpj" ON "Companies" ("Cnpj");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Companies_IsActive" ON "Companies" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Companies_LegalName" ON "Companies" ("LegalName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Companies_UniversityId" ON "Companies" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_InventionResearchers_ResearcherId" ON "InventionResearchers" ("ResearcherId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_NitDocuments_ContractId" ON "NitDocuments" ("ContractId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_NitDocuments_InventionId" ON "NitDocuments" ("InventionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_NitDocuments_Type" ON "NitDocuments" ("Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_NitDocuments_UniversityId" ON "NitDocuments" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_NitDocuments_UploadedAtUtc" ON "NitDocuments" ("UploadedAtUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_NitDocuments_UploadedByUserId" ON "NitDocuments" ("UploadedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Researchers_Cpf" ON "Researchers" ("Cpf");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Researchers_IsActive" ON "Researchers" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Researchers_Name" ON "Researchers" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_Researchers_UniversityId" ON "Researchers" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_RoyaltyPayments_Competence" ON "RoyaltyPayments" ("Competence");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_RoyaltyPayments_ContractId" ON "RoyaltyPayments" ("ContractId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_RoyaltyPayments_ReceivedAt" ON "RoyaltyPayments" ("ReceivedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_TechnologyTransferOpportunities_CompanyId" ON "TechnologyTransferOpportunities" ("CompanyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_TechnologyTransferOpportunities_InventionId" ON "TechnologyTransferOpportunities" ("InventionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_TechnologyTransferOpportunities_IsActive" ON "TechnologyTransferOpportunities" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_TechnologyTransferOpportunities_Stage" ON "TechnologyTransferOpportunities" ("Stage");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    CREATE INDEX "IX_TechnologyTransferOpportunities_UniversityId" ON "TechnologyTransferOpportunities" ("UniversityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD CONSTRAINT "FK_TechnologyTransferContracts_Companies_CompanyId" FOREIGN KEY ("CompanyId") REFERENCES "Companies" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    ALTER TABLE "TechnologyTransferContracts" ADD CONSTRAINT "FK_TechnologyTransferContracts_Universities_UniversityId" FOREIGN KEY ("UniversityId") REFERENCES "Universities" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618221517_NitInnovationManagement20') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260618221517_NitInnovationManagement20', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223731_AddNitDocumentEncryption') THEN
    ALTER TABLE "NitDocuments" ADD "EncryptionAlgorithm" character varying(80);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223731_AddNitDocumentEncryption') THEN
    ALTER TABLE "NitDocuments" ADD "EncryptionIV" character varying(128);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223731_AddNitDocumentEncryption') THEN
    ALTER TABLE "NitDocuments" ADD "IsEncrypted" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223731_AddNitDocumentEncryption') THEN
    ALTER TABLE "NitDocuments" ADD "OriginalFileName" character varying(260) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223731_AddNitDocumentEncryption') THEN
    ALTER TABLE "NitDocuments" ADD "StoredFileName" character varying(260) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223731_AddNitDocumentEncryption') THEN
    UPDATE "NitDocuments"
    SET "OriginalFileName" = "FileName",
        "StoredFileName" = "FileName"
    WHERE "OriginalFileName" = '' OR "StoredFileName" = '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260618223731_AddNitDocumentEncryption') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260618223731_AddNitDocumentEncryption', '8.0.11');
    END IF;
END $EF$;
COMMIT;

