# Frontend-Only Portfolio Demo Playbook

Use this when you want a small, deployable portfolio project that looks like a real system without getting blocked by backend hosting, database networking, payment cards, or firewall issues.

## Goal

Build a working live demo that recruiters can click through immediately.

The deployed version can be frontend-only, but the repository should still show that the project is backend-ready through clean structure, mock API boundaries, documentation, and optional backend files.

## Scope Rules

- Pick one realistic workflow, not a full platform.
- Use 3 demo roles at most.
- Use seeded mock data instead of admin screens.
- Make every role have at least one useful action.
- Avoid file uploads, payments, notifications, analytics, and complex settings.
- Prefer obvious UI over clever UI.
- Keep the live demo easy to deploy on Vercel.

## Recommended Stack

- Frontend: Angular
- Styling: Tailwind or simple global CSS
- Icons: `@lucide/angular`
- Live deployment: Vercel
- Backend-ready layer: .NET API, Express API, Laravel API, or any backend stack you want to show
- Database-ready layer: Supabase/PostgreSQL schema or seed scripts

The deployed demo does not need to connect to the backend if hosting becomes a blocker.

## Build Order

1. Define the workflow.
2. Define the demo roles.
3. Scaffold the frontend.
4. Add a mock API service.
5. Add seeded data covering every status.
6. Add role-button login.
7. Build the main workflow screens.
8. Add clear action buttons for each role.
9. Add backend-ready documentation.
10. Deploy only the frontend to Vercel.

## Example Workflow Shape

For a procurement project:

- Employee creates a draft request.
- Employee submits the request.
- Manager approves or rejects it.
- Finance marks approved requests as ordered.
- Finance marks ordered requests as completed.
- Employee or Finance can cancel requests when appropriate.

For another project, use the same pattern:

- Creator role starts the record.
- Reviewer role approves or rejects it.
- Operations/Admin role finishes the process.

## Mock Login Pattern

Do not make users type credentials in the live demo. Use role buttons.

Example:

```text
Continue as Employee
Continue as Manager
Continue as Finance
```

Behind the scenes, map each button to a demo user:

```ts
loginAs(role: Role): void {
  const email = `${role.toLowerCase()}@demo.com`;
  const password = role.toLowerCase();
  this.api.login(email, password).subscribe(...);
}
```

This keeps the demo fast and prevents visitors from getting stuck.

## Mock Data Rules

Seed enough records so the app feels alive immediately.

Include:

- At least one draft item for the creator role.
- At least one submitted item for the reviewer role.
- At least one approved item for the operations/admin role.
- At least one in-progress item for the operations/admin role.
- At least one completed item.
- At least one rejected or cancelled item.

For each record, include:

- ID
- Reference number
- Requester/owner
- Vendor/customer/person/entity name
- Status
- Total amount or priority
- Created date
- Line items or detail rows
- Timeline/history logs

Use localStorage for a simple interactive demo:

```ts
const mockDataKey = 'project_mock_data';
const mockDataVersionKey = 'project_mock_data_version';
const mockDataVersion = '1';
```

When you change the seed data, bump the version so old browser data refreshes.

## Role Action Queue

Add an action queue on the main list/dashboard.

This solves the problem where users do not know which item to click.

Example labels:

- Employee: `Submit draft`
- Manager: `Review`
- Finance: `Issue order`
- Finance: `Receive order`

The detail page should still contain the actual action buttons.

## Currency And Local Context

Do not leave Angular's default currency pipe if the project is not using USD.

For Philippine peso amounts:

```html
{{ amount | currency: 'PHP' : 'symbol' : '1.2-2' }}
```

Also make the mock amounts realistic for the currency. Do not just change the symbol.

## Frontend-Only Config

Add a runtime config file such as:

```js
window.projectConfig = {
  apiBaseUrl: '',
  useMockApi: true
};
```

The API service should choose mock mode when `useMockApi` is true.

This lets the same frontend later point to a real backend by changing config.

## Backend-Ready Messaging

Be honest in the UI and README.

Good wording:

```text
The live Vercel demo runs entirely in the browser with seeded mock data, so it does not need paid backend hosting.
The repository still includes API structure, database scripts, and deployment files for a backend-ready version.
```

Avoid pretending the deployed demo has a live database if it does not.

## README Checklist

The README should include:

- What the app does
- Demo roles
- Live demo mode explanation
- Backend-ready explanation
- Deployment settings
- What is intentionally out of scope

Example Vercel settings:

```text
Root Directory: client
Build Command: npm run build
Output Directory: dist/client/browser
Framework Preset: Angular
```

## Vercel Update Flow

After making changes:

```powershell
git add .
git commit -m "Update frontend demo"
git push
```

Vercel should redeploy automatically from GitHub.

If it does not:

1. Open the project in Vercel.
2. Go to Deployments.
3. Click Redeploy on the latest deployment.

## Final Verification

Before calling the project ready:

- Angular build passes.
- Login buttons work.
- Every role has at least one visible action.
- Currency/local labels are correct.
- Mock data refreshes after seed changes.
- README says whether the live demo is frontend-only.
- Vercel deploy settings are documented.

## Portfolio Rule Of Thumb

The goal is not to prove you built an enterprise system.

The goal is to prove you can design a clean workflow, separate concerns, make deployment tradeoffs, document honestly, and ship something people can actually use.
