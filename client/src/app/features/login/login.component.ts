import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Role } from '../../core/models';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  error = '';
  isSubmitting = false;

  loginAs(role: Role): void {
    const email = `${role.toLowerCase()}@demo.com`;
    const password = role.toLowerCase();

    this.error = '';
    this.isSubmitting = true;

    this.api.login(email, password).subscribe({
      next: response => {
        this.auth.setSession(response);
        this.router.navigateByUrl('/');
      },
      error: error => {
        this.error = error.error?.message || 'Sign in failed. Check the demo credentials.';
        this.isSubmitting = false;
      }
    });
  }
}
