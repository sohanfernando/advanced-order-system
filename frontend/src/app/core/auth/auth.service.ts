import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { API_BASE_URL } from '../config';
import { AuthUser, LoginRequest, RegisterRequest } from '../models';

export const AUTH_API_URL = `${API_BASE_URL}/auth`;

type AuthStatus = 'unknown' | 'authenticated' | 'anonymous';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly currentUser = signal<AuthUser | null>(null);
  private readonly status = signal<AuthStatus>('unknown');

  // Shared so parallel guard checks only call /me once
  private sessionCheck$: Observable<boolean> | null = null;

  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this.status() === 'authenticated');

  // Restores the session from the auth cookie the first time it is needed
  ensureSession(): Observable<boolean> {
    if (this.status() !== 'unknown') {
      return of(this.isAuthenticated());
    }

    this.sessionCheck$ ??= this.http.get<AuthUser>(`${AUTH_API_URL}/me`).pipe(
      map((user) => {
        this.setUser(user);
        return true;
      }),
      catchError(() => {
        this.clearSession();
        return of(false);
      }),
      finalize(() => (this.sessionCheck$ = null)),
      shareReplay(1),
    );

    return this.sessionCheck$;
  }

  login(request: LoginRequest): Observable<AuthUser> {
    return this.http
      .post<AuthUser>(`${AUTH_API_URL}/login`, request)
      .pipe(tap((user) => this.setUser(user)));
  }

  register(request: RegisterRequest): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${AUTH_API_URL}/register`, request);
  }

  // Always clears the local session, even if the request fails
  logout(): Observable<void> {
    return this.http.post<void>(`${AUTH_API_URL}/logout`, null).pipe(
      catchError(() => of(undefined)),
      finalize(() => this.clearSession()),
    );
  }

  clearSession(): void {
    this.currentUser.set(null);
    this.status.set('anonymous');
  }

  private setUser(user: AuthUser): void {
    this.currentUser.set(user);
    this.status.set('authenticated');
  }
}
