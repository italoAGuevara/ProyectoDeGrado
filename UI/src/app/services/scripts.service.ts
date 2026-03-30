import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { unwrapApiDetails } from '../utils/api-response.util';
import { ToastService } from './toast.service';

export type ScriptWhen = 'pre' | 'post';

/** Solo se permiten scripts .ps1, .bat y .js */
export type ScriptType = 'ps1' | 'bat' | 'js';

export interface CopyScript {
  id: string;
  name: string;
  scriptType: ScriptType;
  scriptPath: string;
  arguments: string;
  /** El API no persiste activo; se deja en true para compatibilidad con la UI. */
  enabled: boolean;
}

interface ScriptApiDto {
  id: number;
  nombre: string;
  scriptPath: string;
  arguments: string;
  tipo: string;
}

const API_SCRIPTS = '/api/scripts';

@Injectable({
  providedIn: 'root',
})
export class ScriptsService {
  private readonly http = inject(HttpClient);
  private readonly toast = inject(ToastService);

  /** Lista cargada desde el API. */
  scripts = signal<CopyScript[]>([]);
  readonly loading = signal(false);

  /** GET /api/scripts */
  loadAll(): void {
    this.loading.set(true);
    this.http.get<unknown>(API_SCRIPTS).subscribe({
      next: (res) => {
        const raw = unwrapApiDetails<ScriptApiDto[]>(res);
        const list = Array.isArray(raw) ? raw : [];
        this.scripts.set(list.map((s) => this.fromApi(s)));
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.show(this.messageFromHttp(err), 'error');
      },
    });
  }

  /** POST /api/scripts */
  create(script: CopyScript): Observable<CopyScript> {
    const body = this.toCreateBody(script);
    return this.http.post<unknown>(API_SCRIPTS, body).pipe(
      map((res) => this.fromApi(unwrapApiDetails<ScriptApiDto>(res))),
      tap((created) => {
        this.scripts.update((list) => [...list, created]);
        this.toast.show('Script creado', 'success');
      }),
      catchError((err) => {
        this.toast.show(this.messageFromHttp(err), 'error');
        return throwError(() => err);
      })
    );
  }

  /** PUT /api/scripts/:id */
  update(script: CopyScript): Observable<CopyScript> {
    const body = this.toUpdateBody(script);
    return this.http.put<unknown>(`${API_SCRIPTS}/${script.id}`, body).pipe(
      map((res) => this.fromApi(unwrapApiDetails<ScriptApiDto>(res))),
      tap((updated) => {
        this.scripts.update((list) => list.map((s) => (s.id === updated.id ? updated : s)));
        this.toast.show('Script actualizado', 'success');
      }),
      catchError((err) => {
        this.toast.show(this.messageFromHttp(err), 'error');
        return throwError(() => err);
      })
    );
  }

  /** DELETE /api/scripts/:id */
  deleteById(id: string): Observable<void> {
    return this.http.delete(`${API_SCRIPTS}/${id}`).pipe(
      map(() => void 0),
      tap(() => {
        this.scripts.update((list) => list.filter((s) => s.id !== id));
        this.toast.show('Script eliminado', 'success');
      }),
      catchError((err) => {
        this.toast.show(this.messageFromHttp(err), 'error');
        return throwError(() => err);
      })
    );
  }

  private fromApi(s: ScriptApiDto): CopyScript {
    return {
      id: String(s.id),
      name: s.nombre,
      scriptPath: s.scriptPath,
      arguments: s.arguments ?? '',
      scriptType: tipoApiToUi(s.tipo),
      enabled: true,
    };
  }

  private toCreateBody(s: CopyScript) {
    return {
      nombre: s.name.trim(),
      scriptPath: s.scriptPath.trim(),
      arguments: s.arguments ?? '',
      tipo: tipoUiToApi(s.scriptType),
    };
  }

  private toUpdateBody(s: CopyScript) {
    return this.toCreateBody(s);
  }

  private messageFromHttp(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      const body = err.error as { message?: string } | null;
      if (body?.message) return body.message;
      if (err.status === 0) return 'No hay conexión con el servidor. ¿Está la API en marcha?';
      if (err.status === 409) return body?.message ?? 'Conflicto: el recurso está en uso.';
    }
    return 'No se pudo completar la operación.';
  }
}

function tipoApiToUi(tipo: string): ScriptType {
  const t = tipo.trim().toLowerCase().replace(/^\./, '');
  if (t === 'ps1' || t === 'bat' || t === 'js') return t;
  return 'ps1';
}

function tipoUiToApi(st: ScriptType): string {
  return `.${st}`;
}
