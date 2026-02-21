import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, catchError, map, tap } from 'rxjs';

const AUTH_TOKEN_KEY = 'cloudkeep_token';
const API_LOGIN = '/api/login';
const API_CHANGE_PASSWORD = '/api/user/change-password';
const API_REQUIRE_AUTH_GET = '/api/settings/require-auth';
const API_REQUIRE_AUTH_PUT = '/api/settings/require-auth';

export interface RequireAuthResponse {
  requireAuth: boolean;
}

/** Respuesta envuelta por el middleware ApiResponse */
export interface ApiResponseWrapper<T> {
  details?: T;
  message?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private tokenSignal = signal<string | null>(this.getStoredToken());
  private requireAuthSignal = signal<boolean>(true);

  isLoggedIn = computed(() => !!this.tokenSignal());

  /** Si true, las rutas protegidas exigen login. Si false, se puede entrar sin contraseña. */
  requireAuth = this.requireAuthSignal.asReadonly();

  constructor() {
    const t = localStorage.getItem(AUTH_TOKEN_KEY);    
    if (t) this.tokenSignal.set(t);
  }

  /** Carga el valor desde el API (sin auth). Llamar al arranque de la app. */
  loadRequireAuthFromApi(): Observable<boolean> {
    return this.http.get<ApiResponseWrapper<RequireAuthResponse> | RequireAuthResponse>(API_REQUIRE_AUTH_GET).pipe(
      map((res) => {
        const value = (res as ApiResponseWrapper<RequireAuthResponse>).details?.requireAuth
          ?? (res as RequireAuthResponse).requireAuth ?? true;
        this.requireAuthSignal.set(value);
        return value;
      }),
      catchError(() => {
        this.requireAuthSignal.set(true);
        return of(true);
      })
    );
  }

  /** Actualiza el valor en el API (requiere JWT) y localmente. */
  setRequireAuth(value: boolean): Observable<void> {
    return this.http.put<void>(API_REQUIRE_AUTH_PUT, { requireAuth: value }).pipe(
      tap(() => this.requireAuthSignal.set(value))
    );
  }

  getToken(): string | null {
    return this.tokenSignal() ?? localStorage.getItem(AUTH_TOKEN_KEY);
  }

  private getStoredToken(): string | null {    
    return localStorage.getItem(AUTH_TOKEN_KEY);
  }

  /** POST /api/login con { password }. El API devuelve { message, details } con el token en details. */
  login(password: string): Observable<string> {
    return this.http.post<ApiResponseWrapper<string> | string>(API_LOGIN, { password }).pipe(
      map((res) => (res as ApiResponseWrapper<string>)?.details ?? (typeof res === 'string' ? res : '')),
      tap((token) => {
        localStorage.setItem(AUTH_TOKEN_KEY, token);
        this.tokenSignal.set(token);
      })
    );
  }

  /** PUT /api/user/change-password (requiere JWT). Body: { currentPassword, newPassword }. */
  changePassword(currentPassword: string, newPassword: string): Observable<void> {    
    return this.http.put<void>(API_CHANGE_PASSWORD, {
      currentPassword,
      newPassword,
    });
  }

  logout(): void {
    localStorage.removeItem(AUTH_TOKEN_KEY);
    this.tokenSignal.set(null);
  }
}
