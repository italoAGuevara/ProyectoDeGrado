import { Component } from '@angular/core';
import { Routes } from '@angular/router';
import { DashboardLayoutComponent } from './layout/dashboard-layout.component';
import { HomeComponent } from './pages/home/home.component';
import { JobsComponent } from './pages/jobs/jobs.component';
import { JobWizardComponent } from './pages/jobs/job-wizard/job-wizard.component';
import { DestinationsComponent } from './pages/destinations/destinations.component';
import { ScriptsComponent } from './pages/scripts/scripts.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { ReportsComponent } from './pages/reports/reports.component';
import { JobReportComponent } from './pages/reports/job-report/job-report.component';
import { DestinationExecutionsComponent } from './pages/reports/destination-executions/destination-executions.component';
import { LogsReportComponent } from './pages/reports/logs-report/logs-report.component';
import { LoginComponent } from './pages/login/login.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    component: DashboardLayoutComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'trabajos', component: JobsComponent },
      { path: 'trabajos/nuevo', component: JobWizardComponent },
      { path: 'destinos', component: DestinationsComponent },
      { path: 'scripts', component: ScriptsComponent },
      { path: 'reportes/trabajos', component: JobReportComponent },
      { path: 'reportes/destinos', component: DestinationExecutionsComponent },
      { path: 'reportes/logs', component: LogsReportComponent },
      { path: 'reportes', component: ReportsComponent },
      { path: 'configuracion', component: SettingsComponent },
    ],
  },
];
