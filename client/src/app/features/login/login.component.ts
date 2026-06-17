import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = 'employee@demo.com';
  password = 'employee';
  error = '';
  isSubmitting = false;

  login(): void {
    this.error = '';
    this.isSubmitting = true;

    this.api.login(this.email, this.password).subscribe({
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
