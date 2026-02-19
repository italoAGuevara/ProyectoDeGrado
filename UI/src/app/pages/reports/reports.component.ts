import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReportBreadcrumbComponent, BreadcrumbItem } from '../../layout/report-breadcrumb/report-breadcrumb.component';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, RouterLink, ReportBreadcrumbComponent],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css',
})
export class ReportsComponent {
  breadcrumbItems: BreadcrumbItem[] = [{ label: 'Reportes' }];
}
