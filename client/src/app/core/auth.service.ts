import { computed, Injectable, signal } from '@angular/core';
import { LoginResponse, CurrentUser } from './models';

const tokenKey = 'procureflow_token';
const userKey = 'procureflow_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(localStorage.getItem(tokenKey));
  private readonly userSignal = signal<CurrentUser | null>(
    JSON.parse(localStorage.getItem(userKey) || 'null')
  );

  readonly token = computed(() => this.tokenSignal());
  readonly user = computed(() => this.userSignal());
  readonly isSignedIn = computed(() => Boolean(this.tokenSignal()));

  setSession(response: LoginResponse): void {
    this.tokenSignal.set(response.accessToken);
    this.userSignal.set(response.user);
    localStorage.setItem(tokenKey, response.accessToken);
    localStorage.setItem(userKey, JSON.stringify(response.user));
  }

  logout(): void {
    this.tokenSignal.set(null);
    this.userSignal.set(null);
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
  }
}
