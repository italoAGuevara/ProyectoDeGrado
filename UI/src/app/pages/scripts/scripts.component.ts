import { NgClass } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

export type ScriptWhen = 'pre' | 'post';

/** Solo se permiten scripts .ps1, .bat y .js */
export type ScriptType = 'ps1' | 'bat' | 'js';

export interface CopyScript {
  id: string;
  name: string;
  when: ScriptWhen;
  /** Tipo de script permitido: .ps1, .bat o .js */
  scriptType: ScriptType;
  /** Ruta al archivo del script (debe coincidir con la extensión de scriptType) */
  scriptPath: string;
  /** Argumentos opcionales al invocar el script */
  arguments: string;
  order: number;
  enabled: boolean;
}

@Component({
  selector: 'app-scripts',
  standalone: true,
  imports: [FormsModule, NgClass],
  templateUrl: './scripts.component.html',
})
export class ScriptsComponent {
  readonly allowedTypes: { value: ScriptType; label: string }[] = [
    { value: 'ps1', label: 'PowerShell (.ps1)' },
    { value: 'bat', label: 'Batch (.bat)' },
    { value: 'js', label: 'Node.js (.js)' },
  ];

  scripts = signal<CopyScript[]>([
    {
      id: '1',
      name: 'Preparar carpetas',
      when: 'pre',
      scriptType: 'ps1',
      scriptPath: 'C:\\Scripts\\pre-backup.ps1',
      arguments: '',
      order: 1,
      enabled: true,
    },
    {
      id: '2',
      name: 'Notificar fin',
      when: 'post',
      scriptType: 'js',
      scriptPath: 'C:\\Scripts\\notify.js',
      arguments: '',
      order: 1,
      enabled: true,
    },
  ]);

  showModal = signal(false);
  editingScript = signal<CopyScript | null>(null);
  formName = '';
  formWhen: ScriptWhen = 'pre';
  formScriptType: ScriptType = 'ps1';
  formScriptPath = '';
  formArguments = '';
  formOrder = 1;
  formEnabled = true;
  formPathError = signal<string | null>(null);

  openCreate(): void {
    this.editingScript.set(null);
    this.formName = '';
    this.formWhen = 'pre';
    this.formScriptType = 'ps1';
    this.formScriptPath = '';
    this.formArguments = '';
    this.formOrder = 1;
    this.formEnabled = true;
    this.formPathError.set(null);
    this.showModal.set(true);
  }

  openEdit(script: CopyScript): void {
    this.editingScript.set(script);
    this.formName = script.name;
    this.formWhen = script.when;
    this.formScriptType = script.scriptType;
    this.formScriptPath = script.scriptPath;
    this.formArguments = script.arguments;
    this.formOrder = script.order;
    this.formEnabled = script.enabled;
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
    if (!this.validatePath()) return;
    const edit = this.editingScript();
    const script: CopyScript = {
      id: edit?.id ?? String(Date.now()),
      name: this.formName,
      when: this.formWhen,
      scriptType: this.formScriptType,
      scriptPath: this.formScriptPath.trim(),
      arguments: this.formArguments,
      order: this.formOrder,
      enabled: this.formEnabled,
    };

    if (edit) {
      this.scripts.update((list) =>
        list.map((s) => (s.id === edit.id ? script : s))
      );
    } else {
      this.scripts.update((list) => [...list, script]);
    }
    this.closeModal();
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingScript.set(null);
  }

  deleteScript(script: CopyScript): void {
    if (confirm(`¿Eliminar script "${script.name}"?`)) {
      this.scripts.update((list) => list.filter((s) => s.id !== script.id));
    }
  }

  whenLabel(when: ScriptWhen): string {
    return when === 'pre' ? 'Pre-copiado' : 'Post-copiado';
  }

  whenBadgeClass(when: ScriptWhen): string {
    return when === 'pre' ? 'bg-primary' : 'bg-info text-dark';
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
