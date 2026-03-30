/** Respuesta envuelta por el middleware de la API (.NET): { message, details }. */
export interface ApiDetailsWrapper<T> {
  message?: string;
  details?: T;
}

export function unwrapApiDetails<T>(res: unknown): T {
  if (res !== null && typeof res === 'object' && 'details' in res) {
    const d = (res as ApiDetailsWrapper<T>).details;
    if (d !== undefined && d !== null) return d;
  }
  return res as T;
}
