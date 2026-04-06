import { Injectable, inject, signal, computed } from '@angular/core';
import {
  JobExecutionsReportService,
  EjecucionHistorialItem,
} from './job-executions-report.service';

const STORAGE_KEY = 'cloudkeep-bell-read-execution-ids';

export interface BellNotification {
  id: string;
  title: string;
  message: string;
  date: string;
  read: boolean;
  /** Ruta al abrir la notificación (p. ej. reporte de trabajos) */
  link?: string;
}

@Injectable({
  providedIn: 'root',
})
export class BellNotificationsService {
  private report = inject(JobExecutionsReportService);
  private notificationsSig = signal<BellNotification[]>([]);
  private readExecutionIds = new Set<number>();

  readonly notifications = this.notificationsSig.asReadonly();
  readonly unreadCount = computed(() => this.notificationsSig().filter((n) => !n.read).length);
  readonly loading = signal(false);

  constructor() {
    this.loadReadIdsFromStorage();
  }

  /** Últimas ejecuciones del API de reportes (misma fuente que el módulo Reportes). */
  refresh(): void {
    this.loading.set(true);
    this.report.getHistorial({ page: 1, pageSize: 20 }).subscribe({
      next: (list) => {
        const items = list.items.map((item) => this.mapExecution(item));
        this.notificationsSig.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.notificationsSig.set([]);
        this.loading.set(false);
      },
    });
  }

  markAsRead(n: BellNotification): void {
    const execId = parseInt(n.id.replace(/^exec-/, ''), 10);
    if (!Number.isNaN(execId)) {
      this.readExecutionIds.add(execId);
      this.saveReadIdsToStorage();
    }
    this.notificationsSig.update((list) =>
      list.map((item) => (item.id === n.id ? { ...item, read: true } : item))
    );
  }

  private mapExecution(item: EjecucionHistorialItem): BellNotification {
    const id = `exec-${item.id}`;
    const read = this.readExecutionIds.has(item.id);
    const date = this.relativeTime(item.endTime || item.startTime);
    const { title, message } = this.buildTitleMessage(item);
    return {
      id,
      title,
      message,
      date,
      read,
      link: '/reportes',
    };
  }

  private buildTitleMessage(item: EjecucionHistorialItem): { title: string; message: string } {
    const nombre = item.trabajoNombre || 'Trabajo';
    const estado = (item.estado || '').toLowerCase();
    if (estado === 'fallido') {
      const err = item.errorMessage ? this.truncate(item.errorMessage, 120) : 'Sin detalle.';
      return {
        title: 'Ejecución fallida',
        message: `${nombre}: ${err}`,
      };
    }
    if (estado === 'completado') {
      const files =
        item.archivosCopiados != null
          ? `${item.archivosCopiados} archivo(s) copiados.`
          : 'Copia finalizada.';
      return {
        title: 'Ejecución completada',
        message: `${nombre}. ${files}`,
      };
    }
    if (estado === 'en_progreso') {
      return {
        title: 'Ejecución en curso',
        message: `${nombre} está ejecutándose (${this.disparoLabel(item.disparo)}).`,
      };
    }
    return {
      title: 'Ejecución',
      message: `${nombre} — estado: ${item.estado}`,
    };
  }

  private disparoLabel(disparo: string): string {
    return disparo === 'programada' ? 'programada' : 'manual';
  }

  private truncate(s: string, max: number): string {
    return s.length <= max ? s : s.slice(0, max) + '…';
  }

  private relativeTime(iso: string): string {
    const t = new Date(iso).getTime();
    if (Number.isNaN(t)) return '';
    const sec = Math.round((Date.now() - t) / 1000);
    if (sec < 45) return 'Hace un momento';
    const min = Math.round(sec / 60);
    if (min < 60) return `Hace ${min} min`;
    const hrs = Math.round(min / 60);
    if (hrs < 24) return `Hace ${hrs} h`;
    const days = Math.round(hrs / 24);
    if (days < 7) return `Hace ${days} día${days === 1 ? '' : 's'}`;
    return new Date(iso).toLocaleDateString('es', { day: 'numeric', month: 'short' });
  }

  private loadReadIdsFromStorage(): void {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return;
      const arr = JSON.parse(raw) as unknown;
      if (!Array.isArray(arr)) return;
      for (const x of arr) {
        if (typeof x === 'number') this.readExecutionIds.add(x);
        else if (typeof x === 'string') {
          const n = parseInt(x, 10);
          if (!Number.isNaN(n)) this.readExecutionIds.add(n);
        }
      }
    } catch {
      /* ignore corrupt storage */
    }
  }

  private saveReadIdsToStorage(): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify([...this.readExecutionIds]));
    } catch {
      /* ignore quota / private mode */
    }
  }
}
