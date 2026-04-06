import { NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import {
  CreateDestinoPayload,
  DestinationsService,
  DestinoRow,
  DestinationType,
  tipoDestinoUiToApi,
  UpdateDestinoPayload,
} from '../../services/destinations.service';
import { messageFromHttpError } from '../../utils/http-error.util';
import { ToastService } from '../../services/toast.service';

/** Tipos disponibles en el formulario (sin Google Drive). */
type DestinoFormType = 's3' | 'azure_blob';

@Component({
  selector: 'app-destinations',
  standalone: true,
  imports: [FormsModule, NgClass],
  templateUrl: './destinations.component.html',
})
export class DestinationsComponent implements OnInit {
  readonly destSvc = inject(DestinationsService);
  private readonly toast = inject(ToastService);

  /** Quita barras finales del prefijo guardado en API para mostrarlo en el formulario. */
  private static carpetaDestinoToForm(stored: string | undefined): string {
    return (stored ?? '').trim().replace(/\/+$/, '');
  }

  showModal = signal(false);
  editingDest = signal<DestinoRow | null>(null);
  saving = signal(false);

  formName = '';
  formType: DestinoFormType = 's3';
  formBucket = '';
  formRegion = 'us-east-1';
  formAccessKeyId = '';
  formSecretAccessKey = '';
  formAzureContainer = '';
  formAzureConnectionString = '';
  /** Prefijo dentro del bucket (S3) o del contenedor (Azure); sin barra final en el formulario. */
  formCarpetaDestino = '';

  testingConnection = signal(false);
  testResult = signal<{ success: boolean; message: string } | null>(null);

  formNameError = signal<string | null>(null);
  formBucketError = signal<string | null>(null);
  formRegionError = signal<string | null>(null);
  formAccessKeyIdError = signal<string | null>(null);
  formSecretAccessKeyError = signal<string | null>(null);
  formAzureContainerError = signal<string | null>(null);
  formAzureConnectionStringError = signal<string | null>(null);
  formCarpetaDestinoError = signal<string | null>(null);

  ngOnInit(): void {
    this.destSvc.loadAll();
  }

  openCreate(): void {
    this.editingDest.set(null);
    this.formName = '';
    this.formType = 's3';
    this.formBucket = '';
    this.formRegion = 'us-east-1';
    this.formAccessKeyId = '';
    this.formSecretAccessKey = '';
    this.formAzureContainer = '';
    this.formAzureConnectionString = '';
    this.formCarpetaDestino = '';
    this.clearFormFieldErrors();
    this.showModal.set(true);
    this.testResult.set(null);
  }

  openEdit(d: DestinoRow): void {
    if (d.type === 'google_drive') {
      this.toast.show(
        'Los destinos Google Drive ya no se configuran en la aplicación. Puedes eliminar este destino si ya no lo usas.',
        'error'
      );
      return;
    }

    this.editingDest.set(d);
    this.formName = d.name;
    this.formType = d.type === 'azure_blob' ? 'azure_blob' : 's3';
    if (d.type === 's3') {
      this.formAccessKeyId = (d.accessKeyId ?? '').trim();
      this.formBucket = d.bucketName ?? '';
      this.formRegion = (d.region ?? '').trim() ? (d.region ?? '').trim() : 'us-east-1';
      this.formAzureContainer = '';
      this.formAzureConnectionString = '';
      this.formCarpetaDestino = DestinationsComponent.carpetaDestinoToForm(d.carpetaDestino);
    } else {
      this.formAccessKeyId = '';
      this.formBucket = '';
      this.formRegion = 'us-east-1';
      this.formAzureContainer = d.azureBlobContainerName ?? '';
      this.formAzureConnectionString = '';
      this.formCarpetaDestino = DestinationsComponent.carpetaDestinoToForm(d.carpetaDestino);
    }
    this.formSecretAccessKey = '';
    this.clearFormFieldErrors();
    this.showModal.set(true);
    this.testResult.set(null);
  }

  private clearFormFieldErrors(): void {
    this.formNameError.set(null);
    this.formBucketError.set(null);
    this.formRegionError.set(null);
    this.formAccessKeyIdError.set(null);
    this.formSecretAccessKeyError.set(null);
    this.formAzureContainerError.set(null);
    this.formAzureConnectionStringError.set(null);
    this.formCarpetaDestinoError.set(null);
  }

  onFormNameInput(): void {
    if (this.formNameError()) this.formNameError.set(null);
  }

  onFormTypeChange(): void {
    this.formBucketError.set(null);
    this.formRegionError.set(null);
    this.formAccessKeyIdError.set(null);
    this.formSecretAccessKeyError.set(null);
    this.formAzureContainerError.set(null);
    this.formAzureConnectionStringError.set(null);
    this.formCarpetaDestinoError.set(null);
  }

  onFormBucketInput(): void {
    if (this.formBucketError()) this.formBucketError.set(null);
  }

  onFormRegionInput(): void {
    if (this.formRegionError()) this.formRegionError.set(null);
  }

  onFormAccessKeyIdInput(): void {
    if (this.formAccessKeyIdError()) this.formAccessKeyIdError.set(null);
  }

  onFormSecretAccessKeyInput(): void {
    if (this.formSecretAccessKeyError()) this.formSecretAccessKeyError.set(null);
  }

  onFormAzureContainerInput(): void {
    if (this.formAzureContainerError()) this.formAzureContainerError.set(null);
  }

  onFormAzureConnectionStringInput(): void {
    if (this.formAzureConnectionStringError()) this.formAzureConnectionStringError.set(null);
  }

  onFormCarpetaDestinoInput(): void {
    if (this.formCarpetaDestinoError()) this.formCarpetaDestinoError.set(null);
  }

  private validateForm(edit: DestinoRow | null, nombre: string): boolean {
    let ok = true;
    if (!nombre) {
      this.formNameError.set('El nombre es obligatorio.');
      ok = false;
    }
    if (this.formType === 'azure_blob') {
      if (!this.formAzureContainer.trim()) {
        this.formAzureContainerError.set('El nombre del contenedor es obligatorio.');
        ok = false;
      }
      if (!this.formCarpetaDestino.trim()) {
        this.formCarpetaDestinoError.set('La carpeta destino (prefijo) es obligatoria.');
        ok = false;
      }
      if (!edit) {
        if (!this.formAzureConnectionString.trim()) {
          this.formAzureConnectionStringError.set('La cadena de conexión es obligatoria.');
          ok = false;
        }
      }
    } else {
      if (!this.formCarpetaDestino.trim()) {
        this.formCarpetaDestinoError.set('La carpeta destino (prefijo) es obligatoria.');
        ok = false;
      }
      if (!this.formBucket.trim()) {
        this.formBucketError.set('El bucket es obligatorio.');
        ok = false;
      }
      if (!this.formRegion.trim()) {
        this.formRegionError.set('La región es obligatoria.');
        ok = false;
      }
      if (!edit) {
        if (!this.formAccessKeyId.trim()) {
          this.formAccessKeyIdError.set('Access Key ID es obligatorio.');
          ok = false;
        }
        if (!this.formSecretAccessKey.trim()) {
          this.formSecretAccessKeyError.set('Secret Access Key es obligatorio.');
          ok = false;
        }
      } else {
        const hadStoredAccessKey = (edit.accessKeyId ?? '').trim().length > 0;
        const accessKeyId = this.formAccessKeyId.trim();
        const sk = this.formSecretAccessKey.trim();
        if (!accessKeyId && !hadStoredAccessKey) {
          // Sin actualización de credenciales
        } else {
          if (!accessKeyId) {
            this.formAccessKeyIdError.set('Access Key ID es obligatorio para S3 con claves de acceso.');
            ok = false;
          }
          if (!sk && !edit.secretAccessKeyConfigurada && !!accessKeyId) {
            this.formSecretAccessKeyError.set(
              'Secret Access Key es obligatorio al configurar claves por primera vez.'
            );
            ok = false;
          }
        }
      }
    }
    return ok;
  }

  testConnection(): void {
    this.testResult.set(null);
    if (this.formType === 's3') {
      const bucket = this.formBucket?.trim();
      const region = this.formRegion?.trim();
      if (!bucket || !region) {
        this.testResult.set({
          success: false,
          message: 'Completa Bucket y Región.',
        });
        return;
      }
      if (!this.formCarpetaDestino?.trim()) {
        this.testResult.set({
          success: false,
          message: 'Indica la carpeta destino (prefijo dentro del bucket).',
        });
        return;
      }
      const accessKey = this.formAccessKeyId?.trim();
      const secretKey = this.formSecretAccessKey?.trim();
      if (!accessKey || !secretKey) {
        this.testResult.set({
          success: false,
          message: 'Completa Access Key ID y Secret Access Key.',
        });
        return;
      }

      this.testingConnection.set(true);
      const s3Body = {
        bucketName: bucket,
        region,
        accessKeyId: accessKey,
        secretAccessKey: this.formSecretAccessKey,
      };

      this.destSvc
        .validarS3(s3Body)
        .pipe(finalize(() => this.testingConnection.set(false)))
        .subscribe({
          next: (r) => {
            this.testResult.set({ success: true, message: r.mensaje });
          },
          error: (err: unknown) => {
            this.testResult.set({ success: false, message: messageFromHttpError(err) });
          },
        });
      return;
    }

    const container = this.formAzureContainer?.trim();
    const conn = this.formAzureConnectionString?.trim();
    if (!container) {
      this.testResult.set({ success: false, message: 'Completa el nombre del contenedor.' });
      return;
    }
    if (!this.formCarpetaDestino?.trim()) {
      this.testResult.set({
        success: false,
        message: 'Indica la carpeta destino (prefijo dentro del contenedor).',
      });
      return;
    }
    if (!conn) {
      this.testResult.set({
        success: false,
        message:
          this.editingDest() && !conn
            ? 'Para probar la conexión, pega de nuevo la cadena de conexión (no se muestra al editar por seguridad).'
            : 'Completa la cadena de conexión de Azure Storage.',
      });
      return;
    }
    this.testingConnection.set(true);
    this.destSvc
      .validarAzureBlob({ azureBlobContainerName: container, azureBlobConnectionString: this.formAzureConnectionString })
      .pipe(finalize(() => this.testingConnection.set(false)))
      .subscribe({
        next: (r) => this.testResult.set({ success: true, message: r.mensaje }),
        error: (err: unknown) => this.testResult.set({ success: false, message: messageFromHttpError(err) }),
      });
  }

  save(): void {
    const nombre = this.formName.trim();
    this.clearFormFieldErrors();
    const edit = this.editingDest();
    if (!this.validateForm(edit, nombre)) return;

    if (!edit) {
      const createPayload = this.tryBuildCreatePayload(nombre);
      this.saving.set(true);
      this.destSvc
        .create(createPayload)
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: () => this.closeModal(),
        });
      return;
    }

    const updatePayload = this.tryBuildUpdatePayload(nombre, edit);

    this.saving.set(true);
    this.destSvc
      .update(edit.id, updatePayload)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => this.closeModal(),
      });
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingDest.set(null);
    this.clearFormFieldErrors();
  }

  deleteDest(d: DestinoRow): void {
    if (!confirm(`¿Eliminar destino "${d.name}"?`)) return;
    this.destSvc.deleteById(d.id).subscribe();
  }

  typeLabel(type: DestinationType): string {
    if (type === 's3') return 'Amazon S3';
    if (type === 'azure_blob') return 'Azure Blob Storage';
    return 'Google Drive';
  }

  typeBadgeClass(type: DestinationType): string {
    if (type === 's3') return 'bg-warning text-dark';
    if (type === 'azure_blob') return 'bg-primary';
    return 'bg-info';
  }

  get modalTitle(): string {
    return this.editingDest() ? 'Editar destino' : 'Nuevo destino';
  }

  private tryBuildCreatePayload(nombre: string): CreateDestinoPayload {
    const tipoApi = tipoDestinoUiToApi(this.formType as DestinationType);
    if (this.formType === 'azure_blob') {
      return {
        nombre,
        tipoApi,
        azureBlobContainerName: this.formAzureContainer.trim(),
        azureBlobConnectionString: this.formAzureConnectionString.trim(),
        carpetaDestino: this.formCarpetaDestino.trim(),
      };
    }
    return {
      nombre,
      tipoApi,
      bucketName: this.formBucket.trim(),
      region: this.formRegion.trim(),
      accessKeyId: this.formAccessKeyId.trim(),
      secretAccessKey: this.formSecretAccessKey.trim(),
      carpetaDestino: this.formCarpetaDestino.trim(),
    };
  }

  private tryBuildUpdatePayload(nombre: string, edit: DestinoRow): UpdateDestinoPayload {
    const payload: UpdateDestinoPayload = {
      nombre,
      tipoApi: tipoDestinoUiToApi(this.formType as DestinationType),
    };

    if (this.formType === 'azure_blob') {
      const c = this.formAzureContainer.trim();
      if (c !== (edit.azureBlobContainerName ?? '')) payload.azureBlobContainerName = c;
      const cs = this.formAzureConnectionString.trim();
      if (cs) payload.azureBlobConnectionString = cs;
      payload.carpetaDestino = this.formCarpetaDestino.trim();
      return payload;
    }

    payload.bucketName = this.formBucket.trim();
    payload.region = this.formRegion.trim();
    payload.carpetaDestino = this.formCarpetaDestino.trim();

    const hadStoredAccessKey = (edit.accessKeyId ?? '').trim().length > 0;
    const accessKeyId = this.formAccessKeyId.trim();
    const sk = this.formSecretAccessKey.trim();

    if (!accessKeyId && !hadStoredAccessKey) {
      return payload;
    }
    payload.accessKeyId = accessKeyId;
    if (sk) payload.secretAccessKey = sk;
    return payload;
  }
}
