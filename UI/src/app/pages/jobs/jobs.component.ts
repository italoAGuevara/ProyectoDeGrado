import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ScriptsService } from '../../services/scripts.service';
import { ToastService } from '../../services/toast.service';
import { JobsService, Job } from '../../services/jobs.service';

/** Cron: minuto hora díaMes mes díaSemana (0=domingo, 6=sábado) */
const WEEKDAY_LABELS: { value: number; label: string }[] = [
  { value: 0, label: 'D' },
  { value: 1, label: 'L' },
  { value: 2, label: 'M' },
  { value: 3, label: 'X' },
  { value: 4, label: 'J' },
  { value: 5, label: 'V' },
  { value: 6, label: 'S' },
];

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './jobs.component.html',
})
export class JobsComponent {
  private router = inject(Router);
  private scriptsService = inject(ScriptsService);
  private toastService = inject(ToastService);
  private jobsService = inject(JobsService);

  readonly weekdayOptions = WEEKDAY_LABELS;
  readonly hours = Array.from({ length: 24 }, (_, i) => i);
  // ... (rest of the code)

  // ...

  runJob(job: Job): void {
    // Aquí conectarás con el backend para ejecutar el trabajo
    this.runningJobId.set(job.id);
    this.toastService.show(`Iniciando trabajo: ${job.name}`, 'info');

    setTimeout(() => {
      this.runningJobId.set(null);
      this.toastService.show(`Trabajo finalizado: ${job.name}`, 'success');
      // Por ahora solo feedback visual; luego: llamar API y mostrar resultado
    }, 1500);
  }
  readonly minutes = [0, 15, 30, 45];
  readonly daysOfMonth = Array.from({ length: 31 }, (_, i) => i + 1);

  availableScripts = this.scriptsService.scripts;

  jobs = this.jobsService.jobs;

  openCreate(): void {
    this.router.navigate(['/trabajos/nuevo']);
  }

  openEdit(job: Job): void {
    this.router.navigate(['/trabajos/nuevo', { id: job.id }]);
  }

  deleteJob(job: Job): void {
    if (confirm(`¿Eliminar trabajo "${job.name}"?`)) {
      this.jobsService.deleteJob(job.id);
      this.toastService.show(`Trabajo eliminado: ${job.name}`, 'warning');
    }
  }


  /** ID del trabajo que se está ejecutando (para estado visual) */
  runningJobId = signal<string | null>(null);

  isRunning(job: Job): boolean {
    return this.runningJobId() === job.id;
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
    const allScripts = this.availableScripts();
    return (job.scripts || [])
      .map((js) => {
        const s = allScripts.find((as) => as.id === js.scriptId);
        return s ? { name: s.name, when: js.when } : null;
      })
      .filter((item): item is { name: string; when: 'pre' | 'post' } => !!item);
  }


}
