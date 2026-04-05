import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { unwrapApiDetails } from '../utils/api-response.util';
import { messageFromHttpError } from '../utils/http-error.util';
import { ToastService } from './toast.service';

export type DestinationType = 's3' | 'google_drive' | 'azure_blob';

export interface DestinoRow {
  id: number;
  name: string;
  type: DestinationType;
  idCarpeta: string;
  accessKeyId: string;
  secretAccessKeyConfigurada: boolean;
  bucketName: string;
  region: string;
  serviceAccountEmail: string;
  privateKeyConfigurada: boolean;
  azureBlobContainerName: string;
  azureBlobConnectionStringConfigurada: boolean;
  carpetaDestino: string;
}

interface DestinoApiDto {
  id: number;
  nombre: string;
  tipo: string;
  idCarpeta: string;
  accessKeyId: string;
  secretAccessKeyConfigurada: boolean;
  bucketName: string;
  region: string;
  serviceAccountEmail: string;
  privateKeyConfigurada: boolean;
  azureBlobContainerName?: string;
  azureBlobConnectionStringConfigurada?: boolean;
  carpetaDestino?: string;
}

const API_DESTINOS = '/api/destinos';

export function tipoDestinoApiToUi(tipo: string): DestinationType {
  if (tipo === 'GoogleDrive') return 'google_drive';
  if (tipo === 'AzureBlob') return 'azure_blob';
  return 's3';
}

export function tipoDestinoUiToApi(t: DestinationType): string {
  if (t === 'google_drive') return 'GoogleDrive';
  if (t === 'azure_blob') return 'AzureBlob';
  return 'S3';
}

export interface CreateDestinoPayload {
  nombre: string;
  tipoApi: string;
  idCarpeta?: string;
  bucketName?: string;
  region?: string;
  accessKeyId?: string;
  secretAccessKey?: string;
  serviceAccountEmail?: string;
  privateKey?: string;
  azureBlobContainerName?: string;
  azureBlobConnectionString?: string;
  carpetaDestino?: string;
}

export interface UpdateDestinoPayload {
  nombre?: string;
  tipoApi?: string;
  idCarpeta?: string;
  bucketName?: string;
  region?: string;
  accessKeyId?: string;
  secretAccessKey?: string;
  serviceAccountEmail?: string;
  privateKey?: string;
  azureBlobContainerName?: string;
  azureBlobConnectionString?: string;
  carpetaDestino?: string;
}

export interface GoogleDriveValidacionDto {
  mensaje: string;
  nombreCarpeta?: string | null;
}

export interface S3ValidacionDto {
  mensaje: string;
  bucket?: string | null;
  identityArn?: string | null;
}

export interface AzureBlobValidacionDto {
  mensaje: string;
  containerName?: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class DestinationsService {
  private readonly http = inject(HttpClient);
  private readonly toast = inject(ToastService);

  destinations = signal<DestinoRow[]>([]);
  readonly loading = signal(false);

  loadAll(): void {
    this.loading.set(true);
    this.http.get<unknown>(API_DESTINOS).subscribe({
      next: (res) => {
        const raw = unwrapApiDetails<DestinoApiDto[]>(res);
        const list = Array.isArray(raw) ? raw : [];
        this.destinations.set(list.map((d) => this.fromApi(d)));
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.show(messageFromHttpError(err), 'error');
      },
    });
  }

  create(payload: CreateDestinoPayload): Observable<DestinoRow> {
    const body = this.toCreateBody(payload);

    return this.http.post<unknown>(API_DESTINOS, body).pipe(
      map((res) => this.fromApi(unwrapApiDetails<DestinoApiDto>(res))),
      tap((created) => {
        this.destinations.update((list) => [...list, created].sort((a, b) => a.id - b.id));
        this.toast.show('Destino creado', 'success');
      }),
      catchError((err) => {
        this.toast.show(messageFromHttpError(err), 'error');
        return throwError(() => err);
      })
    );
  }

  update(id: number, payload: UpdateDestinoPayload): Observable<DestinoRow> {
    const body = this.toUpdateBody(payload);

    return this.http.put<unknown>(`${API_DESTINOS}/${id}`, body).pipe(
      map((res) => this.fromApi(unwrapApiDetails<DestinoApiDto>(res))),
      tap((updated) => {
        this.destinations.update((list) => list.map((d) => (d.id === updated.id ? updated : d)));
        this.toast.show('Destino actualizado', 'success');
      }),
      catchError((err) => {
        this.toast.show(messageFromHttpError(err), 'error');
        return throwError(() => err);
      })
    );
  }

  /** Valida en la API credenciales y acceso a la carpeta (Google Drive API). */
  validarGoogleDrive(body: {
    idCarpeta: string;
    serviceAccountEmail: string;
    privateKey: string;
  }): Observable<GoogleDriveValidacionDto> {
    return this.http
      .post<unknown>(`${API_DESTINOS}/validar-google-drive`, {
        idCarpeta: body.idCarpeta.trim(),
        serviceAccountEmail: body.serviceAccountEmail.trim(),
        privateKey: body.privateKey,
      })
      .pipe(
        map((res) => unwrapApiDetails<GoogleDriveValidacionDto>(res)),
        catchError((err) => throwError(() => err))
      );
  }

  /**
   * Prueba acceso al bucket con HeadBucket.
   * Sin accessKey/secret usa la cadena de credenciales del servidor (modo IAM).
   */
  validarS3(body: {
    bucketName: string;
    region: string;
    accessKeyId?: string;
    secretAccessKey?: string;
  }): Observable<S3ValidacionDto> {
    const payload: Record<string, string> = {
      bucketName: body.bucketName.trim(),
      region: body.region.trim(),
    };
    if (body.accessKeyId !== undefined && body.secretAccessKey !== undefined) {
      payload['accessKeyId'] = body.accessKeyId.trim();
      payload['secretAccessKey'] = body.secretAccessKey;
    }
    return this.http.post<unknown>(`${API_DESTINOS}/validar-s3`, payload).pipe(
      map((res) => unwrapApiDetails<S3ValidacionDto>(res)),
      catchError((err) => throwError(() => err))
    );
  }

  deleteById(id: number): Observable<void> {
    return this.http.delete(`${API_DESTINOS}/${id}`).pipe(
      catchError((err: unknown) => {
        this.toast.show(messageFromHttpError(err), 'error');
        return throwError(() => err);
      }),
      tap(() => {
        this.destinations.update((list) => list.filter((d) => d.id !== id));
        this.toast.show('Destino eliminado', 'success');
      }),
      map(() => void 0)
    );
  }

  private toCreateBody(p: CreateDestinoPayload): Record<string, unknown> {
    const body: Record<string, unknown> = {
      nombre: p.nombre.trim(),
      tipo: p.tipoApi,
    };
    this.assignIfDefined(body, 'idCarpeta', p.idCarpeta);
    this.assignIfDefined(body, 'bucketName', p.bucketName);
    this.assignIfDefined(body, 'region', p.region);
    this.assignIfDefined(body, 'accessKeyId', p.accessKeyId);
    this.assignIfDefined(body, 'secretAccessKey', p.secretAccessKey);
    this.assignIfDefined(body, 'serviceAccountEmail', p.serviceAccountEmail);
    this.assignIfDefined(body, 'privateKey', p.privateKey);
    this.assignIfDefined(body, 'azureBlobContainerName', p.azureBlobContainerName);
    this.assignIfDefined(body, 'azureBlobConnectionString', p.azureBlobConnectionString);
    this.assignIfDefined(body, 'carpetaDestino', p.carpetaDestino);
    return body;
  }

  private toUpdateBody(p: UpdateDestinoPayload): Record<string, unknown> {
    const body: Record<string, unknown> = {};
    if (p.nombre !== undefined) body['nombre'] = p.nombre.trim();
    if (p.tipoApi !== undefined) body['tipo'] = p.tipoApi;
    this.assignIfDefined(body, 'idCarpeta', p.idCarpeta);
    this.assignIfDefined(body, 'bucketName', p.bucketName);
    this.assignIfDefined(body, 'region', p.region);
    this.assignIfDefined(body, 'accessKeyId', p.accessKeyId);
    this.assignIfDefined(body, 'secretAccessKey', p.secretAccessKey);
    this.assignIfDefined(body, 'serviceAccountEmail', p.serviceAccountEmail);
    this.assignIfDefined(body, 'privateKey', p.privateKey);
    this.assignIfDefined(body, 'azureBlobContainerName', p.azureBlobContainerName);
    this.assignIfDefined(body, 'azureBlobConnectionString', p.azureBlobConnectionString);
    this.assignIfDefined(body, 'carpetaDestino', p.carpetaDestino);
    return body;
  }

  validarAzureBlob(body: {
    azureBlobContainerName: string;
    azureBlobConnectionString: string;
  }): Observable<AzureBlobValidacionDto> {
    return this.http
      .post<unknown>(`${API_DESTINOS}/validar-azure-blob`, {
        azureBlobContainerName: body.azureBlobContainerName.trim(),
        azureBlobConnectionString: body.azureBlobConnectionString,
      })
      .pipe(
        map((res) => unwrapApiDetails<AzureBlobValidacionDto>(res)),
        catchError((err) => throwError(() => err))
      );
  }

  private assignIfDefined(body: Record<string, unknown>, key: string, value: string | undefined): void {
    if (value !== undefined && value !== '') body[key] = value;
  }

  private fromApi(d: DestinoApiDto): DestinoRow {
    return {
      id: d.id,
      name: d.nombre,
      type: tipoDestinoApiToUi(d.tipo),
      idCarpeta: d.idCarpeta ?? '',
      accessKeyId: d.accessKeyId ?? '',
      secretAccessKeyConfigurada: !!d.secretAccessKeyConfigurada,
      bucketName: d.bucketName ?? '',
      region: d.region ?? '',
      serviceAccountEmail: d.serviceAccountEmail ?? '',
      privateKeyConfigurada: !!d.privateKeyConfigurada,
      azureBlobContainerName: d.azureBlobContainerName ?? '',
      azureBlobConnectionStringConfigurada: !!d.azureBlobConnectionStringConfigurada,
      carpetaDestino: d.carpetaDestino ?? '',
    };
  }
}
