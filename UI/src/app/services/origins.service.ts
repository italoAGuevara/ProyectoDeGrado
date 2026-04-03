import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { unwrapApiDetails } from '../utils/api-response.util';
import { messageFromHttpError } from '../utils/http-error.util';
import { ToastService } from './toast.service';

export interface OrigenRow {
  id: number;
  name: string;
  path: string;
  description: string;
}

interface OrigenApiDto {
  id: number;
  nombre: string;
  ruta: string;
  descripcion: string;
}

const API_ORIGENES = '/api/origenes';

@Injectable({
  providedIn: 'root',
})
export class OriginsService {
  private readonly http = inject(HttpClient);
  private readonly toast = inject(ToastService);

  origins = signal<OrigenRow[]>([]);
  readonly loading = signal(false);

  loadAll(): void {
    this.loading.set(true);
    this.http.get<unknown>(API_ORIGENES).subscribe({
      next: (res) => {
        const raw = unwrapApiDetails<OrigenApiDto[]>(res);
        const list = Array.isArray(raw) ? raw : [];
        this.origins.set(list.map((o) => this.fromApi(o)).sort((a, b) => a.id - b.id));
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.show(messageFromHttpError(err), 'error');
      },
    });
  }

  private fromApi(o: OrigenApiDto): OrigenRow {
    return {
      id: o.id,
      name: o.nombre,
      path: o.ruta ?? '',
      description: o.descripcion ?? '',
    };
  }
}
