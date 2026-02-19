import { Routes } from '@angular/router';
import { DashboardLayoutComponent } from './layout/dashboard-layout.component';
import { HomeComponent } from './pages/home/home.component';
import { JobsComponent } from './pages/jobs/jobs.component';
import { DestinationsComponent } from './pages/destinations/destinations.component';
import { ScriptsComponent } from './pages/scripts/scripts.component';

export const routes: Routes = [
  {
    path: '',
    component: DashboardLayoutComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'trabajos', component: JobsComponent },
      { path: 'destinos', component: DestinationsComponent },
      { path: 'scripts', component: ScriptsComponent },
    ],
  },
];
