CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE TABLE "Departments" (
        "Id" uuid NOT NULL,
        "Name" character varying(120) NOT NULL,
        CONSTRAINT "PK_Departments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE TABLE "Vendors" (
        "Id" uuid NOT NULL,
        "Name" character varying(160) NOT NULL,
        "ContactEmail" character varying(240),
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Vendors" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Name" character varying(160) NOT NULL,
        "Email" character varying(240) NOT NULL,
        "PasswordHash" character varying(500) NOT NULL,
        "Role" character varying(40) NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Users_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE TABLE "ProcurementRequests" (
        "Id" uuid NOT NULL,
        "RequestNo" character varying(40) NOT NULL,
        "RequestedById" uuid NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "VendorId" uuid NOT NULL,
        "Status" character varying(40) NOT NULL,
        "EstimatedTotal" numeric(12,2) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "SubmittedAt" timestamp with time zone,
        CONSTRAINT "PK_ProcurementRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ProcurementRequests_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ProcurementRequests_Users_RequestedById" FOREIGN KEY ("RequestedById") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ProcurementRequests_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE TABLE "ApprovalLogs" (
        "Id" uuid NOT NULL,
        "RequestId" uuid NOT NULL,
        "ActorId" uuid NOT NULL,
        "FromStatus" character varying(40) NOT NULL,
        "ToStatus" character varying(40) NOT NULL,
        "Remarks" character varying(1000),
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ApprovalLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ApprovalLogs_ProcurementRequests_RequestId" FOREIGN KEY ("RequestId") REFERENCES "ProcurementRequests" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ApprovalLogs_Users_ActorId" FOREIGN KEY ("ActorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE TABLE "RequestItems" (
        "Id" uuid NOT NULL,
        "RequestId" uuid NOT NULL,
        "Description" character varying(300) NOT NULL,
        "Quantity" integer NOT NULL,
        "UnitCost" numeric(12,2) NOT NULL,
        "Category" character varying(120) NOT NULL,
        CONSTRAINT "PK_RequestItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RequestItems_ProcurementRequests_RequestId" FOREIGN KEY ("RequestId") REFERENCES "ProcurementRequests" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE INDEX "IX_ApprovalLogs_ActorId" ON "ApprovalLogs" ("ActorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE INDEX "IX_ApprovalLogs_RequestId" ON "ApprovalLogs" ("RequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE INDEX "IX_ProcurementRequests_DepartmentId" ON "ProcurementRequests" ("DepartmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE INDEX "IX_ProcurementRequests_RequestedById" ON "ProcurementRequests" ("RequestedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ProcurementRequests_RequestNo" ON "ProcurementRequests" ("RequestNo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE INDEX "IX_ProcurementRequests_VendorId" ON "ProcurementRequests" ("VendorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE INDEX "IX_RequestItems_RequestId" ON "RequestItems" ("RequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE INDEX "IX_Users_DepartmentId" ON "Users" ("DepartmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617180044_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260617180044_InitialCreate', '10.0.4');
    END IF;
END $EF$;
COMMIT;

