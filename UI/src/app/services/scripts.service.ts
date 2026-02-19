import { Injectable, signal } from '@angular/core';

export type ScriptWhen = 'pre' | 'post';

/** Solo se permiten scripts .ps1, .bat y .js */
export type ScriptType = 'ps1' | 'bat' | 'js';

export interface CopyScript {
    id: string;
    name: string;
    /** Tipo de script permitido: .ps1, .bat o .js */
    scriptType: ScriptType;
    /** Ruta al archivo del script (debe coincidir con la extensión de scriptType) */
    scriptPath: string;
    /** Argumentos opcionales al invocar el script */
    arguments: string;
    enabled: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ScriptsService {
    scripts = signal<CopyScript[]>([
        {
            id: '1',
            name: 'Preparar carpetas',
            scriptType: 'ps1',
            scriptPath: 'C:\\Scripts\\pre-backup.ps1',
            arguments: '',
            enabled: true,
        },
        {
            id: '2',
            name: 'Notificar fin',
            scriptType: 'js',
            scriptPath: 'C:\\Scripts\\notify.js',
            arguments: '',
            enabled: true,
        },
    ]);

    addScript(script: CopyScript) {
        this.scripts.update((list) => [...list, script]);
    }

    updateScript(updatedScript: CopyScript) {
        this.scripts.update((list) =>
            list.map((s) => (s.id === updatedScript.id ? updatedScript : s))
        );
    }

    deleteScript(id: string) {
        this.scripts.update((list) => list.filter((s) => s.id !== id));
    }
}
