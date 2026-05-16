import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CurrentUserResponse,
  LoginRequest,
  LoginResponse
} from '../models/logiflow.models';

const ACCESS_TOKEN_KEY = 'logiflow.accessToken';
const USER_KEY = 'logiflow.user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = environment.apiBaseUrl;

  private readonly currentUserState = signal<LoginResponse | null>(this.readStoredUser());

  readonly currentUser = this.currentUserState.asReadonly();

  readonly isAuthenticated = computed(() => {
    const token = this.accessToken;
    const user = this.currentUserState();

    if (!token || !user) {
      return false;
    }

    return new Date(user.expiresAt).getTime() > Date.now();
  });

  readonly displayName = computed(() => {
    return this.currentUserState()?.fullName ?? 'Guest';
  });

  readonly role = computed(() => {
    return this.currentUserState()?.role ?? '';
  });

  get accessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, request).pipe(
      tap((response) => {
        localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
        localStorage.setItem(USER_KEY, JSON.stringify(response));
        this.currentUserState.set(response);
      })
    );
  }

  loadCurrentUser(): Observable<CurrentUserResponse> {
    return this.http.get<CurrentUserResponse>(`${this.baseUrl}/auth/me`);
  }

  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUserState.set(null);
    this.router.navigateByUrl('/login');
  }

  private readStoredUser(): LoginResponse | null {
    const raw = localStorage.getItem(USER_KEY);

    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as LoginResponse;

      if (!parsed.accessToken || !parsed.expiresAt) {
        this.clearStoredAuth();
        return null;
      }

      if (new Date(parsed.expiresAt).getTime() <= Date.now()) {
        this.clearStoredAuth();
        return null;
      }

      return parsed;
    } catch {
      this.clearStoredAuth();
      return null;
    }
  }

  private clearStoredAuth(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
