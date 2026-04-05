import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, catchError, map, tap } from 'rxjs';

const AUTH_TOKEN_KEY = 'cloudkeep_token';
/** Solo si el usuario marcó «Recordar contraseña»; se borra al cerrar sesión. */
const AUTH_SAVED_PASSWORD_KEY = 'cloudkeep_saved_password';
const API_LOGIN = '/api/auth/login';
const API_CHANGE_PASSWORD = '/api/auth/change-password';
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
  private tokenSignal = signal<string | null>(this.readTokenFromStorages());
  private requireAuthSignal = signal<boolean>(true);

  isLoggedIn = computed(() => !!this.tokenSignal());

  /** Si true, las rutas protegidas exigen login. Si false, se puede entrar sin contraseña. */
  requireAuth = this.requireAuthSignal.asReadonly();

  constructor() {
    const t = this.readTokenFromStorages();
    if (t) this.tokenSignal.set(t);
  }

  private readTokenFromStorages(): string | null {
    return sessionStorage.getItem(AUTH_TOKEN_KEY) ?? localStorage.getItem(AUTH_TOKEN_KEY);
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
    return this.tokenSignal() ?? this.readTokenFromStorages();
  }

  /**
   * Comprueba el claim `exp` del JWT (sin validar firma).
   * Si el token no es un JWT con `exp` numérico, se considera vencido.
   */
  isAccessTokenExpired(token: string): boolean {
    const exp = this.readJwtExpSeconds(token);
    if (exp === null) return true;
    return Date.now() / 1000 >= exp;
  }

  private readJwtExpSeconds(token: string): number | null {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    try {
      let b64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const pad = b64.length % 4;
      if (pad) b64 += '='.repeat(4 - pad);
      const payload = JSON.parse(atob(b64)) as { exp?: unknown };
      return typeof payload.exp === 'number' ? payload.exp : null;
    } catch {
      return null;
    }
  }

  /**
   * Contraseña recordada para rellenar el formulario de login (solo si hubo login exitoso con «Recordar contraseña»).
   */
  getSavedPasswordForLoginForm(): string | null {
    return localStorage.getItem(AUTH_SAVED_PASSWORD_KEY);
  }

  /**
   * POST /api/auth/login. Si remember es true, el token y la contraseña persisten en localStorage (sesión entre cierres del navegador).
   * Si es false, el token va a sessionStorage y no se guarda la contraseña.
   */
  login(password: string, remember: boolean): Observable<string> {
    return this.http.post<ApiResponseWrapper<string> | string>(API_LOGIN, { password }).pipe(
      map((res) => (res as ApiResponseWrapper<string>)?.details ?? (typeof res === 'string' ? res : '')),
      tap((token) => {
        localStorage.removeItem(AUTH_TOKEN_KEY);
        sessionStorage.removeItem(AUTH_TOKEN_KEY);
        if (remember) {
          localStorage.setItem(AUTH_TOKEN_KEY, token);
          localStorage.setItem(AUTH_SAVED_PASSWORD_KEY, password);
        } else {
          sessionStorage.setItem(AUTH_TOKEN_KEY, token);
          localStorage.removeItem(AUTH_SAVED_PASSWORD_KEY);
        }
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
    sessionStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(AUTH_SAVED_PASSWORD_KEY);
    this.tokenSignal.set(null);
  }
}
