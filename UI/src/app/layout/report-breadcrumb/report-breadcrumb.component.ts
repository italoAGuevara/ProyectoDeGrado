import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

export interface BreadcrumbItem {
  label: string;
  link?: string;
}

@Component({
  selector: 'app-report-breadcrumb',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './report-breadcrumb.component.html',
  styleUrl: './report-breadcrumb.component.css',
})
export class ReportBreadcrumbComponent {
  /** Items: último sin link = página actual */
  items = input.required<BreadcrumbItem[]>();
}
