import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { LucideList, LucideLogOut, LucidePlus, LucideWorkflow } from '@lucide/angular';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  imports: [LucideList, LucideLogOut, LucidePlus, LucideWorkflow, RouterLink, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = computed(() => this.auth.user());

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
