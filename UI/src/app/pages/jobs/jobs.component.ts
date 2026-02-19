import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Job {
  id: string;
  name: string;
  sourcePath: string;
  destinationName: string;
  schedule: string;
  enabled: boolean;
}

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
  readonly weekdayOptions = WEEKDAY_LABELS;
  readonly hours = Array.from({ length: 24 }, (_, i) => i);
  readonly minutes = [0, 15, 30, 45];
  readonly daysOfMonth = Array.from({ length: 31 }, (_, i) => i + 1);

  jobs = signal<Job[]>([
    {
      id: '1',
      name: 'Backup diario documentos',
      sourcePath: 'C:\\Docs',
      destinationName: 'S3 principal',
      schedule: '0 2 * * *',
      enabled: true,
    },
  ]);

  showModal = signal(false);
  editingJob = signal<Job | null>(null);
  formName = '';
  formSourcePath = '';
  formDestination = '';
  /** 'daily' | 'weekly' | 'monthly' */
  formScheduleType: 'daily' | 'weekly' | 'monthly' = 'daily';
  formScheduleHour = 2;
  formScheduleMinute = 0;
  /** Días de la semana seleccionados (0=dom ... 6=sáb) */
  formScheduleWeekdays: number[] = [1];
  /** Día del mes (1-31) para mensual */
  formScheduleDayOfMonth = 1;
  formEnabled = true;

  get formSchedule(): string {
    const m = this.formScheduleMinute;
    const h = this.formScheduleHour;
    if (this.formScheduleType === 'daily') {
      return `${m} ${h} * * *`;
    }
    if (this.formScheduleType === 'weekly') {
      const days = this.formScheduleWeekdays.length ? this.formScheduleWeekdays.sort((a, b) => a - b).join(',') : '0';
      return `${m} ${h} * * ${days}`;
    }
    const d = this.formScheduleDayOfMonth;
    return `${m} ${h} ${d} * *`;
  }

  openCreate(): void {
    this.editingJob.set(null);
    this.formName = '';
    this.formSourcePath = '';
    this.formDestination = '';
    this.formScheduleType = 'daily';
    this.formScheduleHour = 2;
    this.formScheduleMinute = 0;
    this.formScheduleWeekdays = [1];
    this.formScheduleDayOfMonth = 1;
    this.formEnabled = true;
    this.showModal.set(true);
  }

  openEdit(job: Job): void {
    this.editingJob.set(job);
    this.formName = job.name;
    this.formSourcePath = job.sourcePath;
    this.formDestination = job.destinationName;
    this.formEnabled = job.enabled;
    this.parseCronToForm(job.schedule);
    this.showModal.set(true);
  }

  /** Rellena los campos de programación a partir de una expresión cron */
  private parseCronToForm(cron: string): void {
    const parts = cron.trim().split(/\s+/);
    if (parts.length < 5) {
      this.formScheduleType = 'daily';
      this.formScheduleHour = 0;
      this.formScheduleMinute = 0;
      this.formScheduleWeekdays = [1];
      this.formScheduleDayOfMonth = 1;
      return;
    }
    const [min, hour, dayMonth, , dayWeek] = parts;
    this.formScheduleMinute = parseInt(min, 10) || 0;
    this.formScheduleHour = parseInt(hour, 10) || 0;

    if (dayMonth !== '*' && dayWeek === '*') {
      this.formScheduleType = 'monthly';
      this.formScheduleDayOfMonth = Math.min(31, Math.max(1, parseInt(dayMonth, 10) || 1));
      this.formScheduleWeekdays = [1];
      return;
    }
    if (dayWeek !== '*') {
      this.formScheduleType = 'weekly';
      this.formScheduleWeekdays = dayWeek.split(',').map((s) => parseInt(s.trim(), 10)).filter((n) => !isNaN(n) && n >= 0 && n <= 6);
      if (this.formScheduleWeekdays.length === 0) this.formScheduleWeekdays = [1];
      this.formScheduleDayOfMonth = 1;
      return;
    }
    this.formScheduleType = 'daily';
    this.formScheduleWeekdays = [1];
    this.formScheduleDayOfMonth = 1;
  }

  toggleWeekday(day: number): void {
    const i = this.formScheduleWeekdays.indexOf(day);
    if (i >= 0) {
      const next = this.formScheduleWeekdays.filter((_, idx) => idx !== i);
      if (next.length === 0) return;
      this.formScheduleWeekdays = next;
    } else {
      this.formScheduleWeekdays = [...this.formScheduleWeekdays, day].sort((a, b) => a - b);
    }
  }

  isWeekdaySelected(day: number): boolean {
    return this.formScheduleWeekdays.includes(day);
  }

  /** Formato de número con 2 dígitos (para hora/minuto en template) */
  pad2(n: number): string {
    return String(n).padStart(2, '0');
  }

  save(): void {
    const schedule = this.formSchedule;
    const edit = this.editingJob();
    if (edit) {
      this.jobs.update((list) =>
        list.map((j) =>
          j.id === edit.id
            ? {
                ...j,
                name: this.formName,
                sourcePath: this.formSourcePath,
                destinationName: this.formDestination,
                schedule,
                enabled: this.formEnabled,
              }
            : j
        )
      );
    } else {
      this.jobs.update((list) => [
        ...list,
        {
          id: String(Date.now()),
          name: this.formName,
          sourcePath: this.formSourcePath,
          destinationName: this.formDestination,
          schedule,
          enabled: this.formEnabled,
        },
      ]);
    }
    this.closeModal();
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingJob.set(null);
  }

  deleteJob(job: Job): void {
    if (confirm(`¿Eliminar trabajo "${job.name}"?`)) {
      this.jobs.update((list) => list.filter((j) => j.id !== job.id));
    }
  }

  runJob(job: Job): void {
    // Aquí conectarás con el backend para ejecutar el trabajo
    this.runningJobId.set(job.id);
    setTimeout(() => {
      this.runningJobId.set(null);
      // Por ahora solo feedback visual; luego: llamar API y mostrar resultado
    }, 1500);
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

  get modalTitle(): string {
    return this.editingJob() ? 'Editar trabajo' : 'Nuevo trabajo';
  }
}
