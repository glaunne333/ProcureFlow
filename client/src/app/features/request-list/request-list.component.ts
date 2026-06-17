import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { LucidePlus } from '@lucide/angular';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { DashboardSummary, RequestListItem } from '../../core/models';

@Component({
  selector: 'app-request-list',
  imports: [CurrencyPipe, LucidePlus, RouterLink],
  templateUrl: './request-list.component.html'
})
export class RequestListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = computed(() => this.auth.user());
  readonly summary = signal<DashboardSummary | null>(null);
  readonly requests = signal<RequestListItem[]>([]);
  readonly error = signal('');

  ngOnInit(): void {
    if (!this.auth.isSignedIn()) {
      this.router.navigateByUrl('/login');
      return;
    }

    forkJoin({
      summary: this.api.summary(),
      requests: this.api.requests()
    }).subscribe({
      next: result => {
        this.summary.set(result.summary);
        this.requests.set(result.requests);
      },
      error: error => this.error.set(error.error?.detail || 'Could not load requests.')
    });
  }
}
