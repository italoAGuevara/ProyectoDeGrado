import { Component, inject, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobsService } from '../../../services/jobs.service';
import { DestinationsService } from '../../../services/destinations.service';
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
export class DestinationExecutionsComponent implements OnInit {
  private jobsService = inject(JobsService);
  private destinationsService = inject(DestinationsService);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Reportes', link: '/reportes' },
    { label: 'Ejecuciones por destino' },
  ];

  ngOnInit(): void {
    this.jobsService.loadAll();
    this.destinationsService.loadAll();
  }

  destinations = computed<DestinationSummary[]>(() => {
    const jobs = this.jobsService.jobs();
    const dests = this.destinationsService.destinations();
    const byName = new Map<string, number>();
    for (const j of jobs) {
      const name = dests.find((d) => d.id === j.destinoId)?.name ?? `Destino #${j.destinoId}`;
      byName.set(name, (byName.get(name) ?? 0) + 1);
    }
    return Array.from(byName.entries()).map(([name, jobsCount]) => ({
      name,
      jobsCount,
      lastRun: null as string | null,
    }));
  });

  totalJobs = computed(() => this.jobsService.jobs().length);
}
