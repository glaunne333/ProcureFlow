export type Role = 'Employee' | 'Manager' | 'Finance';

export interface CurrentUser {
  id: string;
  name: string;
  email: string;
  role: Role;
  departmentId: string;
}

export interface LoginResponse {
  accessToken: string;
  user: CurrentUser;
}

export interface Lookup {
  id: string;
  name: string;
}

export interface RequestListItem {
  id: string;
  requestNo: string;
  requestedBy: string;
  vendor: string;
  status: string;
  estimatedTotal: number;
  createdAt: string;
  submittedAt: string | null;
}

export interface RequestDetail extends RequestListItem {
  items: RequestItem[];
  approvalLogs: ApprovalLog[];
}

export interface RequestItem {
  description: string;
  quantity: number;
  unitCost: number;
  category: string;
  lineTotal: number;
}

export interface ApprovalLog {
  actor: string;
  fromStatus: string;
  toStatus: string;
  remarks: string | null;
  createdAt: string;
}

export interface DashboardSummary {
  statusCounts: { status: string; count: number }[];
  recentRequests: RequestListItem[];
}
