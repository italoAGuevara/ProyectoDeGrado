import { NgClass } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

type DestinationType = 's3' | 'google_drive';

interface Destination {
  id: string;
  name: string;
  type: DestinationType;
  config: Record<string, string>;
  enabled: boolean;
}

@Component({
  selector: 'app-destinations',
  standalone: true,
  imports: [FormsModule, NgClass],
  templateUrl: './destinations.component.html',
})
export class DestinationsComponent {
  destinations = signal<Destination[]>([
    {
      id: '1',
      name: 'S3 principal',
      type: 's3',
      config: { bucket: 'mi-bucket', region: 'us-east-1' },
      enabled: true,
    },
    {
      id: '2',
      name: 'Google Drive backup',
      type: 'google_drive',
      config: { folderId: 'abc123' },
      enabled: true,
    },
  ]);

  showModal = signal(false);
  editingDest = signal<Destination | null>(null);
  formName = '';
  formType: DestinationType = 's3';
  formBucket = '';
  formRegion = 'us-east-1';
  formFolderId = '';

  openCreate(): void {
    this.editingDest.set(null);
    this.formName = '';
    this.formType = 's3';
    this.formBucket = '';
    this.formRegion = 'us-east-1';
    this.formFolderId = '';
    this.showModal.set(true);
  }

  openEdit(d: Destination): void {
    this.editingDest.set(d);
    this.formName = d.name;
    this.formType = d.type;
    this.formBucket = d.config['bucket'] ?? '';
    this.formRegion = d.config['region'] ?? 'us-east-1';
    this.formFolderId = d.config['folderId'] ?? '';
    this.showModal.set(true);
  }

  save(): void {
    const edit = this.editingDest();
    const config: Record<string, string> =
      this.formType === 's3'
        ? { bucket: this.formBucket, region: this.formRegion }
        : { folderId: this.formFolderId };

    if (edit) {
      this.destinations.update((list) =>
        list.map((d) =>
          d.id === edit.id
            ? { ...d, name: this.formName, type: this.formType, config }
            : d
        )
      );
    } else {
      this.destinations.update((list) => [
        ...list,
        {
          id: String(Date.now()),
          name: this.formName,
          type: this.formType,
          config,
          enabled: true,
        },
      ]);
    }
    this.closeModal();
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingDest.set(null);
  }

  deleteDest(d: Destination): void {
    if (confirm(`¿Eliminar destino "${d.name}"?`)) {
      this.destinations.update((list) => list.filter((x) => x.id !== d.id));
    }
  }

  typeLabel(type: DestinationType): string {
    return type === 's3' ? 'Amazon S3' : 'Google Drive';
  }

  typeBadgeClass(type: DestinationType): string {
    return type === 's3' ? 'bg-warning text-dark' : 'bg-info';
  }

  get modalTitle(): string {
    return this.editingDest() ? 'Editar destino' : 'Nuevo destino';
  }
}
