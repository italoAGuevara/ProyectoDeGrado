import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { unwrapApiDetails } from '../utils/api-response.util';

const API_BASE = '/api/reportes/log-acciones-usuario';

export interface LogAccionUsuarioItem {
  id: string;
  fechaAccion: string;
  valorAnterior: string;
  valorNuevo: string;
  accion: string;
  tablaAfectada: string;
}

/** Contrato actual del API (mismo estilo que historial de ejecuciones). */
interface LogAccionesUsuarioListDto {
  items: LogAccionUsuarioItem[];
}

function tryParseJson(raw: string): unknown {
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

function normalizeLogAccionesPayload(data: unknown): LogAccionUsuarioItem[] {
  if (Array.isArray(data)) {
    return data as LogAccionUsuarioItem[];
  }
  if (data !== null && typeof data === 'object' && 'items' in data) {
    const items = (data as LogAccionesUsuarioListDto).items;
    return Array.isArray(items) ? items : [];
  }
  return [];
}

@Injectable({
  providedIn: 'root',
})
export class LogAccionesUsuarioService {
  private http = inject(HttpClient);

  /** `limite` opcional; el servidor usa 500 por defecto y capa en 2000. */
  listar(limite?: number): Observable<LogAccionUsuarioItem[]> {
    let params = new HttpParams();
    if (limite != null && limite > 0) {
      params = params.set('limite', String(limite));
    }
    return this.http
      .get(API_BASE, { params, responseType: 'text' })
      .pipe(
        map((raw) => {
          const parsed = tryParseJson(raw);
          return normalizeLogAccionesPayload(unwrapApiDetails(parsed));
        })
    );
  }
}
