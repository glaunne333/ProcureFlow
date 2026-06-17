import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucidePlus, LucideTrash2 } from '@lucide/angular';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Lookup } from '../../core/models';

interface EditableItem {
  description: string;
  quantity: number;
  unitCost: number;
  category: string;
}

@Component({
  selector: 'app-request-form',
  imports: [CurrencyPipe, FormsModule, LucidePlus, LucideTrash2],
  templateUrl: './request-form.component.html'
})
export class RequestFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly vendors = signal<Lookup[]>([]);
  readonly error = signal('');
  readonly items = signal<EditableItem[]>([
    { description: '', quantity: 1, unitCost: 0, category: 'General' }
  ]);

  vendorId = '';
  isSubmitting = false;

  ngOnInit(): void {
    if (!this.auth.isSignedIn()) {
      this.router.navigateByUrl('/login');
      return;
    }

    if (this.auth.user()?.role !== 'Employee') {
      this.router.navigateByUrl('/');
      return;
    }

    this.api.vendors().subscribe({
      next: vendors => {
        this.vendors.set(vendors);
        this.vendorId = vendors[0]?.id || '';
      },
      error: error => this.error.set(error.error?.detail || 'Could not load vendors.')
    });
  }

  addItem(): void {
    this.items.update(items => [...items, { description: '', quantity: 1, unitCost: 0, category: 'General' }]);
  }

  removeItem(index: number): void {
    this.items.update(items => items.filter((_, itemIndex) => itemIndex !== index));
  }

  total(): number {
    return this.items().reduce((sum, item) => sum + item.quantity * item.unitCost, 0);
  }

  create(): void {
    this.error.set('');
    this.isSubmitting = true;

    this.api.createRequest({ vendorId: this.vendorId, items: this.items() }).subscribe({
      next: result => this.router.navigate(['/requests', result.id]),
      error: error => {
        this.error.set(error.error?.message || 'Could not create request.');
        this.isSubmitting = false;
      }
    });
  }
}
