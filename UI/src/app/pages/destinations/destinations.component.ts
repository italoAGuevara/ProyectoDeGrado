import { NgClass } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

type DestinationType = 's3' | 'google_drive';
type S3ConfigMode = 'bucket_region' | 'credentials';

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
  formS3Mode: S3ConfigMode = 'bucket_region';
  formBucket = '';
  formRegion = 'us-east-1';
  formAccessKeyId = '';
  formSecretAccessKey = '';
  formFolderId = '';
  /** Google Drive: Service Account (client_email del JSON de la cuenta de servicio) */
  formGoogleServiceAccountEmail = '';
  /** Google Drive: clave privada (private_key del JSON de la cuenta de servicio) */
  formGooglePrivateKey = '';

  testingConnection = signal(false);
  testResult = signal<{ success: boolean; message: string } | null>(null);

  openCreate(): void {
    this.editingDest.set(null);
    this.formName = '';
    this.formType = 's3';
    this.formS3Mode = 'bucket_region';
    this.formBucket = '';
    this.formRegion = 'us-east-1';
    this.formAccessKeyId = '';
    this.formSecretAccessKey = '';
    this.formFolderId = '';
    this.formGoogleServiceAccountEmail = '';
    this.formGooglePrivateKey = '';
    this.showModal.set(true);
    this.testResult.set(null);
  }

  openEdit(d: Destination): void {
    this.editingDest.set(d);
    this.formName = d.name;
    this.formType = d.type;
    if (d.type === 's3') {
      const hasCredentials = !!(d.config['accessKeyId'] ?? '').trim();
      this.formS3Mode = hasCredentials ? 'credentials' : 'bucket_region';
      this.formBucket = d.config['bucket'] ?? '';
      this.formRegion = d.config['region'] ?? 'us-east-1';
      this.formAccessKeyId = d.config['accessKeyId'] ?? '';
      this.formSecretAccessKey = d.config['secretAccessKey'] ?? '';
    } else {
      this.formFolderId = d.config['folderId'] ?? '';
      this.formGoogleServiceAccountEmail = d.config['serviceAccountEmail'] ?? '';
      this.formGooglePrivateKey = d.config['privateKey'] ?? '';
    }
    this.showModal.set(true);
    this.testResult.set(null);
  }

  testConnection(): void {
    this.testResult.set(null);
    if (this.formType === 's3') {
      if (this.formS3Mode === 'bucket_region') {
        const bucket = this.formBucket?.trim();
        const region = this.formRegion?.trim();
        if (!bucket || !region) {
          this.testResult.set({
            success: false,
            message: 'Completa Bucket y Región.',
          });
          return;
        }
      } else {
        const accessKey = this.formAccessKeyId?.trim();
        const secretKey = this.formSecretAccessKey?.trim();
        if (!accessKey || !secretKey) {
          this.testResult.set({
            success: false,
            message: 'Completa Access Key ID y Secret Access Key.',
          });
          return;
        }
      }
    } else {
      const folderId = this.formFolderId?.trim();
      const email = this.formGoogleServiceAccountEmail?.trim();
      const key = this.formGooglePrivateKey?.trim();
      if (!folderId) {
        this.testResult.set({ success: false, message: 'Completa el ID de carpeta.' });
        return;
      }
      if (!email || !key) {
        this.testResult.set({ success: false, message: 'Completa el correo de la Service Account y la clave privada.' });
        return;
      }
    }
    this.testingConnection.set(true);
    // Simula llamada al backend; reemplazar por servicio real cuando exista
    setTimeout(() => {
      this.testingConnection.set(false);
      if (this.formType === 's3') {
        this.testResult.set({
          success: true,
          message: 'Conexión con S3 verificada.',
        });
      } else {
        this.testResult.set({
          success: true,
          message: 'Conexión con Google Drive verificada.',
        });
      }
    }, 1200);
  }

  save(): void {
    const edit = this.editingDest();
    const config: Record<string, string> =
      this.formType === 's3'
        ? this.formS3Mode === 'bucket_region'
          ? { bucket: this.formBucket, region: this.formRegion }
          : { accessKeyId: this.formAccessKeyId, secretAccessKey: this.formSecretAccessKey }
        : {
            folderId: this.formFolderId,
            serviceAccountEmail: this.formGoogleServiceAccountEmail,
            privateKey: this.formGooglePrivateKey,
          };

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
