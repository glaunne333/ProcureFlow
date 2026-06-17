# Supabase Plan

For ProcureFlow, Supabase should be used as hosted PostgreSQL only. Do not use Supabase Auth, Storage, Edge Functions, or Row Level Security for the MVP.

## Approach

Use EF Core migrations from the .NET backend as the source of truth for the schema.

The repo should contain:

- EF Core entity configuration in `server/src/ProcureFlow.Infrastructure`
- EF Core migration files committed to source control
- Optional generated SQL script for easy review/deployment
- Seed data for demo users, departments, vendors, and sample requests

## Local Workflow

1. Create a Supabase project from the dashboard.
2. Copy the PostgreSQL connection string.
3. Put the connection string in local user secrets or an uncommitted `.env` file.
4. Run EF Core migrations against Supabase.
5. Verify the tables and seed data in the Supabase table editor.

## Runtime Workflow

The deployed Render API connects to Supabase using an environment variable:

```text
ConnectionStrings__DefaultConnection=Host=...;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
```

The Angular app never connects to Supabase directly. It only talks to the .NET API.

## Migration Commands

Install EF Core tooling if needed:

```powershell
dotnet tool install --global dotnet-ef
```

Create a migration:

```powershell
dotnet ef migrations add InitialCreate --project server/src/ProcureFlow.Infrastructure --startup-project server/src/ProcureFlow.Api
```

Apply migrations:

```powershell
dotnet ef database update --project server/src/ProcureFlow.Infrastructure --startup-project server/src/ProcureFlow.Api
```

Generate a reviewable SQL script:

```powershell
dotnet ef migrations script --idempotent --project server/src/ProcureFlow.Infrastructure --startup-project server/src/ProcureFlow.Api --output database/procureflow.sql
```

## MVP Tables

- Users
- Departments
- Vendors
- ProcurementRequests
- RequestItems
- ApprovalLogs

## Keep It Small

Do not add stored procedures, triggers, database functions, custom schemas, or complex permissions for the MVP. Business rules should live in the .NET application layer.
