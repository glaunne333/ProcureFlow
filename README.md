# ProcureFlow

ProcureFlow is a small procurement request and approval system built for a portfolio project. The goal is to show one realistic enterprise workflow without overbuilding: employees submit purchase requests, managers approve or reject them, and finance marks approved requests as ordered or completed.

## Small-scope MVP

- Demo login with seeded users for Employee, Manager, and Finance roles
- Purchase request creation with line items
- Status workflow: Draft -> Submitted -> Approved or Rejected -> Ordered -> Completed or Cancelled
- Approval log for each workflow action
- Compact dashboard/request list with summary counts
- Angular + Tailwind frontend
- .NET 10 Web API backend
- Supabase PostgreSQL database
- Vercel frontend deployment and Render backend deployment

## Portfolio Guardrails

- Build one complete workflow, not a procurement platform
- Use seeded vendors/categories instead of admin CRUD
- Keep reporting to simple counts and recent activity
- Keep authorization role-based and demo-friendly
- Avoid file uploads, multi-level routing, payments, invoices, notifications, and advanced analytics

## Planned Structure

```text
client/                         Angular + Tailwind frontend
server/
  src/
    ProcureFlow.Api/            Endpoints, middleware, auth, OpenAPI
    ProcureFlow.Application/    DTOs, validators, interfaces, use cases
    ProcureFlow.Domain/         Entities, enums, business rules
    ProcureFlow.Infrastructure/ EF Core, repositories, JWT service
  tests/
    ProcureFlow.Tests/          Focused workflow and service tests
docs/
  scope.md
```

## Demo Roles

- Employee: create drafts, submit requests, view own request history
- Manager: view submitted requests, approve or reject with remarks
- Finance: view approved requests and update them to ordered/completed/cancelled

## Demo Credentials

- Employee: `employee@demo.com` / `employee`
- Manager: `manager@demo.com` / `manager`
- Finance: `finance@demo.com` / `finance`

## Deployment Targets

- Frontend: Vercel
- Backend: Render
- Database: Supabase PostgreSQL
- Repository: GitHub

## Deployment Notes

1. Run `server/database/seed-demo-data.sql` in Supabase SQL Editor after migrations.
2. Deploy the backend to Render first.
3. Set Render environment variables:

```text
ConnectionStrings__DefaultConnection=Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.lvjnsomiatgetxauyqhu;Password=...;SSL Mode=Require;Trust Server Certificate=true
Jwt__Key=replace-with-long-random-secret
Jwt__Issuer=portfolio-demo
Jwt__Audience=portfolio-demo
Cors__AllowedOrigins=https://your-vercel-app.vercel.app
ASPNETCORE_ENVIRONMENT=Production
```

4. After Render gives you the API URL, update `client/public/config.js`.
5. Deploy the Angular frontend to Vercel with `client` as the project root.
