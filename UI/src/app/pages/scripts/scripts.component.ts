import { NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CopyScript, ScriptsService, ScriptType } from '../../services/scripts.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-scripts',
  standalone: true,
  imports: [FormsModule, NgClass],
  templateUrl: './scripts.component.html',
})
export class ScriptsComponent implements OnInit {
  private scriptsService = inject(ScriptsService);
  private toast = inject(ToastService);

  readonly allowedTypes: { value: ScriptType; label: string }[] = [
    { value: 'ps1', label: 'PowerShell (.ps1)' },
    { value: 'bat', label: 'Batch (.bat)' },
    { value: 'js', label: 'Node.js (.js)' },
  ];

  scripts = this.scriptsService.scripts;
  loading = this.scriptsService.loading;

  showModal = signal(false);
  editingScript = signal<CopyScript | null>(null);
  formName = '';
  formScriptType: ScriptType = 'ps1';
  formScriptPath = '';
  formArguments = '';
  formPathError = signal<string | null>(null);

  ngOnInit(): void {
    this.scriptsService.loadAll();
  }

  openCreate(): void {
    this.editingScript.set(null);
    this.formName = '';
    this.formScriptType = 'ps1';
    this.formScriptPath = '';
    this.formArguments = '';
    this.formPathError.set(null);
    this.showModal.set(true);
  }

  openEdit(script: CopyScript): void {
    this.editingScript.set(script);
    this.formName = script.name;
    this.formScriptType = script.scriptType;
    this.formScriptPath = script.scriptPath;
    this.formArguments = script.arguments;
    this.formPathError.set(null);
    this.showModal.set(true);
  }

  validatePath(): boolean {
    const path = this.formScriptPath.trim().toLowerCase();
    const ext = '.' + this.formScriptType;
    if (!path) {
      this.formPathError.set('Indica la ruta del script.');
      return false;
    }
    if (!path.endsWith(ext)) {
      this.formPathError.set(`La ruta debe terminar en ${ext}`);
      return false;
    }
    this.formPathError.set(null);
    return true;
  }

  save(): void {
    if (!this.formName.trim()) {
      this.toast.show('El nombre es obligatorio.', 'warning');
      return;
    }
    if (!this.validatePath()) return;

    const edit = this.editingScript();
    const script: CopyScript = {
      id: edit?.id ?? '',
      name: this.formName.trim(),
      scriptType: this.formScriptType,
      scriptPath: this.formScriptPath.trim(),
      arguments: this.formArguments,
      enabled: true,
    };

    if (edit) {
      this.scriptsService.update(script).subscribe({
        next: () => this.closeModal(),
        error: () => {},
      });
    } else {
      this.scriptsService.create(script).subscribe({
        next: () => this.closeModal(),
        error: () => {},
      });
    }
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingScript.set(null);
  }

  deleteScript(script: CopyScript): void {
    if (!confirm(`¿Eliminar script "${script.name}"?`)) return;
    this.scriptsService.deleteById(script.id).subscribe({ error: () => {} });
  }

  scriptTypeLabel(type: ScriptType): string {
    const t = this.allowedTypes.find((x) => x.value === type);
    return t ? t.label : type;
  }

  scriptTypeBadgeClass(type: ScriptType): string {
    switch (type) {
      case 'ps1':
        return 'bg-primary';
      case 'bat':
        return 'bg-dark';
      case 'js':
        return 'bg-success';
      default:
        return 'bg-secondary';
    }
  }

  get modalTitle(): string {
    return this.editingScript() ? 'Editar script' : 'Nuevo script';
  }
}
