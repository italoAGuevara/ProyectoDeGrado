import { Component, inject, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobsService } from '../../../services/jobs.service';
import { ReportBreadcrumbComponent, BreadcrumbItem } from '../../../layout/report-breadcrumb/report-breadcrumb.component';

export interface DestinationSummary {
  name: string;
  jobsCount: number;
  lastRun: string | null;
}

@Component({
  selector: 'app-destination-executions',
  standalone: true,
  imports: [RouterLink, ReportBreadcrumbComponent],
  templateUrl: './destination-executions.component.html',
  styleUrl: './destination-executions.component.css',
})
export class DestinationExecutionsComponent {
  private jobsService = inject(JobsService);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Reportes', link: '/reportes' },
    { label: 'Ejecuciones por destino' },
  ];

  /** Agrupa trabajos por nombre de destino y cuenta ejecuciones (placeholder) */
  destinations = computed<DestinationSummary[]>(() => {
    const jobs = this.jobsService.jobs();
    const byName = new Map<string, number>();
    for (const j of jobs) {
      const n = j.destinationName || 'Sin destino';
      byName.set(n, (byName.get(n) ?? 0) + 1);
    }
    return Array.from(byName.entries()).map(([name, jobsCount]) => ({
      name,
      jobsCount,
      lastRun: null as string | null,
    }));
  });

  totalJobs = computed(() => this.jobsService.jobs().length);
}
