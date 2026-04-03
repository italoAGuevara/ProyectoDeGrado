import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobsService, Job } from '../../../services/jobs.service';
import { ScriptsService } from '../../../services/scripts.service';
import { DestinationsService } from '../../../services/destinations.service';
import { ReportBreadcrumbComponent, BreadcrumbItem } from '../../../layout/report-breadcrumb/report-breadcrumb.component';

@Component({
  selector: 'app-job-report',
  standalone: true,
  imports: [RouterLink, ReportBreadcrumbComponent],
  templateUrl: './job-report.component.html',
  styleUrl: './job-report.component.css',
})
export class JobReportComponent implements OnInit {
  private jobsService = inject(JobsService);
  private scriptsService = inject(ScriptsService);
  private destinationsService = inject(DestinationsService);

  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Reportes', link: '/reportes' },
    { label: 'Reporte de trabajos' },
  ];
  jobs = this.jobsService.jobs;

  ngOnInit(): void {
    this.jobsService.loadAll();
    this.scriptsService.loadAll();
    this.destinationsService.loadAll();
  }

  destinationLabel(job: Job): string {
    const d = this.destinationsService.destinations().find((x) => x.id === job.destinoId);
    return d?.name ?? `Destino #${job.destinoId}`;
  }

  scheduleLabel(schedule: string): string {
    const parts = schedule.trim().split(/\s+/);
    if (parts.length < 5) return schedule;
    const [min, hour, dayMonth, , dayWeek] = parts;
    const h = parseInt(hour, 10);
    const m = parseInt(min, 10);
    const timeStr = `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
    if (dayMonth !== '*' && dayWeek === '*') {
      return `Mensual, día ${dayMonth} a las ${timeStr}`;
    }
    if (dayWeek !== '*') {
      const names = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
      const days = dayWeek.split(',').map((s) => names[parseInt(s.trim(), 10)]).filter(Boolean);
      return `Semanal (${days.join(', ')}) a las ${timeStr}`;
    }
    return `Diario a las ${timeStr}`;
  }

  getJobScripts(job: Job): { name: string; when: 'pre' | 'post' }[] {
    const allScripts = this.scriptsService.scripts();
    const out: { name: string; when: 'pre' | 'post' }[] = [];
    if (job.scriptPreId != null) {
      const pre = allScripts.find((s) => s.id === String(job.scriptPreId));
      if (pre) out.push({ name: pre.name, when: 'pre' });
    }
    if (job.scriptPostId != null) {
      const post = allScripts.find((s) => s.id === String(job.scriptPostId));
      if (post) out.push({ name: post.name, when: 'post' });
    }
    return out;
  }

  countActive(): number {
    return this.jobs().filter((j) => j.enabled).length;
  }

  countPaused(): number {
    return this.jobs().filter((j) => !j.enabled).length;
  }
}
