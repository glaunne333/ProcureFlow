import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DashboardSummary, LoginResponse, Lookup, RequestDetail, RequestListItem } from './models';

declare global {
  interface Window {
    procureFlowConfig?: {
      apiBaseUrl?: string;
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
  private readonly baseUrl = window.procureFlowConfig?.apiBaseUrl || environment.apiBaseUrl;

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/api/auth/login`, { email, password });
  }

  summary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.baseUrl}/api/dashboard/summary`);
  }

  requests(): Observable<RequestListItem[]> {
    return this.http.get<RequestListItem[]>(`${this.baseUrl}/api/requests`);
  }

  request(id: string): Observable<RequestDetail> {
    return this.http.get<RequestDetail>(`${this.baseUrl}/api/requests/${id}`);
  }

  vendors(): Observable<Lookup[]> {
    return this.http.get<Lookup[]>(`${this.baseUrl}/api/vendors`);
  }

  createRequest(payload: CreateRequestPayload): Observable<{ id: string; requestNo: string }> {
    return this.http.post<{ id: string; requestNo: string }>(`${this.baseUrl}/api/requests`, payload);
  }

  action(id: string, action: string, remarks: string | null): Observable<{ id: string; status: string }> {
    return this.http.post<{ id: string; status: string }>(
      `${this.baseUrl}/api/requests/${id}/${action}`,
      { remarks }
    );
  }
}
