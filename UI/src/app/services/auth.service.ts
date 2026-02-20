import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

const AUTH_TOKEN_KEY = 'cloudkeep_token';
const API_LOGIN = '/api/login';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private tokenSignal = signal<string | null>(this.getStoredToken());

  isLoggedIn = computed(() => !!this.tokenSignal());

  constructor() {
    const t = localStorage.getItem(AUTH_TOKEN_KEY);
    if (t) this.tokenSignal.set(t);
  }

  getToken(): string | null {
    return this.tokenSignal() ?? localStorage.getItem(AUTH_TOKEN_KEY);
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(AUTH_TOKEN_KEY);
  }

  /** POST /api/login con { password }. El API devuelve el token JWT como string en el body. */
  login(password: string): Observable<string> {
    return this.http.post<string>(API_LOGIN, { password }).pipe(
      tap((token) => {
        localStorage.setItem(AUTH_TOKEN_KEY, token);
        this.tokenSignal.set(token);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(AUTH_TOKEN_KEY);
    this.tokenSignal.set(null);
  }
}
