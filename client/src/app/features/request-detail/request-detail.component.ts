import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideCheck, LucidePackageCheck, LucideSend, LucideX } from '@lucide/angular';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { RequestDetail } from '../../core/models';

@Component({
  selector: 'app-request-detail',
  imports: [CurrencyPipe, DatePipe, FormsModule, LucideCheck, LucidePackageCheck, LucideSend, LucideX],
  templateUrl: './request-detail.component.html'
})
export class RequestDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly user = computed(() => this.auth.user());
  readonly request = signal<RequestDetail | null>(null);
  readonly error = signal('');

  remarks = '';

  ngOnInit(): void {
    if (!this.auth.isSignedIn()) {
      this.router.navigateByUrl('/login');
      return;
    }

    this.load();
  }

  load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigateByUrl('/');
      return;
    }

    this.api.request(id).subscribe({
      next: request => this.request.set(request),
      error: error => this.error.set(error.error?.detail || 'Could not load request.')
    });
  }

  run(action: string): void {
    const request = this.request();

    if (!request) {
      return;
    }

    this.error.set('');
    this.api.action(request.id, action, this.remarks || null).subscribe({
      next: () => {
        this.remarks = '';
        this.load();
      },
      error: error => this.error.set(error.error?.message || 'Action failed.')
    });
  }

  canSubmit(request: RequestDetail): boolean {
    return this.user()?.role === 'Employee' && request.status === 'Draft';
  }

  canReview(request: RequestDetail): boolean {
    return this.user()?.role === 'Manager' && request.status === 'Submitted';
  }

  canOrder(request: RequestDetail): boolean {
    return this.user()?.role === 'Finance' && request.status === 'Approved';
  }

  canComplete(request: RequestDetail): boolean {
    return this.user()?.role === 'Finance' && request.status === 'Ordered';
  }

  canCancel(request: RequestDetail): boolean {
    const role = this.user()?.role;
    return (role === 'Employee' && request.status === 'Draft')
      || (role === 'Finance' && (request.status === 'Approved' || request.status === 'Ordered'));
  }
}
