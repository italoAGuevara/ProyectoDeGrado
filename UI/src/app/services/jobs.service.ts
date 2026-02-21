import { Injectable, signal } from '@angular/core';

export interface JobScript {
  scriptId: string;
  when: 'pre' | 'post';
}

export interface Job {
  id: string;
  name: string;
  description?: string;
  destinationName: string;
  schedule: string;
  enabled: boolean;
  scripts: JobScript[];
  // Filters
  excludeHidden?: boolean;
  excludeSystem?: boolean;
  excludeTemporary?: boolean;
  excludeLargerThanMB?: number | null;
  customFilters?: string;
}

@Injectable({
  providedIn: 'root',
})
export class JobsService {
  jobs = signal<Job[]>([
    {
      id: '1',
      name: 'Backup diario documentos',
      description: 'Respaldo de mis documentos importantes',
      destinationName: 'S3 principal',
      schedule: '0 2 * * *',
      enabled: true,
      scripts: [{ scriptId: '1', when: 'pre' }],
    },
  ]);

  getJob(id: string): Job | undefined {
    return this.jobs().find((j) => j.id === id);
  }

  addJob(job: Job): void {
    this.jobs.update((list) => [...list, job]);
  }

  updateJob(job: Job): void {
    this.jobs.update((list) => list.map((j) => (j.id === job.id ? job : j)));
  }

  deleteJob(id: string): void {
    this.jobs.update((list) => list.filter((j) => j.id !== id));
  }
}
