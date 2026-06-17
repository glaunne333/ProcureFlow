# ProcureFlow

ProcureFlow is a small procurement request and approval system built for a portfolio project. The goal is to show one realistic enterprise workflow without overbuilding: employees submit purchase requests, managers approve or reject them, and finance marks approved requests as ordered or completed.

The live demo is intentionally frontend-only so it can run on Vercel without paid backend hosting. The repository remains backend-ready with a .NET 10 API, EF Core PostgreSQL migrations, Docker deployment files, and Supabase seed scripts.

## Small-scope MVP

- Demo login with seeded users for Employee, Manager, and Finance roles
- Purchase request creation with line items
- Status workflow: Draft -> Submitted -> Approved or Rejected -> Ordered -> Completed or Cancelled
- Approval log for each workflow action
- Compact dashboard/request list with summary counts
- Angular + Tailwind frontend
- .NET 10 Web API backend
- Supabase PostgreSQL database
- Vercel frontend demo deployment
- Backend Docker deployment-ready when a host is available

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
- Backend: Docker-capable host that allows outbound PostgreSQL connections
- Database: Supabase PostgreSQL
- Repository: GitHub

## Live Demo Mode

The Angular app can run as a frontend-only Vercel demo using `client/public/config.js`.

```js
window.procureFlowConfig = {
  apiBaseUrl: '',
  useMockApi: true
};
```

In this mode, demo users, vendors, and workflow requests are simulated in the browser. The seeded mock data includes draft, submitted, approved, ordered, completed, and rejected requests so every demo role has something useful to do. No backend or database is required for the live UI demo.

## Backend-Ready Path

- .NET 10 Web API with JWT login and request workflow endpoints
- Role-based workflow rules for Employee, Manager, and Finance
- EF Core PostgreSQL mappings and migrations
- Supabase SQL seed data for demo users, vendors, and requests
- Dockerfiles for deploying the API later

## Deployment Notes

1. For the frontend-only demo, deploy only the `client` folder to Vercel.
2. For the full-stack version, run `server/database/seed-demo-data.sql` in Supabase SQL Editor after migrations.
3. Deploy the backend first.
4. Set backend environment variables:

```text
ConnectionStrings__DefaultConnection=Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.lvjnsomiatgetxauyqhu;Password=...;SSL Mode=Require;Trust Server Certificate=true
Jwt__Key=replace-with-long-random-secret
Jwt__Issuer=portfolio-demo
Jwt__Audience=portfolio-demo
Cors__AllowedOrigins=https://your-vercel-app.vercel.app
ASPNETCORE_ENVIRONMENT=Production
```

5. After the backend gives you the API URL, update `client/public/config.js`.
6. Deploy the Angular frontend to Vercel with `client` as the project root.

If Render cannot be used, deploy the root `Dockerfile` to another Docker-capable host. The container listens on the `PORT` environment variable, or `7860` by default.
