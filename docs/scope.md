# ProcureFlow Scope

## Build

- Login with seeded demo accounts
- Dashboard/request list with status filter and role-based visibility
- Request creation form with line items
- Request details page with timeline and actions
- Manager approval filter/actions
- Summary counts and recent activity
- Vendors and categories as seeded lookup data

## Skip

- Vendor quotation uploads
- Full document management
- Multi-level approval routing
- Payment, invoicing, or accounting integrations
- Advanced analytics
- Event sourcing or message queues
- Complex admin settings
- Vendor/category CRUD
- Email notifications
- Search beyond a simple status filter
- Pagination unless the seeded demo data becomes awkward to display

## Core Screens

1. Login
2. Dashboard + requests list
3. Create request
4. Request details + approval/finance actions

The approval queue can be a filtered view of the request list instead of a separate screen unless the UI becomes clearer with a dedicated route.

## Core Tables

1. Users
2. Departments
3. Vendors
4. ProcurementRequests
5. RequestItems
6. ApprovalLogs

## Success Criteria

- A visitor can log in as each seeded role and complete the full request lifecycle.
- Invalid status transitions are blocked by backend business rules.
- Each status change creates an approval log entry.
- The deployed frontend can call the deployed API against Supabase.
- The README explains the demo credentials, workflow, stack, and deployment links.
