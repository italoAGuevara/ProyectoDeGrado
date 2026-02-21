import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { CopyScript, ScriptsService } from '../../../services/scripts.service';
import { ToastService } from '../../../services/toast.service';

const WEEKDAY_LABELS = [
    { value: 0, label: 'D' },
    { value: 1, label: 'L' },
    { value: 2, label: 'M' },
    { value: 3, label: 'X' },
    { value: 4, label: 'J' },
    { value: 5, label: 'V' },
    { value: 6, label: 'S' },
];

@Component({
    selector: 'app-job-wizard',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule],
    templateUrl: './job-wizard.component.html',
    styleUrl: './job-wizard.component.css'
})
export class JobWizardComponent implements OnInit {
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private scriptsService = inject(ScriptsService);
    private toastService = inject(ToastService);

    ngOnInit(): void {
        this.route.params.subscribe(params => {
            const id = params['id'];
            if (id) {
                // TODO: Load job data
                this.editingJobId = id;
            }
        });
    }

    readonly weekdayOptions = WEEKDAY_LABELS;
    readonly hours = Array.from({ length: 24 }, (_, i) => i);
    readonly minutes = [0, 15, 30, 45];
    readonly daysOfMonth = Array.from({ length: 31 }, (_, i) => i + 1);

    availableScripts = this.scriptsService.scripts;

    // ...

    steps = ['General', 'Destino', 'Fuente', 'Programación', 'Opciones'];
    currentStep = 0;

    // Edit mode
    editingJobId: string | null = null;


    // Form Fields
    formName = '';
    formDescription = '';
    formEnabled = true;
    formDestination = '';

    // Filters
    excludeHidden = false;
    excludeSystem = false;
    excludeTemporary = false;
    excludeLargerThanMB: number | null = null;
    customFilters = '';
    // ...
    saveJob(): void {
        // Aquí se debería guardar en el servicio/store
        // Simularemos guardado
        console.log('Guardando trabajo...', {
            name: this.formName,
            description: this.formDescription,
            destination: this.formDestination,
            schedule: this.formSchedule,
            scripts: this.formScripts,
            enabled: this.formEnabled
        });

        this.toastService.show('Trabajo creado correctamente', 'success');
        this.router.navigate(['/trabajos']);
    }

    // Schedule
    formScheduleType: 'daily' | 'weekly' | 'monthly' = 'daily';
    formScheduleHour = 2;
    formScheduleMinute = 0;
    formScheduleWeekdays: number[] = [1];
    formScheduleDayOfMonth = 1;

    // Options
    formScripts: { scriptId: string; when: 'pre' | 'post' }[] = [];

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
            const days = this.formScheduleWeekdays.length ? this.formScheduleWeekdays.sort((a, b) => a - b).join(',') : '0';
            return `${m} ${h} * * ${days}`;
        }
        const d = this.formScheduleDayOfMonth;
        return `${m} ${h} ${d} * *`;
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
        // Solo permitir volver atrás o ir al siguiente paso (si es válido) - por ahora libre
        this.currentStep = index;
    }

    toggleWeekday(day: number): void {
        const i = this.formScheduleWeekdays.indexOf(day);
        if (i >= 0) {
            const next = this.formScheduleWeekdays.filter((_, idx) => idx !== i);
            if (next.length === 0) return; // Mínimo un día
            this.formScheduleWeekdays = next;
        } else {
            this.formScheduleWeekdays = [...this.formScheduleWeekdays, day].sort((a, b) => a - b);
        }
    }

    isWeekdaySelected(day: number): boolean {
        return this.formScheduleWeekdays.includes(day);
    }

    toggleScript(scriptId: string, when: 'pre' | 'post'): void {
        const index = this.formScripts.findIndex((s) => s.scriptId === scriptId && s.when === when);
        if (index >= 0) {
            this.formScripts.splice(index, 1);
        } else {
            this.formScripts.push({ scriptId, when });
        }
    }

    isScriptSelected(scriptId: string, when: 'pre' | 'post'): boolean {
        return this.formScripts.some((s) => s.scriptId === scriptId && s.when === when);
    }

    pad2(n: number): string {
        return String(n).padStart(2, '0');
    }


    cancel(): void {
        this.router.navigate(['/trabajos']);
    }
}
