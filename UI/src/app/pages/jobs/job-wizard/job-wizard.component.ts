import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ScriptsService } from '../../../services/scripts.service';
import { ToastService } from '../../../services/toast.service';
import { DestinationsService } from '../../../services/destinations.service';
import { OriginsService } from '../../../services/origins.service';
import { JobsService } from '../../../services/jobs.service';

const WEEKDAY_LABELS = [
  { value: 0, label: 'D' },
  { value: 1, label: 'L' },
  { value: 2, label: 'M' },
  { value: 3, label: 'X' },
  { value: 4, label: 'J' },
  { value: 5, label: 'V' },
  { value: 6, label: 'S' },
];

const MINUTE_OPTIONS = [0, 15, 30, 45];

function nearestMinute(m: number): number {
  return MINUTE_OPTIONS.reduce((best, x) => (Math.abs(x - m) < Math.abs(best - m) ? x : best), 0);
}

/** Interpreta cron de 5 campos estándar para rellenar el formulario (casos simples). */
function applyCronToForm(
  cron: string,
  target: {
    formScheduleType: 'daily' | 'weekly' | 'monthly';
    formScheduleHour: number;
    formScheduleMinute: number;
    formScheduleWeekdays: number[];
    formScheduleDayOfMonth: number;
  }
): void {
  const parts = cron.trim().split(/\s+/).filter(Boolean);
  if (parts.length < 5) return;
  const [minS, hourS, dom, , dow] = parts;
  const m = parseInt(minS, 10);
  const h = parseInt(hourS, 10);
  if (!Number.isNaN(h) && h >= 0 && h <= 23) target.formScheduleHour = h;
  if (!Number.isNaN(m) && m >= 0 && m <= 59) target.formScheduleMinute = nearestMinute(m);

  if (dow !== '*' && dow !== '?') {
    target.formScheduleType = 'weekly';
    const days = dow
      .split(',')
      .map((s) => parseInt(s.trim(), 10))
      .filter((n) => !Number.isNaN(n) && n >= 0 && n <= 6);
    if (days.length) target.formScheduleWeekdays = [...new Set(days)].sort((a, b) => a - b);
    return;
  }
  if (dom !== '*' && dom !== '?') {
    const d = parseInt(dom, 10);
    if (!Number.isNaN(d) && d >= 1 && d <= 31) {
      target.formScheduleType = 'monthly';
      target.formScheduleDayOfMonth = d;
    }
    return;
  }
  target.formScheduleType = 'daily';
}

@Component({
  selector: 'app-job-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './job-wizard.component.html',
  styleUrl: './job-wizard.component.css',
})
export class JobWizardComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  readonly scriptsService = inject(ScriptsService);
  private toastService = inject(ToastService);
  readonly destinationsService = inject(DestinationsService);
  readonly originsService = inject(OriginsService);
  private jobsService = inject(JobsService);

  readonly saving = signal(false);
  readonly loadingJob = signal(false);

  readonly weekdayOptions = WEEKDAY_LABELS;
  readonly hours = Array.from({ length: 24 }, (_, i) => i);
  readonly minutes = MINUTE_OPTIONS;
  readonly daysOfMonth = Array.from({ length: 31 }, (_, i) => i + 1);

  destinations = this.destinationsService.destinations;
  origins = this.originsService.origins;
  availableScripts = this.scriptsService.scripts;

  readonly isEditMode = computed(() => this.editingJobId() !== null);
  editingJobId = signal<number | null>(null);

  steps = ['General', 'Destino', 'Fuente', 'Programación', 'Opciones'];
  currentStep = 0;

  formName = '';
  formDescription = '';
  formEnabled = true;
  formDestinoId: number | null = null;
  formOrigenId: number | null = null;

  formScheduleType: 'daily' | 'weekly' | 'monthly' = 'daily';
  formScheduleHour = 2;
  formScheduleMinute = 0;
  formScheduleWeekdays: number[] = [1];
  formScheduleDayOfMonth = 1;

  formScriptPreId: number | null = null;
  formScriptPostId: number | null = null;
  formPreDetenerEnFallo = false;
  formPostDetenerEnFallo = false;

  ngOnInit(): void {
    this.scriptsService.loadAll();
    this.destinationsService.loadAll();
    this.originsService.loadAll();

    this.route.paramMap.subscribe((p) => {
      const idStr = p.get('id');
      if (idStr != null && idStr !== '') {
        const id = Number(idStr);
        if (Number.isFinite(id)) {
          this.editingJobId.set(id);
          this.loadJobForEdit(id);
          return;
        }
      }
      this.editingJobId.set(null);
      this.resetFormForCreate();
    });
  }

  private resetFormForCreate(): void {
    this.formName = '';
    this.formDescription = '';
    this.formEnabled = true;
    this.formDestinoId = null;
    this.formOrigenId = null;
    this.formScheduleType = 'daily';
    this.formScheduleHour = 2;
    this.formScheduleMinute = 0;
    this.formScheduleWeekdays = [1];
    this.formScheduleDayOfMonth = 1;
    this.formScriptPreId = null;
    this.formScriptPostId = null;
    this.formPreDetenerEnFallo = false;
    this.formPostDetenerEnFallo = false;
    this.currentStep = 0;
  }

  private loadJobForEdit(id: number): void {
    this.loadingJob.set(true);
    this.jobsService.getById(id).subscribe({
      next: (job) => {
        this.formName = job.name;
        this.formDescription = job.description;
        this.formEnabled = job.enabled;
        this.formDestinoId = job.destinoId;
        this.formOrigenId = job.origenId;
        this.formScriptPreId = job.scriptPreId;
        this.formScriptPostId = job.scriptPostId;
        this.formPreDetenerEnFallo = job.preDetenerEnFallo;
        this.formPostDetenerEnFallo = job.postDetenerEnFallo;
        applyCronToForm(job.schedule, this);
        this.loadingJob.set(false);
      },
      error: () => this.loadingJob.set(false),
    });
  }

  get currentStepName(): string {
    return this.steps[this.currentStep];
  }

  get formSchedule(): string {
    const m = this.formScheduleMinute;
    const h = this.formScheduleHour;
    if (this.formScheduleType === 'daily') {
      return `${m} ${h} * * *`;
    }
    if (this.formScheduleType === 'weekly') {
      const days = this.formScheduleWeekdays.length
        ? [...this.formScheduleWeekdays].sort((a, b) => a - b).join(',')
        : '0';
      return `${m} ${h} * * ${days}`;
    }
    const d = this.formScheduleDayOfMonth;
    return `${m} ${h} ${d} * *`;
  }

  private validateForSave(): string | null {
    if (!this.formName.trim()) return 'Indica un nombre para el trabajo.';
    if (!this.formDescription.trim()) return 'La descripción es obligatoria.';
    if (this.formDestinoId == null) return 'Selecciona un destino.';
    if (this.formOrigenId == null) return 'Selecciona un origen.';
    return null;
  }

  saveJob(): void {
    const err = this.validateForSave();
    if (err) {
      this.toastService.show(err, 'error');
      return;
    }

    const payload = {
      nombre: this.formName.trim(),
      descripcion: this.formDescription.trim(),
      origenId: this.formOrigenId!,
      destinoId: this.formDestinoId!,
      scriptPreId: this.formScriptPreId,
      scriptPostId: this.formScriptPostId,
      preDetenerEnFallo: this.formPreDetenerEnFallo,
      postDetenerEnFallo: this.formPostDetenerEnFallo,
      cronExpression: this.formSchedule,
      activo: this.formEnabled,
    };

    this.saving.set(true);
    const id = this.editingJobId();
    const req =
      id != null
        ? this.jobsService.update(id, {
            nombre: payload.nombre,
            descripcion: payload.descripcion,
            origenId: payload.origenId,
            destinoId: payload.destinoId,
            scriptPreId: payload.scriptPreId,
            scriptPostId: payload.scriptPostId,
            preDetenerEnFallo: payload.preDetenerEnFallo,
            postDetenerEnFallo: payload.postDetenerEnFallo,
            cronExpression: payload.cronExpression,
            activo: payload.activo,
            sincronizarScripts: true,
          })
        : this.jobsService.create(payload);

    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/trabajos']);
      },
      error: () => this.saving.set(false),
    });
  }

  nextStep(): void {
    if (this.currentStep < this.steps.length - 1) {
      this.currentStep++;
    } else {
      this.saveJob();
    }
  }

  prevStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  goToStep(index: number): void {
    this.currentStep = index;
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

  pad2(n: number): string {
    return String(n).padStart(2, '0');
  }

  cancel(): void {
    this.router.navigate(['/trabajos']);
  }
}
