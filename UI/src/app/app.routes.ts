import { Component } from '@angular/core';
import { Routes } from '@angular/router';
import { DashboardLayoutComponent } from './layout/dashboard-layout.component';
import { HomeComponent } from './pages/home/home.component';
import { JobsComponent } from './pages/jobs/jobs.component';
import { DestinationsComponent } from './pages/destinations/destinations.component';
import { ScriptsComponent } from './pages/scripts/scripts.component';
import { SettingsComponent } from './pages/settings/settings.component';
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
      { path: 'destinos', component: DestinationsComponent },
      { path: 'scripts', component: ScriptsComponent },
      { path: 'configuracion', component: SettingsComponent },
    ],
  },
];
