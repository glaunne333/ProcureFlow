import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ApprovalLog,
  CurrentUser,
  DashboardSummary,
  LoginResponse,
  Lookup,
  RequestDetail,
  RequestItem,
  RequestListItem
} from './models';
import { AuthService } from './auth.service';

declare global {
  interface Window {
    procureFlowConfig?: {
      apiBaseUrl?: string;
      useMockApi?: boolean;
    };
  }
}

export interface CreateRequestPayload {
  vendorId: string;
  items: {
    description: string;
    quantity: number;
    unitCost: number;
    category: string;
  }[];
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly baseUrl = window.procureFlowConfig?.apiBaseUrl || environment.apiBaseUrl;
  private readonly useMockApi = window.procureFlowConfig?.useMockApi ?? !this.baseUrl;

  login(email: string, password: string): Observable<LoginResponse> {
    if (this.useMockApi) {
      return this.mockLogin(email, password);
    }

    return this.http.post<LoginResponse>(`${this.baseUrl}/api/auth/login`, { email, password });
  }

  summary(): Observable<DashboardSummary> {
    if (this.useMockApi) {
      const requests = this.visibleRequests();
      const statusCounts = [...new Set(requests.map(request => request.status))]
        .map(status => ({ status, count: requests.filter(request => request.status === status).length }));

      return of({
        statusCounts,
        recentRequests: requests.slice(0, 5)
      });
    }

    return this.http.get<DashboardSummary>(`${this.baseUrl}/api/dashboard/summary`);
  }

  requests(): Observable<RequestListItem[]> {
    if (this.useMockApi) {
      return of(this.visibleRequests());
    }

    return this.http.get<RequestListItem[]>(`${this.baseUrl}/api/requests`);
  }

  request(id: string): Observable<RequestDetail> {
    if (this.useMockApi) {
      const request = this.visibleRequestDetails().find(item => item.id === id);
      return request ? of(request) : throwError(() => ({ error: { detail: 'Request was not found.' } }));
    }

    return this.http.get<RequestDetail>(`${this.baseUrl}/api/requests/${id}`);
  }

  vendors(): Observable<Lookup[]> {
    if (this.useMockApi) {
      return of(mockVendors);
    }

    return this.http.get<Lookup[]>(`${this.baseUrl}/api/vendors`);
  }

  createRequest(payload: CreateRequestPayload): Observable<{ id: string; requestNo: string }> {
    if (this.useMockApi) {
      const user = this.auth.user();

      if (!user || user.role !== 'Employee') {
        return throwError(() => ({ error: { message: 'Only employees can create requests.' } }));
      }

      const vendor = mockVendors.find(item => item.id === payload.vendorId);

      if (!vendor) {
        return throwError(() => ({ error: { message: 'Vendor was not found.' } }));
      }

      const id = crypto.randomUUID();
      const items = payload.items.map(item => ({
        ...item,
        lineTotal: item.quantity * item.unitCost
      }));
      const request: StoredRequest = {
        id,
        requestNo: `PR-${new Date().toISOString().replace(/\D/g, '').slice(0, 14)}`,
        requestedById: user.id,
        requestedBy: user.name,
        departmentId: user.departmentId,
        vendorId: vendor.id,
        vendor: vendor.name,
        status: 'Draft',
        estimatedTotal: items.reduce((sum, item) => sum + item.lineTotal, 0),
        createdAt: new Date().toISOString(),
        submittedAt: null,
        items,
        approvalLogs: []
      };

      this.saveRequests([request, ...this.loadRequests()]);
      return of({ id: request.id, requestNo: request.requestNo });
    }

    return this.http.post<{ id: string; requestNo: string }>(`${this.baseUrl}/api/requests`, payload);
  }

  action(id: string, action: string, remarks: string | null): Observable<{ id: string; status: string }> {
    if (this.useMockApi) {
      const user = this.auth.user();
      const requests = this.loadRequests();
      const request = requests.find(item => item.id === id);

      if (!user || !request) {
        return throwError(() => ({ error: { message: 'Request was not found.' } }));
      }

      let nextStatus: string;

      try {
        nextStatus = this.nextStatus(request.status, action, user, remarks);
      } catch (error) {
        return throwError(() => error);
      }

      request.approvalLogs.push({
        actor: user.name,
        fromStatus: request.status,
        toStatus: nextStatus,
        remarks,
        createdAt: new Date().toISOString()
      });
      request.status = nextStatus;
      request.submittedAt = nextStatus === 'Submitted' ? new Date().toISOString() : request.submittedAt;
      this.saveRequests(requests);

      return of({ id, status: request.status });
    }

    return this.http.post<{ id: string; status: string }>(
      `${this.baseUrl}/api/requests/${id}/${action}`,
      { remarks }
    );
  }

  private mockLogin(email: string, password: string): Observable<LoginResponse> {
    const normalizedEmail = email.trim().toLowerCase();
    const user = mockUsers.find(item => item.email === normalizedEmail && item.password === password);

    if (!user) {
      return throwError(() => ({ error: { message: 'Sign in failed. Check the demo credentials.' } }));
    }

    return of({
      accessToken: `mock-token-${user.id}`,
      user: {
        id: user.id,
        name: user.name,
        email: user.email,
        role: user.role,
        departmentId: user.departmentId
      }
    });
  }

  private visibleRequests(): RequestListItem[] {
    return this.visibleRequestDetails().map(request => ({
      id: request.id,
      requestNo: request.requestNo,
      requestedBy: request.requestedBy,
      vendor: request.vendor,
      status: request.status,
      estimatedTotal: request.estimatedTotal,
      createdAt: request.createdAt,
      submittedAt: request.submittedAt
    }));
  }

  private visibleRequestDetails(): StoredRequest[] {
    const user = this.auth.user();
    const requests = this.loadRequests();

    if (!user) {
      return [];
    }

    if (user.role === 'Employee') {
      return requests.filter(request => request.requestedById === user.id);
    }

    return requests;
  }

  private loadRequests(): StoredRequest[] {
    const savedVersion = localStorage.getItem(mockRequestsVersionKey);
    const saved = localStorage.getItem(mockRequestsKey);

    if (saved && savedVersion === mockRequestsVersion) {
      return JSON.parse(saved);
    }

    localStorage.setItem(mockRequestsKey, JSON.stringify(seedRequests));
    localStorage.setItem(mockRequestsVersionKey, mockRequestsVersion);
    return seedRequests;
  }

  private saveRequests(requests: StoredRequest[]): void {
    localStorage.setItem(mockRequestsKey, JSON.stringify(requests));
  }

  private nextStatus(status: string, action: string, user: CurrentUser, remarks: string | null): string {
    if (user.role === 'Employee' && status === 'Draft' && action === 'submit') {
      return 'Submitted';
    }

    if (user.role === 'Manager' && status === 'Submitted' && action === 'approve') {
      return 'Approved';
    }

    if (user.role === 'Manager' && status === 'Submitted' && action === 'reject') {
      if (!remarks?.trim()) {
        throw { error: { message: 'Rejected requests require remarks.' } };
      }

      return 'Rejected';
    }

    if (user.role === 'Finance' && status === 'Approved' && action === 'order') {
      return 'Ordered';
    }

    if (user.role === 'Finance' && status === 'Ordered' && action === 'complete') {
      return 'Completed';
    }

    if (user.role === 'Employee' && status === 'Draft' && action === 'cancel') {
      return 'Cancelled';
    }

    if (user.role === 'Finance' && (status === 'Approved' || status === 'Ordered') && action === 'cancel') {
      return 'Cancelled';
    }

    throw { error: { message: 'That action is not available for this role or status.' } };
  }
}

interface StoredRequest extends RequestDetail {
  requestedById: string;
  departmentId: string;
  vendorId: string;
}

const mockRequestsKey = 'procureflow_mock_requests';
const mockRequestsVersionKey = 'procureflow_mock_requests_version';
const mockRequestsVersion = '4';

const mockUsers: (CurrentUser & { password: string })[] = [
  {
    id: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    name: 'Employee Demo',
    email: 'employee@demo.com',
    password: 'employee',
    role: 'Employee',
    departmentId: '22222222-2222-2222-2222-222222222222'
  },
  {
    id: '99999999-9999-9999-9999-999999999999',
    name: 'Manager Demo',
    email: 'manager@demo.com',
    password: 'manager',
    role: 'Manager',
    departmentId: '22222222-2222-2222-2222-222222222222'
  },
  {
    id: 'ffffffff-ffff-ffff-ffff-ffffffffffff',
    name: 'Finance Demo',
    email: 'finance@demo.com',
    password: 'finance',
    role: 'Finance',
    departmentId: '11111111-1111-1111-1111-111111111111'
  }
];

const mockVendors: Lookup[] = [
  { id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', name: 'Acme Office Supplies' },
  { id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', name: 'Northwind Hardware' },
  { id: 'cccccccc-cccc-cccc-cccc-cccccccccccc', name: 'Contoso IT Services' }
];

const seedRequests: StoredRequest[] = [
  {
    id: '44444444-4444-4444-4444-444444444444',
    requestNo: 'PR-202606170001',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    vendor: 'Contoso IT Services',
    status: 'Draft',
    estimatedTotal: 77500,
    createdAt: daysAgo(1),
    submittedAt: null,
    items: [
      makeItem('Laptop', 1, 68000, 'Hardware'),
      makeItem('Docking station', 1, 9500, 'Hardware')
    ],
    approvalLogs: []
  },
  {
    id: '55555555-5555-5555-5555-555555555555',
    requestNo: 'PR-202606170002',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    vendor: 'Acme Office Supplies',
    status: 'Submitted',
    estimatedTotal: 29880,
    createdAt: daysAgo(3),
    submittedAt: daysAgo(2),
    items: [
      makeItem('Ergonomic chairs', 4, 7200, 'Furniture'),
      makeItem('Whiteboard markers', 6, 180, 'Office Supplies')
    ],
    approvalLogs: [
      makeLog('Employee Demo', 'Draft', 'Submitted', 'Replacing worn office seating.', daysAgo(2))
    ] satisfies ApprovalLog[]
  },
  {
    id: '66666666-6666-6666-6666-666666666666',
    requestNo: 'PR-202606170003',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    vendor: 'Northwind Hardware',
    status: 'Approved',
    estimatedTotal: 129500,
    createdAt: daysAgo(5),
    submittedAt: daysAgo(4),
    items: [
      makeItem('Barcode scanners', 3, 24500, 'Hardware'),
      makeItem('Label printer', 1, 56000, 'Hardware')
    ],
    approvalLogs: [
      makeLog('Employee Demo', 'Draft', 'Submitted', 'Needed for receiving desk rollout.', daysAgo(4)),
      makeLog('Manager Demo', 'Submitted', 'Approved', 'Approved within team budget.', daysAgo(3))
    ] satisfies ApprovalLog[]
  },
  {
    id: '77777777-7777-7777-7777-777777777777',
    requestNo: 'PR-202606170004',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    vendor: 'Contoso IT Services',
    status: 'Ordered',
    estimatedTotal: 48000,
    createdAt: daysAgo(8),
    submittedAt: daysAgo(7),
    items: [
      makeItem('Cloud backup setup', 1, 48000, 'Software')
    ],
    approvalLogs: [
      makeLog('Employee Demo', 'Draft', 'Submitted', 'Covers procurement document backup.', daysAgo(7)),
      makeLog('Manager Demo', 'Submitted', 'Approved', 'Operationally necessary.', daysAgo(6)),
      makeLog('Finance Demo', 'Approved', 'Ordered', 'Purchase order issued.', daysAgo(5))
    ] satisfies ApprovalLog[]
  },
  {
    id: '12121212-1212-1212-1212-121212121212',
    requestNo: 'PR-202606170007',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    vendor: 'Northwind Hardware',
    status: 'Approved',
    estimatedTotal: 42500,
    createdAt: daysAgo(6),
    submittedAt: daysAgo(5),
    items: [
      makeItem('UPS battery backups', 5, 8500, 'Hardware')
    ],
    approvalLogs: [
      makeLog('Employee Demo', 'Draft', 'Submitted', 'Keeps workstations protected during outages.', daysAgo(5)),
      makeLog('Manager Demo', 'Submitted', 'Approved', 'Approved for continuity work.', daysAgo(4))
    ] satisfies ApprovalLog[]
  },
  {
    id: '34343434-3434-3434-3434-343434343434',
    requestNo: 'PR-202606170008',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    vendor: 'Contoso IT Services',
    status: 'Ordered',
    estimatedTotal: 62000,
    createdAt: daysAgo(9),
    submittedAt: daysAgo(8),
    items: [
      makeItem('Network cabling service', 1, 62000, 'Services')
    ],
    approvalLogs: [
      makeLog('Employee Demo', 'Draft', 'Submitted', 'Needed for the operations room move.', daysAgo(8)),
      makeLog('Manager Demo', 'Submitted', 'Approved', 'Approved for site readiness.', daysAgo(7)),
      makeLog('Finance Demo', 'Approved', 'Ordered', 'Service order sent to vendor.', daysAgo(6))
    ] satisfies ApprovalLog[]
  },
  {
    id: '88888888-8888-8888-8888-888888888888',
    requestNo: 'PR-202606170005',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    vendor: 'Acme Office Supplies',
    status: 'Completed',
    estimatedTotal: 10800,
    createdAt: daysAgo(12),
    submittedAt: daysAgo(11),
    items: [
      makeItem('Desk lamps', 6, 1800, 'Office Supplies')
    ],
    approvalLogs: [
      makeLog('Employee Demo', 'Draft', 'Submitted', 'For finance workstations.', daysAgo(11)),
      makeLog('Manager Demo', 'Submitted', 'Approved', 'Approved for workspace refresh.', daysAgo(10)),
      makeLog('Finance Demo', 'Approved', 'Ordered', 'Vendor confirmed order.', daysAgo(9)),
      makeLog('Finance Demo', 'Ordered', 'Completed', 'Items received.', daysAgo(6))
    ] satisfies ApprovalLog[]
  },
  {
    id: '99999999-1111-4444-8888-999999999999',
    requestNo: 'PR-202606170006',
    requestedById: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    requestedBy: 'Employee Demo',
    departmentId: '22222222-2222-2222-2222-222222222222',
    vendorId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    vendor: 'Northwind Hardware',
    status: 'Rejected',
    estimatedTotal: 96000,
    createdAt: daysAgo(15),
    submittedAt: daysAgo(14),
    items: [
      makeItem('Standing desks', 3, 32000, 'Furniture')
    ],
    approvalLogs: [
      makeLog('Employee Demo', 'Draft', 'Submitted', 'Requested for optional workspace upgrades.', daysAgo(14)),
      makeLog('Manager Demo', 'Submitted', 'Rejected', 'Defer until next quarter budget review.', daysAgo(13))
    ] satisfies ApprovalLog[]
  }
];

function makeItem(description: string, quantity: number, unitCost: number, category: string): RequestItem {
  return {
    description,
    quantity,
    unitCost,
    category,
    lineTotal: quantity * unitCost
  };
}

function makeLog(actor: string, fromStatus: string, toStatus: string, remarks: string, createdAt: string): ApprovalLog {
  return {
    actor,
    fromStatus,
    toStatus,
    remarks,
    createdAt
  };
}

function daysAgo(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toISOString();
}
