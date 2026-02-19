import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgClass } from '@angular/common';
import { ReportBreadcrumbComponent, BreadcrumbItem } from '../../../layout/report-breadcrumb/report-breadcrumb.component';

export type LogLevel = 'info' | 'warning' | 'error';

export interface LogEntry {
  id: string;
  level: LogLevel;
  date: string;
  message: string;
  source: string;
}

@Component({
  selector: 'app-logs-report',
  standalone: true,
  imports: [RouterLink, ReportBreadcrumbComponent, NgClass],
  templateUrl: './logs-report.component.html',
  styleUrl: './logs-report.component.css',
})
export class LogsReportComponent {
  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Reportes', link: '/reportes' },
    { label: 'Logs y errores' },
  ];

  /** Datos de ejemplo; en producción vendrían del backend */
  logs: LogEntry[] = [
    { id: '1', level: 'info', date: '2025-02-19 10:30:00', message: 'Copia completada correctamente.', source: 'Backup diario documentos' },
    { id: '2', level: 'warning', date: '2025-02-19 09:15:00', message: 'Algunos archivos omitidos por filtro.', source: 'Backup diario documentos' },
    { id: '3', level: 'error', date: '2025-02-18 02:00:00', message: 'Error de conexión con el destino. Reintentando...', source: 'S3 principal' },
    { id: '4', level: 'info', date: '2025-02-18 01:58:00', message: 'Script pre-ejecución finalizado.', source: 'Backup diario documentos' },
    { id: '5', level: 'info', date: '2025-02-17 02:00:00', message: 'Trabajo programado iniciado.', source: 'Sistema' },
  ];

  levelLabel(level: LogLevel): string {
    return { info: 'Info', warning: 'Advertencia', error: 'Error' }[level];
  }

  levelClass(level: LogLevel): string {
    return { info: 'bg-info', warning: 'bg-warning text-dark', error: 'bg-danger' }[level];
  }
}
